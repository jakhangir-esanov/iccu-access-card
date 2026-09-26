namespace Iccu.Application.Common.Paging;

public interface IPagingRequest : ISortingRequest
{
    int First { get; init; }
    int Rows { get; init; }
}
