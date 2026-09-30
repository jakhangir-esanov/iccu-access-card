namespace Iccu.Presentation.Readers.Requests;

using Iccu.Domain.Common.Enums;

internal sealed record GetReadersRequest
{
    public int? First { get; init; }
    public int? Rows { get; init; }
    public string? SortField { get; init; }
    public int? SortOrder { get; init; }
    public string? Search { get; init; }
    public ReaderCategory? Category { get; init; }
    public RegistrationSource? Source { get; init; }
    public CardStatus? Status { get; init; }
    public Gender? Gender { get; init; }
    public Citizenship? Citizenship { get; init; }
    public DateOnly? RegisteredFrom { get; init; }
    public DateOnly? RegisteredTo { get; init; }
}
