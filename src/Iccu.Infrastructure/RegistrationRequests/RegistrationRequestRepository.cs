namespace Iccu.Infrastructure.RegistrationRequests;

using Iccu.Domain.Common.Enums;
using Iccu.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Iccu.Domain.RegistrationRequests;

internal sealed class RegistrationRequestRepository(ApplicationDbContext dbContext) : IRegistrationRequestRepository
{
    public async Task<RegistrationRequest?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.RegistrationRequests.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<RegistrationRequest>> GetStalePendingAsync(
        DateTime utcNow,
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.RegistrationRequests
            .Where(x => x.Status == RegistrationRequestStatus.Pending && x.ExpiresAt <= utcNow)
            .OrderBy(x => x.ExpiresAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public void Insert(RegistrationRequest request)
    {
        dbContext.RegistrationRequests.Add(request);
    }
}
