namespace Iccu.Application.Readers.GetReaders;

using Iccu.Domain.Common.Enums;

public sealed record ReaderListItemResponse(
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
    string DocumentNumberMasked,
    RegistrationSource Source,
    DateOnly IssuedOn,
    DateOnly ExpiresOn,
    bool IsExpired,
    int PrintCount,
    DateTime CreatedAt);
