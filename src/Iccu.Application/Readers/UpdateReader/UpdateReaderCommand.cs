namespace Iccu.Application.Readers.UpdateReader;

using Iccu.Domain.Common;
using Iccu.Domain.Readers;
using Iccu.Domain.StoredFiles;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Common.Validation;
using Iccu.Application.Abstractions.Data;

public sealed record UpdateReaderCommand(Guid Id, PersonDetails Details, Guid PhotoFileId) : ICommand;

internal sealed class UpdateReaderCommandHandler(
    IUnitOfWork unitOfWork,
    IReaderRepository readerRepository,
    IStoredFileRepository storedFileRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateReaderCommand>
{
    public async Task<Result> Handle(UpdateReaderCommand request, CancellationToken cancellationToken)
    {
        Reader? reader = await readerRepository.GetAsync(request.Id, cancellationToken);
        if (reader is null)
        {
            return Result.Failure(ReaderErrors.NotFound);
        }

        PersonDetails details = request.Details.Normalized();

        if (await readerRepository.IsDocumentRegisteredAsync(
                details.DocumentType, details.DocumentNumber, reader.Id, cancellationToken))
        {
            return Result.Failure(ReaderErrors.DocumentAlreadyRegistered);
        }

        DateTime utcNow = dateTimeProvider.UtcNow;

        reader.UpdateDetails(details, utcNow);

        if (request.PhotoFileId != reader.PhotoFileId)
        {
            if (await storedFileRepository.GetAsync(request.PhotoFileId, cancellationToken) is null)
            {
                return Result.Failure(StoredFileErrors.NotFound);
            }

            reader.ReplacePhoto(request.PhotoFileId, utcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
