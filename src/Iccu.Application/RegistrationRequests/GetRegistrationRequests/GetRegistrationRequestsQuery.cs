namespace Iccu.Application.RegistrationRequests.GetRegistrationRequests;

using Dapper;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Common.Data;
using Iccu.Application.Common.Paging;
using Iccu.Application.Common.Messaging;

public sealed record GetRegistrationRequestsQuery(
    PagingRequest<RegistrationRequestListItemResponse> Paging,
    RegistrationRequestStatus? Status = null,
    string? Search = null) : IPagedListQuery<RegistrationRequestListItemResponse>;

internal sealed class GetRegistrationRequestsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IPagedListQueryHandler<GetRegistrationRequestsQuery, RegistrationRequestListItemResponse>
{
    private const string RequestsCte =
        """
        WITH requests AS (
            SELECT
                q.id,
                q.photo_file_id,
                lpad(q.code::text, 4, '0') AS code,
                q.status,
                q.category,
                q.last_name,
                q.first_name,
                q.middle_name,
                q.phone,
                q.submitted_at,
                q.expires_at,
                q.reviewed_at,
                reviewer.full_name AS reviewed_by_name,
                EXISTS (
                    SELECT 1
                    FROM iccu.readers existing
                    WHERE existing.document_type = q.document_type
                      AND existing.document_number = q.document_number
                      AND existing.deleted_at IS NULL) AS has_registered_document
            FROM iccu.registration_requests q
            LEFT JOIN iccu.users reviewer ON reviewer.id = q.reviewed_by
        )
        """;

    public async Task<PagedList<RegistrationRequestListItemResponse>> Handle(
        GetRegistrationRequestsQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var paging = request.Paging;

        var conditions = new List<string>();

        if (request.Status is not null)
        {
            conditions.Add("status = @Status");
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add("(code = @Code OR last_name ILIKE @Search OR first_name ILIKE @Search OR phone LIKE @Search)");
        }

        string whereSql = conditions.Count == 0 ? string.Empty : $"WHERE {string.Join(" AND ", conditions)}";

        var dataSql =
            $"""
            {RequestsCte}
            SELECT
                id AS {nameof(RegistrationRequestListItemResponse.Id)},
                photo_file_id AS {nameof(RegistrationRequestListItemResponse.PhotoFileId)},
                code AS {nameof(RegistrationRequestListItemResponse.Code)},
                status AS {nameof(RegistrationRequestListItemResponse.Status)},
                category AS {nameof(RegistrationRequestListItemResponse.Category)},
                last_name AS {nameof(RegistrationRequestListItemResponse.LastName)},
                first_name AS {nameof(RegistrationRequestListItemResponse.FirstName)},
                middle_name AS {nameof(RegistrationRequestListItemResponse.MiddleName)},
                phone AS {nameof(RegistrationRequestListItemResponse.Phone)},
                submitted_at AS {nameof(RegistrationRequestListItemResponse.SubmittedAt)},
                expires_at AS {nameof(RegistrationRequestListItemResponse.ExpiresAt)},
                reviewed_at AS {nameof(RegistrationRequestListItemResponse.ReviewedAt)},
                reviewed_by_name AS {nameof(RegistrationRequestListItemResponse.ReviewedByName)},
                has_registered_document AS {nameof(RegistrationRequestListItemResponse.HasRegisteredDocument)}
            FROM requests
            {whereSql}
            ORDER BY {paging.SortField} {paging.SortDirection}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var countSql =
            $"""
            {RequestsCte}
            SELECT COUNT(*)
            FROM requests
            {whereSql};
            """;

        var param = new
        {
            Status = (int?)request.Status,
            Code = request.Search?.Trim(),
            Search = string.IsNullOrWhiteSpace(request.Search) ? null : $"%{request.Search.Trim()}%",
            Offset = paging.First,
            PageSize = paging.Rows
        };

        var items = (await connection.QueryAsync<RegistrationRequestListItemResponse>(
            new CommandDefinition(dataSql, param, cancellationToken: cancellationToken))).AsList();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, param, cancellationToken: cancellationToken));

        return new PagedList<RegistrationRequestListItemResponse>(items, totalCount);
    }
}
