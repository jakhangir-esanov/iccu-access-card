namespace Iccu.UnitTests.Fakes;

using System.Data.Common;
using Iccu.Domain.Users;
using Iccu.Application.Abstractions.Storage;
using Iccu.Application.Abstractions.Authentication;
using Iccu.Application.Abstractions.Data;

internal sealed class FakeCurrentUser(Guid id) : ICurrentUser
{
    public Guid UserId { get; } = id;
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Exception? FailWith { get; set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (FailWith is not null)
        {
            throw FailWith;
        }

        SaveCount++;
        return Task.FromResult(0);
    }

    public Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}

internal sealed class FakeFileStore : IFileStore
{
    public Dictionary<string, byte[]> Files { get; } = [];

    public List<string> Deleted { get; } = [];

    public async Task SaveAsync(string storagePath, Stream content, CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        Files[storagePath] = buffer.ToArray();
    }

    public Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default) =>
        Task.FromResult<Stream?>(Files.TryGetValue(storagePath, out byte[]? content) ? new MemoryStream(content) : null);

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        Files.Remove(storagePath);
        Deleted.Add(storagePath);
        return Task.CompletedTask;
    }
}

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public int SimulatedVerifications { get; private set; }

    public string Hash(string password) => $"hashed:{password}";

    public bool Verify(string passwordHash, string password) => passwordHash == Hash(password);

    public void SimulateVerification(string password) => SimulatedVerifications++;
}

internal sealed class FakeTokenService : ITokenService
{
    private int _sequence;

    public IssuedTokens Issue(User user, DateTime utcNow)
    {
        string refreshToken = $"refresh-{++_sequence}";

        return new IssuedTokens(
            $"access-for-{user.Username}",
            utcNow.AddMinutes(15),
            refreshToken,
            HashRefreshToken(refreshToken),
            utcNow.AddDays(7));
    }

    public string HashRefreshToken(string refreshToken) => $"hash:{refreshToken}";
}
