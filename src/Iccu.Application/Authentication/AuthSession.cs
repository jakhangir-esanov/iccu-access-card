namespace Iccu.Application.Authentication;

using Iccu.Domain.Users;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Abstractions.Authentication;

public sealed record AuthSession(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    UserProfileResponse User)
{
    public static AuthSession Create(IssuedTokens tokens, User user) => new(
        tokens.AccessToken,
        tokens.AccessTokenExpiresAt,
        tokens.RefreshToken,
        tokens.RefreshTokenExpiresAt,
        new UserProfileResponse(user.Id, user.Username, user.FullName, user.Role));
}

public sealed record UserProfileResponse(
    Guid Id,
    string Username,
    string FullName,
    UserRole Role);
