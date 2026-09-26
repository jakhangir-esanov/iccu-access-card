namespace Iccu.Application.Readers.GetReader;

using Iccu.Domain.Common.Enums;

public sealed record ReaderResponse(
    Guid Id,
    Guid PhotoFileId,
    string CardNumber,
    ReaderCategory Category,
    string LastName,
    string FirstName,
    string? MiddleName,
    DateOnly BirthDate,
    string Phone,
    DocumentType DocumentType,
    string DocumentNumber,
    RegistrationSource Source,
    DateOnly IssuedOn,
    DateOnly ExpiresOn,
    bool IsExpired,
    int PrintCount,
    DateTime? LastPrintedAt,
    DateTime CreatedAt,
    string? CreatedByName,
    DateTime? UpdatedAt);
