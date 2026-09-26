namespace Iccu.Presentation.Authentication.Requests;


internal sealed record LoginRequest
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
