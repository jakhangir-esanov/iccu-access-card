namespace Iccu.Domain.Users;

using Iccu.Domain.Common.Enums;

public sealed class User
{
    private User()
    {
    }

    public Guid Id { get; private set; }

    public string Username { get; private set; } = null!;

    public string FullName { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public int FailedLoginAttempts { get; private set; }

    public DateTime? LockedUntil { get; private set; }

    public DateTime? LastLoginAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static User Create(
        string username,
        string fullName,
        UserRole role,
        string passwordHash,
        DateTime utcNow)
    {
        return new User
        {
            Id = Guid.CreateVersion7(utcNow),
            Username = username,
            FullName = fullName,
            Role = role,
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = utcNow
        };
    }

    public void Update(string fullName, UserRole role, bool isActive)
    {
        FullName = fullName;
        Role = role;
        IsActive = isActive;
    }

    public void ChangePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
    }

    public void LockUntil(DateTime lockedUntil)
    {
        LockedUntil = lockedUntil;
        FailedLoginAttempts = 0;
    }

    public void ClearLoginFailures()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }

    public void RecordSuccessfulLogin(DateTime utcNow)
    {
        LastLoginAt = utcNow;
    }
}
