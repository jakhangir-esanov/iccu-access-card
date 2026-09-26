namespace Iccu.Presentation.Authentication.Requests;


internal sealed record ChangeMyPasswordRequest
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
