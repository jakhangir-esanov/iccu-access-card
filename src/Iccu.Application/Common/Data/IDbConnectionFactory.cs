namespace Iccu.Application.Common.Data;

using System.Data.Common;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}
