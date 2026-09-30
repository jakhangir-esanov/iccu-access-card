namespace Iccu.Application.RegistrationRequests.ApproveRegistrationRequest;

using Iccu.Domain.Common;
using Iccu.Domain.Readers;
using System.Globalization;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Common.Clock;
using Iccu.Domain.RegistrationRequests;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Data;
using Iccu.Application.Abstractions.Authentication;

public sealed record ApproveRegistrationRequestCommand(Guid Id) : ICommand<ApproveRegistrationRequestResponse>;

public sealed record ApproveRegistrationRequestResponse(Guid ReaderId, string CardNumber);

internal sealed class ApproveRegistrationRequestCommandHandler(
    IUnitOfWork unitOfWork,
    IRegistrationRequestRepository registrationRequestRepository,
    IReaderRepository readerRepository,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<ApproveRegistrationRequestCommand, ApproveRegistrationRequestResponse>
{
    private const int CardValidityYears = 2;
    private const string CardNumberFormat = "D7";

    public async Task<Result<ApproveRegistrationRequestResponse>> Handle(
        ApproveRegistrationRequestCommand request,
        CancellationToken cancellationToken)
    {
        RegistrationRequest? registrationRequest = await registrationRequestRepository.GetAsync(request.Id, cancellationToken);
        if (registrationRequest is null)
        {
            return Result.Failure<ApproveRegistrationRequestResponse>(RegistrationRequestErrors.NotFound);
        }

        DateTime utcNow = dateTimeProvider.UtcNow;

        if (registrationRequest.Status != RegistrationRequestStatus.Pending)
        {
            return Result.Failure<ApproveRegistrationRequestResponse>(RegistrationRequestErrors.NotPending);
        }

        if (registrationRequest.ExpiresAt <= utcNow)
        {
            return Result.Failure<ApproveRegistrationRequestResponse>(RegistrationRequestErrors.Expired);
        }

        if (await readerRepository.IsPhoneRegisteredAsync(registrationRequest.Phone, cancellationToken: cancellationToken))
        {
            return Result.Failure<ApproveRegistrationRequestResponse>(ReaderErrors.PhoneAlreadyRegistered);
        }

        DateOnly today = dateTimeProvider.Today;

        var reader = Reader.Register(
            registrationRequest.Details,
            registrationRequest.PhotoFileId,
            RegistrationSource.SelfService,
            today,
            today.AddYears(CardValidityYears),
            utcNow,
            currentUser.UserId);

        registrationRequest.MarkApproved(reader.Id, utcNow, currentUser.UserId);
        readerRepository.Insert(reader);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApproveRegistrationRequestResponse(
            reader.Id,
            reader.CardNumber.ToString(CardNumberFormat, CultureInfo.InvariantCulture));
    }
}
