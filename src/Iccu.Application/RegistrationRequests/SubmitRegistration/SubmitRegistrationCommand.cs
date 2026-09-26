namespace Iccu.Application.RegistrationRequests.SubmitRegistration;

using Iccu.Domain.Common;
using System.Globalization;
using Iccu.Domain.StoredFiles;
using Iccu.Application.Common.Clock;
using Iccu.Domain.RegistrationRequests;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Common.Validation;
using Iccu.Application.Abstractions.Data;
using Iccu.Application.Abstractions.Notifications;

public sealed record SubmitRegistrationCommand(
    PersonDetails Details,
    Guid PhotoFileId,
    bool ConsentGiven) : ICommand<SubmitRegistrationResponse>;

public sealed record SubmitRegistrationResponse(string Code, DateTime ExpiresAt);

internal sealed class SubmitRegistrationCommandHandler(
    IUnitOfWork unitOfWork,
    IRegistrationRequestRepository registrationRequestRepository,
    IStoredFileRepository storedFileRepository,
    IRegistrationNotifier registrationNotifier,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<SubmitRegistrationCommand, SubmitRegistrationResponse>
{
    private const string CodeFormat = "D4";

    private static readonly TimeSpan PendingLifetime = TimeSpan.FromHours(24);

    public async Task<Result<SubmitRegistrationResponse>> Handle(
        SubmitRegistrationCommand request,
        CancellationToken cancellationToken)
    {
        if (await storedFileRepository.GetAsync(request.PhotoFileId, cancellationToken) is null)
        {
            return Result.Failure<SubmitRegistrationResponse>(StoredFileErrors.NotFound);
        }

        DateTime utcNow = dateTimeProvider.UtcNow;

        var registrationRequest = RegistrationRequest.Submit(
            request.Details.Normalized(),
            request.PhotoFileId,
            utcNow,
            utcNow.Add(PendingLifetime));

        registrationRequestRepository.Insert(registrationRequest);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        string code = registrationRequest.Code.ToString(CodeFormat, CultureInfo.InvariantCulture);

        await registrationNotifier.NotifySubmittedAsync(
            new RegistrationSubmittedNotice(registrationRequest.Id, code, $"{registrationRequest.LastName} {registrationRequest.FirstName}", registrationRequest.SubmittedAt),
            cancellationToken);

        return new SubmitRegistrationResponse(code, registrationRequest.ExpiresAt);
    }
}
