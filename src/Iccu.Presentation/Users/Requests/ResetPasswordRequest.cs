namespace Iccu.Presentation.Users.Requests;


internal sealed record ResetPasswordRequest
{
    public string NewPassword { get; init; } = string.Empty;
}
