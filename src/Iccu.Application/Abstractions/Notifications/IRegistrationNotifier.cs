namespace Iccu.Application.Abstractions.Notifications;

public interface IRegistrationNotifier
{
    Task NotifySubmittedAsync(RegistrationSubmittedNotice notice, CancellationToken cancellationToken = default);
}

public sealed record RegistrationSubmittedNotice(Guid Id, string Code, string FullName, DateTime SubmittedAt);
