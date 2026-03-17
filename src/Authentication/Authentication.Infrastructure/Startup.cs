namespace Authentication.Infrastructure;

public static class Startup
{
    public static IServiceCollection RegisterInfrastructureLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .RegisterOptions()
            .RegisterAuthentication(configuration)
            .RegisterCors(configuration)
            .RegisterOpenTelemetry(configuration)
            .RegisterManagerImplementation()
            .RegisterDatabase(configuration)
            .RegisterIdentity()
            .RegisterLoggingRedaction();
    }
}