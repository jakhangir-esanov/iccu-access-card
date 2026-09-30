namespace Iccu.Presentation.Readers.Requests;

using Iccu.Domain.Common.Enums;

internal sealed record ExportReadersRequest
{
    public string? Search { get; init; }
    public ReaderCategory? Category { get; init; }
    public RegistrationSource? Source { get; init; }
    public CardStatus? Status { get; init; }
    public Gender? Gender { get; init; }
    public Citizenship? Citizenship { get; init; }
    public DateOnly? RegisteredFrom { get; init; }
    public DateOnly? RegisteredTo { get; init; }
}
