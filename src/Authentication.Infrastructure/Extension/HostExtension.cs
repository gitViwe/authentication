using Microsoft.Extensions.Hosting;

namespace Authentication.Infrastructure.Extension;

public static class HostExtension
{
    public static void ApplyMigrations(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        context.Database.Migrate();
    }
}