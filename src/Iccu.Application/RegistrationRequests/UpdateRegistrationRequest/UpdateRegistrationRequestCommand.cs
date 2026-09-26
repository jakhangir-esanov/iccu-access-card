namespace Iccu.Application.RegistrationRequests.UpdateRegistrationRequest;

using Iccu.Domain.Common;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Common.Clock;
using Iccu.Domain.RegistrationRequests;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Common.Validation;
using Iccu.Application.Abstractions.Data;

public sealed record UpdateRegistrationRequestCommand(Guid Id, PersonDetails Details) : ICommand;

internal sealed class UpdateRegistrationRequestCommandHandler(
    IUnitOfWork unitOfWork,
    IRegistrationRequestRepository registrationRequestRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateRegistrationRequestCommand>
{
    public async Task<Result> Handle(UpdateRegistrationRequestCommand request, CancellationToken cancellationToken)
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

        if (registrationRequest.ExpiresAt <= dateTimeProvider.UtcNow)
        {
            return Result.Failure(RegistrationRequestErrors.Expired);
        }

        registrationRequest.UpdateDetails(request.Details.Normalized());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
