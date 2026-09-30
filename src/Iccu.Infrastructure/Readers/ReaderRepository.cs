namespace Iccu.Infrastructure.Readers;

using Iccu.Domain.Readers;
using Iccu.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

internal sealed class ReaderRepository(ApplicationDbContext dbContext) : IReaderRepository
{
    public async Task<Reader?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Readers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> IsPhoneRegisteredAsync(
        string phone,
        Guid? exceptReaderId = null,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Readers.AnyAsync(
            x => x.Phone == phone && x.Id != exceptReaderId,
            cancellationToken);
    }

    public void Insert(Reader reader)
    {
        dbContext.Readers.Add(reader);
    }
}
