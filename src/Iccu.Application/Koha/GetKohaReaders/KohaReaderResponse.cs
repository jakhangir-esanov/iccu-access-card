namespace Iccu.Application.Koha.GetKohaReaders;

using Iccu.Domain.Common.Enums;

public sealed record KohaReaderResponse(
    string CardNumber,
    ReaderCategory Category,
    string LastName,
    string FirstName,
    string? MiddleName,
    DateOnly BirthDate,
    Gender? Gender,
    Citizenship? Citizenship,
    string Phone,
    DateOnly IssuedOn,
    DateOnly ExpiresOn);
