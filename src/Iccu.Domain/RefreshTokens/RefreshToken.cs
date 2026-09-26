namespace Iccu.Domain.RefreshTokens;

public sealed class RefreshToken
{
    private RefreshToken()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public static RefreshToken Issue(Guid userId, string tokenHash, DateTime utcNow, DateTime expiresAt)
    {
        return new RefreshToken
        {
            Id = Guid.CreateVersion7(utcNow),
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAt = utcNow,
            ExpiresAt = expiresAt
        };
    }

    public void Revoke(DateTime utcNow)
    {
        RevokedAt = utcNow;
    }
}
