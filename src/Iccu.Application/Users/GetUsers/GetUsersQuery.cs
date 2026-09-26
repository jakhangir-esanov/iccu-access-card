namespace Iccu.Application.Users.GetUsers;

using Dapper;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Common.Data;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Paging;
using Iccu.Application.Common.Messaging;

public sealed record GetUsersQuery(
    PagingRequest<UserResponse> Paging,
    string? Search = null,
    UserRole? Role = null,
    bool? IsActive = null) : IPagedListQuery<UserResponse>;

internal sealed class GetUsersQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IDateTimeProvider dateTimeProvider) : IPagedListQueryHandler<GetUsersQuery, UserResponse>
{
    private const string UsersCte =
        """
        WITH users AS (
            SELECT
                u.id,
                u.username,
                u.full_name,
                u.role,
                u.is_active,
                COALESCE(u.locked_until > @UtcNow, FALSE) AS is_locked_out,
                u.last_login_at,
                u.created_at
            FROM iccu.users u
        )
        """;

    public async Task<PagedList<UserResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var paging = request.Paging;

        var conditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add("(username ILIKE @Search OR full_name ILIKE @Search)");
        }

        if (request.Role is not null)
        {
            conditions.Add("role = @Role");
        }

        if (request.IsActive is not null)
        {
            conditions.Add("is_active = @IsActive");
        }

        string whereSql = conditions.Count == 0 ? string.Empty : $"WHERE {string.Join(" AND ", conditions)}";

        var dataSql =
            $"""
            {UsersCte}
            SELECT
                id AS {nameof(UserResponse.Id)},
                username AS {nameof(UserResponse.Username)},
                full_name AS {nameof(UserResponse.FullName)},
                role AS {nameof(UserResponse.Role)},
                is_active AS {nameof(UserResponse.IsActive)},
                is_locked_out AS {nameof(UserResponse.IsLockedOut)},
                last_login_at AS {nameof(UserResponse.LastLoginAt)},
                created_at AS {nameof(UserResponse.CreatedAt)}
            FROM users
            {whereSql}
            ORDER BY {paging.SortBy} {paging.SortDirection}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var countSql =
            $"""
            {UsersCte}
            SELECT COUNT(*)
            FROM users
            {whereSql};
            """;

        var param = new
        {
            Search = string.IsNullOrWhiteSpace(request.Search) ? null : $"%{request.Search.Trim()}%",
            Role = (int?)request.Role,
            request.IsActive,
            dateTimeProvider.UtcNow,
            Offset = paging.First,
            PageSize = paging.Rows
        };

        var items = (await connection.QueryAsync<UserResponse>(
            new CommandDefinition(dataSql, param, cancellationToken: cancellationToken))).AsList();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, param, cancellationToken: cancellationToken));

        return new PagedList<UserResponse>(items, totalCount);
    }
}
