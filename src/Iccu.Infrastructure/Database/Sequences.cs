namespace Iccu.Infrastructure.Database;

internal static class Sequences
{
    internal const string CardNumber = "card_number_seq";
    internal const string RegistrationCode = "registration_code_seq";

    internal static string NextValue(string sequence) => $"nextval('{Schemas.Iccu}.{sequence}')";
}
