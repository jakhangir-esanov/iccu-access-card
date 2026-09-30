namespace Iccu.Presentation.Readers.Requests;

using Iccu.Domain.Common.Enums;

internal sealed record UpdateReaderRequest
{
    public ReaderCategory Category { get; init; }
    public string LastName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string? MiddleName { get; init; }
    public DateOnly BirthDate { get; init; }
    public Gender? Gender { get; init; }
    public Citizenship? Citizenship { get; init; }
    public string Phone { get; init; } = string.Empty;
    public Guid PhotoFileId { get; init; }
}
