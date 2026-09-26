namespace Iccu.Application.Common.Paging;

using Iccu.Application.Common.Extensions;

public sealed class PagingRequest<T>
{
    private const string DefaultSortField = "id";
    private const int DefaultRows = 10;
    private const int MaxRows = 1000;

    private static readonly HashSet<string> SortableColumns =
        SortColumnExtensions.GetSortableColumns<T>();

    public PagingRequest(int first, int rows, string sortField, int sortOrder)
    {
        First = first;
        Rows = rows;
        SortField = sortField;
        SortOrder = sortOrder;
    }

    private int _first;
    public int First
    {
        get => _first;
        init => _first = value < 0 ? 0 : value;
    }

    private int _rows = DefaultRows;
    public int Rows
    {
        get => _rows;
        init
        {
            if (value <= 0)
            {
                _rows = DefaultRows;
            }
            else if (value > MaxRows)
            {
                _rows = MaxRows;
            }
            else
            {
                _rows = value;
            }
        }
    }

    private string _sortField = DefaultSortField;
    public string SortField
    {
        get => _sortField;
        init => _sortField = SortableColumns.Contains(value.ToLowerInvariant())
                ? value.ToLowerInvariant()
                : DefaultSortField;
    }

    private int _sortOrder = 1;
    public int SortOrder
    {
        get => _sortOrder;
        init => _sortOrder = value is 1 or -1 ? value : 1;
    }

    public string SortDirection => SortOrder == -1 ? "desc" : "asc";
}
