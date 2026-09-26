namespace Iccu.Application.Authentication.GetMe;

using Dapper;
using Iccu.Domain.Users;
using Iccu.Domain.Common;
using Iccu.Application.Common.Data;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Authentication;

public sealed record GetMeQuery : IQuery<UserProfileResponse>;

internal sealed class GetMeQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ICurrentUser currentUser) : IQueryHandler<GetMeQuery, UserProfileResponse>
{
    public async Task<Result<UserProfileResponse>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var sql =
            $"""
            SELECT
                id AS {nameof(UserProfileResponse.Id)},
                username AS {nameof(UserProfileResponse.Username)},
                full_name AS {nameof(UserProfileResponse.FullName)},
                role AS {nameof(UserProfileResponse.Role)}
            FROM iccu.users
            WHERE id = @UserId AND is_active
            """;

        UserProfileResponse? profile = await connection.QuerySingleOrDefaultAsync<UserProfileResponse>(
            new CommandDefinition(sql, new { currentUser.UserId }, cancellationToken: cancellationToken));

        return profile is null
            ? Result.Failure<UserProfileResponse>(UserErrors.NotFound)
            : profile;
    }
}
