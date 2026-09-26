namespace Iccu.Application.Common.Paging;

public interface ISortingRequest
{
    string SortField { get; init; }
    int SortOrder { get; init; }
}
