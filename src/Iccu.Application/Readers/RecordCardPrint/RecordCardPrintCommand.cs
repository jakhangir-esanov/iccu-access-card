namespace Iccu.Application.Readers.RecordCardPrint;

using Iccu.Domain.Common;
using Iccu.Domain.Readers;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Data;

public sealed record RecordCardPrintCommand(Guid Id) : ICommand;

internal sealed class RecordCardPrintCommandHandler(
    IUnitOfWork unitOfWork,
    IReaderRepository readerRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RecordCardPrintCommand>
{
    public async Task<Result> Handle(RecordCardPrintCommand request, CancellationToken cancellationToken)
    {
        Reader? reader = await readerRepository.GetAsync(request.Id, cancellationToken);
        if (reader is null)
        {
            return Result.Failure(ReaderErrors.NotFound);
        }

        reader.RecordPrint(dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
