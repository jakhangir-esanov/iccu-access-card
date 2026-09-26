namespace Iccu.Domain.RegistrationRequests;

public interface IRegistrationRequestRepository
{
    Task<RegistrationRequest?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RegistrationRequest>> GetStalePendingAsync(
        DateTime utcNow,
        int limit,
        CancellationToken cancellationToken = default);

    void Insert(RegistrationRequest request);
}
