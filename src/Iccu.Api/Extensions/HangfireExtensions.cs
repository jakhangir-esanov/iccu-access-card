namespace Iccu.Api.Extensions;

using Hangfire;

internal sealed class HangfireDashboardOptions
{
    internal const string SectionName = "Hangfire:Dashboard";

    public bool Enabled { get; set; }
}

internal static class HangfireExtensions
{
    private const string DashboardPath = "/hangfire";
    private const string DashboardTitle = "ICCU Jobs";

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
            DashboardPath,
            new DashboardOptions
            {
                DashboardTitle = DashboardTitle
            });

        return app;
    }
}
