namespace Iccu.Application.Common.Paging;

public sealed class PagedList<T>
{
    public PagedList(IEnumerable<T> data, int totalCount)
    {
        Data = data;
        TotalCount = totalCount;
    }

    public IEnumerable<T> Data { get; init; }
    public int TotalCount { get; init; }
}
