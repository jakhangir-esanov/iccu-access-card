namespace Iccu.Infrastructure.Authentication;

using Microsoft.AspNetCore.Http;
using Iccu.Application.Abstractions.Authentication;

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid UserId => httpContextAccessor.HttpContext?.User.GetUserId() ??
                      throw new InvalidOperationException("User identifier is unavailable");
}
