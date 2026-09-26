namespace Iccu.Application.Common.Validation;

using Iccu.Domain.Common.Enums;
using System.Text.RegularExpressions;

public static partial class DocumentNumber
{
    private static readonly char[] Separators = [' ', '-', '№', '#', '.'];

    public static string Normalize(string? documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return string.Empty;
        }

        return new string([.. documentNumber
            .ToUpperInvariant()
            .Where(character => !Separators.Contains(character) && !char.IsWhiteSpace(character))]);
    }

    public static bool IsValid(DocumentType type, string? documentNumber)
    {
        string normalized = Normalize(documentNumber);

        return type switch
        {
            DocumentType.Passport => PassportPattern().IsMatch(normalized),
            DocumentType.BirthCertificate => BirthCertificatePattern().IsMatch(normalized),
            _ => false
        };
    }

    [GeneratedRegex(@"^[A-Z]{2}\d{7}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex PassportPattern();

    [GeneratedRegex(@"^(?=.*\d)[\p{Lu}\d]{6,20}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex BirthCertificatePattern();
}
