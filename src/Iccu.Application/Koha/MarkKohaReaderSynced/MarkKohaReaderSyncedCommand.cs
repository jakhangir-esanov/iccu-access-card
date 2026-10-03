namespace Iccu.Application.Koha.MarkKohaReaderSynced;

using Iccu.Domain.Common;
using Iccu.Domain.Readers;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Data;

public sealed record MarkKohaReaderSyncedCommand(int CardNumber) : ICommand;

internal sealed class MarkKohaReaderSyncedCommandHandler(
    IUnitOfWork unitOfWork,
    IReaderRepository readerRepository) : ICommandHandler<MarkKohaReaderSyncedCommand>
{
    public async Task<Result> Handle(MarkKohaReaderSyncedCommand request, CancellationToken cancellationToken)
    {
        Reader? reader = await readerRepository.GetByCardNumberAsync(request.CardNumber, cancellationToken);
        if (reader is null)
        {
            return Result.Failure(ReaderErrors.NotFound);
        }

        reader.MarkKohaSynced();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
