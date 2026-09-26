namespace Iccu.Presentation.RegistrationRequests.Requests;

using Iccu.Domain.Common.Enums;

internal sealed record GetRegistrationRequestsRequest
{
    public int? First { get; init; }
    public int? Rows { get; init; }
    public string? SortField { get; init; }
    public int? SortOrder { get; init; }
    public RegistrationRequestStatus? Status { get; init; }
    public string? Search { get; init; }
}
