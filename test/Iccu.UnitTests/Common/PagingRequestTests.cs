namespace Iccu.UnitTests.Common;

using Iccu.Application.Common.Paging;
using Iccu.Application.Readers.GetReaders;

public class PagingRequestTests
{
    [Theory]
    [InlineData(-5, 0, 0, 10)]
    [InlineData(0, 5000, 0, 1000)]
    [InlineData(40, 20, 40, 20)]
    public void Constructor_Should_ClampFirstAndRows(int first, int rows, int expectedFirst, int expectedRows)
    {
        var paging = new PagingRequest<ReaderListItemResponse>(first, rows, "card_number", 1);

        Assert.Equal(expectedFirst, paging.First);
        Assert.Equal(expectedRows, paging.Rows);
    }

    [Fact]
    public void Page_Should_BeDerivedFromFirstAndRows()
    {
        var paging = new PagingRequest<ReaderListItemResponse>(40, 20, "card_number", 1);

        Assert.Equal(3, paging.Page);
        Assert.Equal(20, paging.PageSize);
    }

    [Theory]
    [InlineData("LAST_NAME", -1, "last_name", "desc")]
    [InlineData("created_at", 7, "created_at", "asc")]
    public void SortField_ForAResponseColumn_IsUsed(string sortField, int sortOrder, string expectedField, string expectedDirection)
    {
        var paging = new PagingRequest<ReaderListItemResponse>(0, 10, sortField, sortOrder);

        Assert.Equal(expectedField, paging.SortBy);
        Assert.Equal(expectedDirection, paging.SortDirection);
    }

    [Theory]
    [InlineData("last_name; DROP TABLE iccu.readers")]
    [InlineData("search_text")]
    [InlineData("document_number")]
    public void SortField_ForAnythingButAResponseColumn_FallsBackToId(string sortField)
    {
        var paging = new PagingRequest<ReaderListItemResponse>(0, 10, sortField, 1);

        Assert.Equal("id", paging.SortBy);
    }
}
