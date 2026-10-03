namespace Iccu.Presentation.Koha.Requests;

internal sealed record GetKohaReadersRequest
{
    public int? First { get; init; }
    public int? Rows { get; init; }
}
