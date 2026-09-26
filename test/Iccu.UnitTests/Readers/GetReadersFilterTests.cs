namespace Iccu.UnitTests.Readers;

using Dapper;
using Iccu.UnitTests.Fakes;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Readers.GetReaders;

public class GetReadersFilterTests
{
    private readonly FakeClock _clock = new();

    private (string WhereSql, DynamicParameters Parameters) Filter(
        CardStatus? status = null,
        string? search = null,
        ReaderCategory? category = null) =>
        GetReadersQueryHandler.BuildFilter(search, category, null, status, null, null, _clock);

    [Fact]
    public void BuildFilter_WithoutCriteria_HasNoWhereClause()
    {
        (string whereSql, DynamicParameters parameters) = Filter();

        Assert.Empty(whereSql);
        Assert.Equal(_clock.Today, parameters.Get<DateOnly>("Today"));
    }

    [Fact]
    public void BuildFilter_ForActiveCards_StartsAtToday()
    {
        (string whereSql, _) = Filter(CardStatus.Active);

        Assert.Equal("WHERE readers.expires_on >= @Today", whereSql);
    }

    [Fact]
    public void BuildFilter_ForCardsExpiringSoon_CoversTheNextThirtyDays()
    {
        (string whereSql, DynamicParameters parameters) = Filter(CardStatus.ExpiringSoon);

        Assert.Equal("WHERE readers.expires_on >= @Today AND readers.expires_on <= @ExpiringSoonUntil", whereSql);
        Assert.Equal(_clock.Today.AddDays(30), parameters.Get<DateOnly>("ExpiringSoonUntil"));
    }

    [Fact]
    public void BuildFilter_ForExpiredCards_EndsBeforeToday()
    {
        (string whereSql, _) = Filter(CardStatus.Expired);

        Assert.Equal("WHERE readers.expires_on < @Today", whereSql);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void BuildFilter_WithoutASearchTerm_SkipsTheSearchCondition(string? search)
    {
        (string whereSql, DynamicParameters parameters) = Filter(search: search);

        Assert.Empty(whereSql);
        Assert.DoesNotContain("Search", parameters.ParameterNames);
    }

    [Fact]
    public void BuildFilter_Should_TrimTheSearchTermAndPassItAsAParameter()
    {
        (string whereSql, DynamicParameters parameters) = Filter(search: "  karimov ", category: ReaderCategory.Student);

        Assert.Contains("readers.search_text LIKE ALL", whereSql);
        Assert.Contains("AND readers.category = @Category", whereSql);
        Assert.DoesNotContain("karimov", whereSql);
        Assert.Equal("karimov", parameters.Get<string>("Search"));
        Assert.Equal((int)ReaderCategory.Student, parameters.Get<int>("Category"));
    }
}
