namespace Iccu.Api.Extensions;

using Iccu.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

internal static class MigrationExtensions
{
    internal static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        await using AsyncServiceScope scope = app.Services.CreateAsyncScope();

        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}
