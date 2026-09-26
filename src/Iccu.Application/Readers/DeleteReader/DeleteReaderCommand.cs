namespace Iccu.Application.Readers.DeleteReader;

using Iccu.Domain.Common;
using Iccu.Domain.Readers;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Data;

public sealed record DeleteReaderCommand(Guid Id) : ICommand;

internal sealed class DeleteReaderCommandHandler(
    IUnitOfWork unitOfWork,
    IReaderRepository readerRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<DeleteReaderCommand>
{
    public async Task<Result> Handle(DeleteReaderCommand request, CancellationToken cancellationToken)
    {
        Reader? reader = await readerRepository.GetAsync(request.Id, cancellationToken);
        if (reader is null)
        {
            return Result.Failure(ReaderErrors.NotFound);
        }

        if (reader.DeletedAt is not null)
        {
            return Result.Failure(ReaderErrors.AlreadyDeleted);
        }

        reader.MarkDeleted(dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
