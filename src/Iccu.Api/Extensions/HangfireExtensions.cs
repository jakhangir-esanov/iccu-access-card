namespace Iccu.Api.Extensions;

using Hangfire;

internal sealed class HangfireDashboardOptions
{
    internal const string SectionName = "Hangfire:Dashboard";

    public bool Enabled { get; set; }

    public string Path { get; set; } = "/hangfire";

    public string Title { get; set; } = "ICCU Jobs";
}

internal static class HangfireExtensions
{
    internal static WebApplication UseJobsDashboard(this WebApplication app)
    {
        HangfireDashboardOptions options = app.Configuration
            .GetSection(HangfireDashboardOptions.SectionName)
            .Get<HangfireDashboardOptions>() ?? new HangfireDashboardOptions();

        if (!options.Enabled)
        {
            return app;
        }

        app.UseHangfireDashboard(
            options.Path,
            new DashboardOptions
            {
                DashboardTitle = options.Title
            });

        return app;
    }
}
