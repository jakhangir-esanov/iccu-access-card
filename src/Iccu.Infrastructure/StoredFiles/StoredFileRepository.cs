namespace Iccu.Infrastructure.StoredFiles;

using Iccu.Domain.StoredFiles;
using Iccu.Domain.Common.Enums;
using Iccu.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

internal sealed class StoredFileRepository(ApplicationDbContext dbContext) : IStoredFileRepository
{
    public async Task<StoredFile?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.StoredFiles.SingleOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<bool> IsInUseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Readers.AnyAsync(r => r.PhotoFileId == id, cancellationToken) ||
               await dbContext.RegistrationRequests.AnyAsync(
                   r => r.PhotoFileId == id && r.Status == RegistrationRequestStatus.Pending,
                   cancellationToken);
    }

    public async Task<IReadOnlyList<StoredFile>> GetUnusedAsync(
        DateTime createdBefore,
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.StoredFiles
            .Where(f => f.CreatedAt < createdBefore &&
                        !dbContext.Readers.Any(r => r.PhotoFileId == f.Id) &&
                        !dbContext.RegistrationRequests.Any(r =>
                            r.PhotoFileId == f.Id && r.Status == RegistrationRequestStatus.Pending))
            .OrderBy(f => f.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public void Insert(StoredFile storedFile)
    {
        dbContext.StoredFiles.Add(storedFile);
    }

    public void Remove(StoredFile storedFile)
    {
        dbContext.StoredFiles.Remove(storedFile);
    }
}
