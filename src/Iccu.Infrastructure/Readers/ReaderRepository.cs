namespace Iccu.Infrastructure.Readers;

using Iccu.Domain.Readers;
using Iccu.Domain.Common.Enums;
using Iccu.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

internal sealed class ReaderRepository(ApplicationDbContext dbContext) : IReaderRepository
{
    public async Task<Reader?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Readers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> IsDocumentRegisteredAsync(
        DocumentType documentType,
        string documentNumber,
        Guid? exceptReaderId = null,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Readers.AnyAsync(
            x => x.DocumentType == documentType &&
                 x.DocumentNumber == documentNumber &&
                 x.Id != exceptReaderId,
            cancellationToken);
    }

    public void Insert(Reader reader)
    {
        dbContext.Readers.Add(reader);
    }
}
