namespace Iccu.Domain.Readers;

using Iccu.Domain.Common.Enums;

public interface IReaderRepository
{
    Task<Reader?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> IsDocumentRegisteredAsync(
        DocumentType documentType,
        string documentNumber,
        Guid? exceptReaderId = null,
        CancellationToken cancellationToken = default);

    void Insert(Reader reader);
}
