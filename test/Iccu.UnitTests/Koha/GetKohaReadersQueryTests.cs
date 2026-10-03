namespace Iccu.UnitTests.Koha;

using Iccu.Application.Koha.GetKohaReaders;

public class GetKohaReadersQueryTests
{
    [Fact]
    public void ReadersCte_Should_ExcludeDeletedAndSyncedReaders()
    {
        string cte = GetKohaReadersQueryHandler.ReadersCte;

        Assert.Contains("r.deleted_at IS NULL", cte);
        Assert.Contains("NOT r.is_koha_synced", cte);
    }
}
