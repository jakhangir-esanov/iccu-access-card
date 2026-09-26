namespace Iccu.Application.RegistrationRequests.GetRegistrationRequest;

using Iccu.Domain.Common.Enums;

public sealed record RegistrationRequestResponse(
    Guid Id,
    Guid PhotoFileId,
    string Code,
    RegistrationRequestStatus Status,
    ReaderCategory Category,
    string LastName,
    string FirstName,
    string? MiddleName,
    DateOnly BirthDate,
    string Phone,
    DocumentType DocumentType,
    string DocumentNumber,
    DateTime SubmittedAt,
    DateTime ExpiresAt,
    DateTime? ReviewedAt,
    string? ReviewedByName,
    string? RejectionReason,
    Guid? ReaderId,
    Guid? RegisteredReaderId,
    string? RegisteredReaderCardNumber);
