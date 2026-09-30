namespace Iccu.Application.Readers.ExportReaders;

using Iccu.Domain.Common.Enums;

internal sealed record ReaderExportRow(
    string CardNumber,
    string LastName,
    string FirstName,
    string? MiddleName,
    ReaderCategory Category,
    DateOnly BirthDate,
    Gender? Gender,
    Citizenship? Citizenship,
    string Phone,
    DateOnly IssuedOn,
    DateOnly ExpiresOn,
    RegistrationSource Source,
    int PrintCount,
    string? CreatedByName);
