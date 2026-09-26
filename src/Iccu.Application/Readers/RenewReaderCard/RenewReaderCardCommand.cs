namespace Iccu.Application.Readers.RenewReaderCard;

using Iccu.Domain.Common;
using Iccu.Domain.Readers;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Data;

public sealed record RenewReaderCardCommand(Guid Id) : ICommand<RenewReaderCardResponse>;

public sealed record RenewReaderCardResponse(DateOnly IssuedOn, DateOnly ExpiresOn);

internal sealed class RenewReaderCardCommandHandler(
    IUnitOfWork unitOfWork,
    IReaderRepository readerRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RenewReaderCardCommand, RenewReaderCardResponse>
{
    private const int CardValidityYears = 2;

    public async Task<Result<RenewReaderCardResponse>> Handle(
        RenewReaderCardCommand request,
        CancellationToken cancellationToken)
    {
        Reader? reader = await readerRepository.GetAsync(request.Id, cancellationToken);
        if (reader is null)
        {
            return Result.Failure<RenewReaderCardResponse>(ReaderErrors.NotFound);
        }

        DateOnly today = dateTimeProvider.Today;

        reader.RenewCard(today, today.AddYears(CardValidityYears), dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RenewReaderCardResponse(reader.IssuedOn, reader.ExpiresOn);
    }
}
