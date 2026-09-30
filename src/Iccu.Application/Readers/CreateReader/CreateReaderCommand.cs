namespace Iccu.Application.Readers.CreateReader;

using Iccu.Domain.Common;
using Iccu.Domain.Readers;
using System.Globalization;
using Iccu.Domain.StoredFiles;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Common.Validation;
using Iccu.Application.Abstractions.Data;
using Iccu.Application.Abstractions.Authentication;

public sealed record CreateReaderCommand(PersonDetails Details, Guid PhotoFileId) : ICommand<CreateReaderResponse>;

public sealed record CreateReaderResponse(Guid Id, string CardNumber);

internal sealed class CreateReaderCommandHandler(
    IUnitOfWork unitOfWork,
    IReaderRepository readerRepository,
    IStoredFileRepository storedFileRepository,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateReaderCommand, CreateReaderResponse>
{
    private const int CardValidityYears = 2;
    private const string CardNumberFormat = "D7";

    public async Task<Result<CreateReaderResponse>> Handle(CreateReaderCommand request, CancellationToken cancellationToken)
    {
        PersonDetails details = request.Details.Normalized();

        if (await readerRepository.IsPhoneRegisteredAsync(details.Phone, cancellationToken: cancellationToken))
        {
            return Result.Failure<CreateReaderResponse>(ReaderErrors.PhoneAlreadyRegistered);
        }

        if (await storedFileRepository.GetAsync(request.PhotoFileId, cancellationToken) is null)
        {
            return Result.Failure<CreateReaderResponse>(StoredFileErrors.NotFound);
        }

        DateOnly today = dateTimeProvider.Today;

        var reader = Reader.Register(
            details,
            request.PhotoFileId,
            RegistrationSource.Reception,
            today,
            today.AddYears(CardValidityYears),
            dateTimeProvider.UtcNow,
            currentUser.UserId);

        readerRepository.Insert(reader);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateReaderResponse(
            reader.Id,
            reader.CardNumber.ToString(CardNumberFormat, CultureInfo.InvariantCulture));
    }
}
