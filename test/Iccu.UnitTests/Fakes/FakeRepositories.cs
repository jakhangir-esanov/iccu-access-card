namespace Iccu.UnitTests.Fakes;

using Iccu.Domain.Common.Enums;
using Iccu.Domain.Readers;
using Iccu.Domain.RegistrationRequests;
using Iccu.Domain.Users;
using Iccu.Domain.StoredFiles;
using Iccu.Domain.RefreshTokens;

internal sealed class FakeReaderRepository : IReaderRepository
{
    public List<Reader> Readers { get; } = [];

    public Task<Reader?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Readers.SingleOrDefault(reader => reader.Id == id && reader.DeletedAt is null));

    public Task<Reader?> GetByCardNumberAsync(int cardNumber, CancellationToken cancellationToken = default) =>
        Task.FromResult(Readers.SingleOrDefault(reader => reader.CardNumber == cardNumber && reader.DeletedAt is null));

    public Task<bool> IsPhoneRegisteredAsync(
        string phone,
        Guid? exceptReaderId = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Readers.Any(reader =>
            reader.DeletedAt is null &&
            reader.Phone == phone &&
            reader.Id != exceptReaderId));

    public void Insert(Reader reader) => Readers.Add(reader);
}

internal sealed class FakeRegistrationRequestRepository : IRegistrationRequestRepository
{
    public List<RegistrationRequest> Requests { get; } = [];

    public Task<RegistrationRequest?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Requests.SingleOrDefault(request => request.Id == id));

    public Task<IReadOnlyList<RegistrationRequest>> GetStalePendingAsync(
        DateTime utcNow,
        int limit,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RegistrationRequest>>([.. Requests
            .Where(request => request.Status == RegistrationRequestStatus.Pending && request.ExpiresAt <= utcNow)
            .Take(limit)]);

    public void Insert(RegistrationRequest request) => Requests.Add(request);
}

internal sealed class FakeUserRepository : IUserRepository
{
    public List<User> Users { get; } = [];

    public Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.SingleOrDefault(user => user.Id == id));

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.SingleOrDefault(user => user.Username == username));

    public Task<bool> IsUsernameTakenAsync(string username, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.Any(user => user.Username == username));

    public void Insert(User user) => Users.Add(user);
}

internal sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
{
    public List<RefreshToken> Tokens { get; } = [];

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        Task.FromResult(Tokens.SingleOrDefault(token => token.TokenHash == tokenHash));

    public Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(
        Guid userId,
        DateTime utcNow,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RefreshToken>>([.. Tokens
            .Where(token => token.UserId == userId && token.RevokedAt is null && token.ExpiresAt > utcNow)]);

    public void Insert(RefreshToken refreshToken) => Tokens.Add(refreshToken);
}

internal sealed class FakeStoredFileRepository : IStoredFileRepository
{
    public List<StoredFile> Files { get; } = [];

    public HashSet<Guid> UsedFileIds { get; } = [];

    public Task<StoredFile?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Files.SingleOrDefault(file => file.Id == id));

    public Task<bool> IsInUseAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(UsedFileIds.Contains(id));

    public Task<IReadOnlyList<StoredFile>> GetUnusedAsync(
        DateTime createdBefore,
        int limit,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<StoredFile>>([.. Files
            .Where(file => file.CreatedAt < createdBefore && !UsedFileIds.Contains(file.Id))
            .Take(limit)]);

    public void Insert(StoredFile storedFile) => Files.Add(storedFile);

    public void Remove(StoredFile storedFile) => Files.Remove(storedFile);
}
