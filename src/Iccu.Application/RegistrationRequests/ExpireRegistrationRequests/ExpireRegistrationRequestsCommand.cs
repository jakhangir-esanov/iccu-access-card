namespace Iccu.Application.RegistrationRequests.ExpireRegistrationRequests;

using Iccu.Domain.Common;
using Iccu.Application.Common.Clock;
using Iccu.Domain.RegistrationRequests;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Data;

public sealed record ExpireRegistrationRequestsCommand : ICommand<int>;

internal sealed class ExpireRegistrationRequestsCommandHandler(
    IUnitOfWork unitOfWork,
    IRegistrationRequestRepository registrationRequestRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ExpireRegistrationRequestsCommand, int>
{
    private const int BatchSize = 200;

    public async Task<Result<int>> Handle(ExpireRegistrationRequestsCommand request, CancellationToken cancellationToken)
    {
        IReadOnlyList<RegistrationRequest> staleRequests = await registrationRequestRepository.GetStalePendingAsync(
            dateTimeProvider.UtcNow, BatchSize, cancellationToken);

        if (staleRequests.Count == 0)
        {
            return 0;
        }

        foreach (RegistrationRequest registrationRequest in staleRequests)
        {
            registrationRequest.MarkExpired();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return staleRequests.Count;
    }
}
