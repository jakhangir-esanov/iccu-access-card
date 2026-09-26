namespace Iccu.Infrastructure.Authentication;

using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        string? userId =
            principal?.FindFirst(CustomClaims.Sub)?.Value ??
            principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userId, out Guid parsedUserId) ?
            parsedUserId :
            throw new InvalidOperationException("User identifier is unavailable");
    }

    public static Guid? GetUserIdOrNull(this ClaimsPrincipal? principal)
    {
        string? userId =
            principal?.FindFirst(CustomClaims.Sub)?.Value ??
            principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userId, out Guid parsedUserId) ? parsedUserId : null;
    }
}
