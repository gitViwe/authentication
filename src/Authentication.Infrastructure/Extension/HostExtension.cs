namespace Authentication.Infrastructure.Extension;

public static class HostExtension
{
    public static void ApplyMigrations(this IHost host)
    {
        OpenTelemetryActivity.InternalProcess.StartActivity("HostExtension", "ApplyMigrations");
        using var scope = host.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        context.Database.Migrate();
    }
}