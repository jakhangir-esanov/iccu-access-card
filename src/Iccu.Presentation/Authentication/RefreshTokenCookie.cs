namespace Iccu.Presentation.Authentication;

using Microsoft.AspNetCore.Http;
using Iccu.Application.Authentication;

internal static class RefreshTokenCookie
{
    public const string Name = "iccu_refresh";

    private const string Path = "/api/auth";

    public static string? Read(HttpRequest request) => request.Cookies[Name];

    public static void Write(HttpResponse response, AuthSession session)
    {
        response.Cookies.Append(Name, session.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = Path,
            Expires = session.RefreshTokenExpiresAt,
            IsEssential = true
        });
    }

    public static void Clear(HttpResponse response)
    {
        response.Cookies.Delete(Name, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = Path
        });
    }
}
