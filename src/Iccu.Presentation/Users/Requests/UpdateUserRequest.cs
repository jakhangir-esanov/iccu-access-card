namespace Iccu.Presentation.Users.Requests;

using Iccu.Domain.Common.Enums;

internal sealed record UpdateUserRequest
{
    public string FullName { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public bool IsActive { get; init; }
}
