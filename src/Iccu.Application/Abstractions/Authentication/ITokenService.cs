namespace Iccu.Application.Abstractions.Authentication;

using Iccu.Domain.Users;

public interface ITokenService
{
    IssuedTokens Issue(User user, DateTime utcNow);

    string HashRefreshToken(string refreshToken);
}

public sealed record IssuedTokens(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    string RefreshTokenHash,
    DateTime RefreshTokenExpiresAt);
