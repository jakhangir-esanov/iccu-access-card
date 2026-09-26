namespace Iccu.Application.RegistrationRequests.RejectRegistrationRequest;

using Iccu.Domain.Common;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Common.Clock;
using Iccu.Domain.RegistrationRequests;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Data;
using Iccu.Application.Abstractions.Authentication;

public sealed record RejectRegistrationRequestCommand(Guid Id, string Reason) : ICommand;

internal sealed class RejectRegistrationRequestCommandHandler(
    IUnitOfWork unitOfWork,
    IRegistrationRequestRepository registrationRequestRepository,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RejectRegistrationRequestCommand>
{
    public async Task<Result> Handle(RejectRegistrationRequestCommand request, CancellationToken cancellationToken)
    {
        RegistrationRequest? registrationRequest = await registrationRequestRepository.GetAsync(request.Id, cancellationToken);
        if (registrationRequest is null)
        {
            return Result.Failure(RegistrationRequestErrors.NotFound);
        }

        if (registrationRequest.Status != RegistrationRequestStatus.Pending)
        {
            return Result.Failure(RegistrationRequestErrors.NotPending);
        }

        registrationRequest.MarkRejected(request.Reason.Trim(), dateTimeProvider.UtcNow, currentUser.UserId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
