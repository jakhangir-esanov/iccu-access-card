namespace Iccu.Application.Readers.ExportReaders;

using System.Globalization;
using Iccu.Domain.Common.Enums;

internal static class ReaderExportSheet
{
    public const string SheetName = "Kitobxonlar";

    private const string DateFormat = "dd.MM.yyyy";

    public static readonly string[] Header =
    [
        "Karta raqami",
        "Familiya",
        "Ism",
        "Otasining ismi",
        "Toifa",
        "Tug'ilgan sana",
        "Telefon",
        "Hujjat turi",
        "Hujjat raqami",
        "Berilgan sana",
        "Amal qilish muddati",
        "Holati",
        "Manba",
        "Chop etilgan",
        "Ro'yxatga olgan xodim"
    ];

    public static string[] ToCells(ReaderExportRow row, DateOnly today)
    {
        return
        [
            row.CardNumber,
            row.LastName,
            row.FirstName,
            row.MiddleName ?? string.Empty,
            CategoryName(row.Category),
            FormatDate(row.BirthDate),
            row.Phone,
            row.DocumentType == DocumentType.Passport ? "Pasport / ID karta" : "Tug'ilganlik guvohnomasi",
            row.DocumentNumber,
            FormatDate(row.IssuedOn),
            FormatDate(row.ExpiresOn),
            row.ExpiresOn < today ? "Muddati o'tgan" : "Faol",
            row.Source == RegistrationSource.Reception ? "Resepshn" : "QR anketa",
            row.PrintCount.ToString(CultureInfo.InvariantCulture),
            row.CreatedByName ?? string.Empty
        ];
    }

    private static string CategoryName(ReaderCategory category) => category switch
    {
        ReaderCategory.Pupil => "O'quvchi",
        ReaderCategory.Student => "Talaba",
        ReaderCategory.Master => "Magistr",
        ReaderCategory.PhD => "PhD",
        ReaderCategory.DSc => "DSc",
        ReaderCategory.Professor => "Professor",
        ReaderCategory.Employee => "Xizmatchi",
        _ => category.ToString()
    };

    private static string FormatDate(DateOnly date) => date.ToString(DateFormat, CultureInfo.InvariantCulture);
}
