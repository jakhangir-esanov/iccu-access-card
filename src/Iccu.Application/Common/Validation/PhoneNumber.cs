namespace Iccu.Application.Common.Validation;

using Iccu.Domain.Common.Enums;

public static class PhoneNumber
{
    private const string CountryCode = "998";

    private const int NationalLength = 9;
    private const int FullLength = 12;
    private const int InternationalMinLength = 8;
    private const int InternationalMaxLength = 15;

    public static string? Normalize(string? phone, Citizenship? citizenship)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        string digits = new([.. phone.Where(char.IsAsciiDigit)]);

        string? uzbekPhone = NormalizeUzbek(digits);

        if (uzbekPhone is not null || citizenship != Citizenship.Foreign)
        {
            return uzbekPhone;
        }

        return digits.Length is >= InternationalMinLength and <= InternationalMaxLength && digits[0] != '0'
            ? $"+{digits}"
            : null;
    }

    private static string? NormalizeUzbek(string digits)
    {
        if (digits.Length == NationalLength)
        {
            digits = CountryCode + digits;
        }

        return digits.Length == FullLength && digits.StartsWith(CountryCode, StringComparison.Ordinal)
            ? $"+{digits}"
            : null;
    }
}
