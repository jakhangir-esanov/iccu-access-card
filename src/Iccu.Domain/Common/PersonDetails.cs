namespace Iccu.Domain.Common;

using Iccu.Domain.Common.Enums;

public sealed record PersonDetails(
    ReaderCategory Category,
    string LastName,
    string FirstName,
    string? MiddleName,
    DateOnly BirthDate,
    string Phone,
    DocumentType DocumentType,
    string DocumentNumber);
