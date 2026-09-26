namespace Iccu.Presentation.Users.Requests;

using Iccu.Domain.Common.Enums;

internal sealed record CreateUserRequest
{
    public string Username { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public string Password { get; init; } = string.Empty;
}
