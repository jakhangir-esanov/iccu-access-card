namespace Iccu.Application.Readers.GetReaders;

using Dapper;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Common.Data;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Paging;
using Iccu.Application.Common.Messaging;

public sealed record GetReadersQuery(
    PagingRequest<ReaderListItemResponse> Paging,
    string? Search = null,
    ReaderCategory? Category = null,
    RegistrationSource? Source = null,
    CardStatus? Status = null,
    DateOnly? RegisteredFrom = null,
    DateOnly? RegisteredTo = null) : IPagedListQuery<ReaderListItemResponse>;

internal sealed class GetReadersQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IDateTimeProvider dateTimeProvider) : IPagedListQueryHandler<GetReadersQuery, ReaderListItemResponse>
{
    internal const string ReadersCte =
        """
        WITH readers AS (
            SELECT
                r.id,
                r.photo_file_id,
                lpad(r.card_number::text, 7, '0') AS card_number,
                r.card_number AS card_sequence,
                r.category,
                r.last_name,
                r.first_name,
                r.middle_name,
                r.birth_date,
                r.phone,
                r.document_type,
                r.document_number,
                left(r.document_number, 2) || repeat('*', greatest(length(r.document_number) - 6, 0)) || right(r.document_number, 4)
                    AS document_number_masked,
                r.source,
                r.issued_on,
                r.expires_on,
                r.expires_on < @Today AS is_expired,
                r.print_count,
                r.search_text,
                r.created_by,
                r.created_at
            FROM iccu.readers r
            WHERE r.deleted_at IS NULL
        )
        """;

    private const int ExpiringSoonDays = 30;

    public async Task<PagedList<ReaderListItemResponse>> Handle(GetReadersQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var paging = request.Paging;

        (string whereSql, DynamicParameters param) = BuildFilter(
            request.Search,
            request.Category,
            request.Source,
            request.Status,
            request.RegisteredFrom,
            request.RegisteredTo,
            dateTimeProvider);

        var dataSql =
            $"""
            {ReadersCte}
            SELECT
                id AS {nameof(ReaderListItemResponse.Id)},
                photo_file_id AS {nameof(ReaderListItemResponse.PhotoFileId)},
                card_number AS {nameof(ReaderListItemResponse.CardNumber)},
                category AS {nameof(ReaderListItemResponse.Category)},
                last_name AS {nameof(ReaderListItemResponse.LastName)},
                first_name AS {nameof(ReaderListItemResponse.FirstName)},
                middle_name AS {nameof(ReaderListItemResponse.MiddleName)},
                birth_date AS {nameof(ReaderListItemResponse.BirthDate)},
                phone AS {nameof(ReaderListItemResponse.Phone)},
                document_type AS {nameof(ReaderListItemResponse.DocumentType)},
                document_number_masked AS {nameof(ReaderListItemResponse.DocumentNumberMasked)},
                source AS {nameof(ReaderListItemResponse.Source)},
                issued_on AS {nameof(ReaderListItemResponse.IssuedOn)},
                expires_on AS {nameof(ReaderListItemResponse.ExpiresOn)},
                is_expired AS {nameof(ReaderListItemResponse.IsExpired)},
                print_count AS {nameof(ReaderListItemResponse.PrintCount)},
                created_at AS {nameof(ReaderListItemResponse.CreatedAt)}
            FROM readers
            {whereSql}
            ORDER BY {paging.SortBy} {paging.SortDirection}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var countSql =
            $"""
            {ReadersCte}
            SELECT COUNT(*)
            FROM readers
            {whereSql};
            """;

        param.Add("Offset", paging.First);
        param.Add("PageSize", paging.Rows);

        var items = (await connection.QueryAsync<ReaderListItemResponse>(
            new CommandDefinition(dataSql, param, cancellationToken: cancellationToken))).AsList();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, param, cancellationToken: cancellationToken));

        return new PagedList<ReaderListItemResponse>(items, totalCount);
    }

    internal static (string WhereSql, DynamicParameters Parameters) BuildFilter(
        string? search,
        ReaderCategory? category,
        RegistrationSource? source,
        CardStatus? status,
        DateOnly? registeredFrom,
        DateOnly? registeredTo,
        IDateTimeProvider dateTimeProvider)
    {
        DateOnly today = dateTimeProvider.Today;

        var conditions = new List<string>();
        var param = new DynamicParameters();

        param.Add("Today", today);
        param.Add("TimeZone", dateTimeProvider.TimeZoneId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(
                """
                (readers.search_text LIKE ALL (
                    SELECT '%' || word || '%'
                    FROM regexp_split_to_table(translate(lower(@Search::text), '‘’ʻʼ`´', ''''''''''''''), '\s+') AS word
                    WHERE word <> '')
                 OR readers.card_sequence::text = ltrim(@Search::text, '0')
                 OR (length(regexp_replace(@Search::text, '\D', '', 'g')) >= 4
                     AND readers.phone LIKE '%' || regexp_replace(@Search::text, '\D', '', 'g') || '%')
                 OR readers.document_number = upper(regexp_replace(@Search::text, '[\s\-№#.]', '', 'g')))
                """);
            param.Add("Search", search.Trim());
        }

        if (category is not null)
        {
            conditions.Add("readers.category = @Category");
            param.Add("Category", (int)category);
        }

        if (source is not null)
        {
            conditions.Add("readers.source = @Source");
            param.Add("Source", (int)source);
        }

        if (status is CardStatus.Active or CardStatus.ExpiringSoon)
        {
            conditions.Add("readers.expires_on >= @Today");
        }

        if (status is CardStatus.ExpiringSoon)
        {
            conditions.Add("readers.expires_on <= @ExpiringSoonUntil");
            param.Add("ExpiringSoonUntil", today.AddDays(ExpiringSoonDays));
        }

        if (status is CardStatus.Expired)
        {
            conditions.Add("readers.expires_on < @Today");
        }

        if (registeredFrom is not null)
        {
            conditions.Add("readers.created_at >= @RegisteredFrom::date::timestamp AT TIME ZONE @TimeZone");
            param.Add("RegisteredFrom", registeredFrom);
        }

        if (registeredTo is not null)
        {
            conditions.Add("readers.created_at < (@RegisteredTo::date + 1)::timestamp AT TIME ZONE @TimeZone");
            param.Add("RegisteredTo", registeredTo);
        }

        string whereSql = conditions.Count == 0 ? string.Empty : $"WHERE {string.Join(" AND ", conditions)}";

        return (whereSql, param);
    }
}
