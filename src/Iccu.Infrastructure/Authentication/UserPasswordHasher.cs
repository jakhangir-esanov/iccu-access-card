namespace Iccu.Infrastructure.Authentication;

using Iccu.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Iccu.Application.Abstractions.Authentication;

internal sealed class UserPasswordHasher : IPasswordHasher
{
    private const string TimingEqualizerPassword = "timing-equalizer-password";

    private static readonly PasswordHasher<User> Hasher = new();

    private static readonly Lazy<string> TimingEqualizerHash =
        new(() => Hasher.HashPassword(null!, TimingEqualizerPassword));

    public string Hash(string password) => Hasher.HashPassword(null!, password);

    public bool Verify(string passwordHash, string password)
    {
        PasswordVerificationResult result = Hasher.VerifyHashedPassword(null!, passwordHash, password);

        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }

    public void SimulateVerification(string password)
    {
        Hasher.VerifyHashedPassword(null!, TimingEqualizerHash.Value, password);
    }
}
