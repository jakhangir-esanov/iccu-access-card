namespace Iccu.Presentation.Authentication;

using Iccu.Application.Authentication;

internal sealed record AccessTokenResponse(string AccessToken, DateTime ExpiresAt, UserProfileResponse User)
{
    public static AccessTokenResponse From(AuthSession session) =>
        new(session.AccessToken, session.AccessTokenExpiresAt, session.User);
}
