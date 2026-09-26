namespace Iccu.Application.Users.GetUsers;

using Iccu.Domain.Common.Enums;

public sealed record UserResponse(
    Guid Id,
    string Username,
    string FullName,
    UserRole Role,
    bool IsActive,
    bool IsLockedOut,
    DateTime? LastLoginAt,
    DateTime CreatedAt);
