namespace Iccu.Application.RegistrationRequests.GetRegistrationRequests;

using Iccu.Domain.Common.Enums;

public sealed record RegistrationRequestListItemResponse(
    Guid Id,
    Guid PhotoFileId,
    string Code,
    RegistrationRequestStatus Status,
    ReaderCategory Category,
    string LastName,
    string FirstName,
    string? MiddleName,
    string Phone,
    DateTime SubmittedAt,
    DateTime ExpiresAt,
    DateTime? ReviewedAt,
    string? ReviewedByName,
    bool HasRegisteredDocument);
