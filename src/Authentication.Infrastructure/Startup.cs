namespace Authentication.Infrastructure;

public static class Startup
{
    public static IServiceCollection RegisterInfrastructureLayer(this IServiceCollection services)
    {
        return services
            .RegisterOptions()
            .RegisterAuthentication()
            .RegisterCors()
            .RegisterOpenTelemetry()
            .RegisterManagerImplementation()
            .RegisterDatabase()
            .RegisterIdentity()
            .RegisterLoggingRedaction();
    }
}