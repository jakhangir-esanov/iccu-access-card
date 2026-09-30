namespace Iccu.Domain.Readers;

public interface IReaderRepository
{
    Task<Reader?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> IsPhoneRegisteredAsync(
        string phone,
        Guid? exceptReaderId = null,
        CancellationToken cancellationToken = default);

    void Insert(Reader reader);
}
