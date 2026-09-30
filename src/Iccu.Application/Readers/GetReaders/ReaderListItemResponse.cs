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
    Gender? Gender,
    Citizenship? Citizenship,
    string Phone,
    RegistrationSource Source,
    DateOnly IssuedOn,
    DateOnly ExpiresOn,
    bool IsExpired,
    int PrintCount,
    DateTime CreatedAt);
