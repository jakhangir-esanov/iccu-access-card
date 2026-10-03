namespace Iccu.Application.Koha.GetKohaReaders;

using Dapper;
using Iccu.Application.Common.Data;
using Iccu.Application.Common.Paging;
using Iccu.Application.Common.Messaging;

public sealed record GetKohaReadersQuery(PagingRequest<KohaReaderResponse> Paging) : IPagedListQuery<KohaReaderResponse>;

internal sealed class GetKohaReadersQueryHandler(
    IDbConnectionFactory dbConnectionFactory) : IPagedListQueryHandler<GetKohaReadersQuery, KohaReaderResponse>
{
    internal const string ReadersCte =
        """
        WITH readers AS (
            SELECT
                lpad(r.card_number::text, 7, '0') AS card_number,
                r.category,
                r.last_name,
                r.first_name,
                r.middle_name,
                r.birth_date,
                r.gender,
                r.citizenship,
                r.phone,
                r.issued_on,
                r.expires_on
            FROM iccu.readers r
            WHERE r.deleted_at IS NULL
              AND NOT r.is_koha_synced
        )
        """;

    public async Task<PagedList<KohaReaderResponse>> Handle(GetKohaReadersQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var paging = request.Paging;

        var dataSql =
            $"""
            {ReadersCte}
            SELECT
                card_number AS {nameof(KohaReaderResponse.CardNumber)},
                category AS {nameof(KohaReaderResponse.Category)},
                last_name AS {nameof(KohaReaderResponse.LastName)},
                first_name AS {nameof(KohaReaderResponse.FirstName)},
                middle_name AS {nameof(KohaReaderResponse.MiddleName)},
                birth_date AS {nameof(KohaReaderResponse.BirthDate)},
                gender AS {nameof(KohaReaderResponse.Gender)},
                citizenship AS {nameof(KohaReaderResponse.Citizenship)},
                phone AS {nameof(KohaReaderResponse.Phone)},
                issued_on AS {nameof(KohaReaderResponse.IssuedOn)},
                expires_on AS {nameof(KohaReaderResponse.ExpiresOn)}
            FROM readers
            ORDER BY {paging.SortField} {paging.SortDirection}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var countSql =
            $"""
            {ReadersCte}
            SELECT COUNT(*)
            FROM readers;
            """;

        var param = new
        {
            Offset = paging.First,
            PageSize = paging.Rows
        };

        var items = (await connection.QueryAsync<KohaReaderResponse>(
            new CommandDefinition(dataSql, param, cancellationToken: cancellationToken))).AsList();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, param, cancellationToken: cancellationToken));

        return new PagedList<KohaReaderResponse>(items, totalCount);
    }
}
