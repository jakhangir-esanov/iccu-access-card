namespace Iccu.Application.Common.Validation;

public static class PhoneNumber
{
    private const string CountryCode = "998";

    private const int NationalLength = 9;
    private const int FullLength = 12;

    public static string? Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        string digits = new([.. phone.Where(char.IsAsciiDigit)]);

        if (digits.Length == NationalLength)
        {
            digits = CountryCode + digits;
        }

        return digits.Length == FullLength && digits.StartsWith(CountryCode, StringComparison.Ordinal)
            ? $"+{digits}"
            : null;
    }
}
