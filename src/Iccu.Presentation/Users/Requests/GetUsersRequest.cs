namespace Iccu.Presentation.Users.Requests;

using Iccu.Domain.Common.Enums;

internal sealed record GetUsersRequest
{
    public int? First { get; init; }
    public int? Rows { get; init; }
    public string? SortField { get; init; }
    public int? SortOrder { get; init; }
    public string? Search { get; init; }
    public UserRole? Role { get; init; }
    public bool? IsActive { get; init; }
}
