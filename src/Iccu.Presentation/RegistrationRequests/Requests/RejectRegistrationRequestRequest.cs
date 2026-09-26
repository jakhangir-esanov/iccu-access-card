namespace Iccu.Presentation.RegistrationRequests.Requests;


internal sealed record RejectRegistrationRequestRequest
{
    public string Reason { get; init; } = string.Empty;
}
