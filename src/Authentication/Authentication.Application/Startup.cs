namespace Authentication.Application;

public static class Startup
{
    public static IServiceCollection RegisterApplicationLayer(this IServiceCollection services)
    {
        return services
            .RegisterFeatures();
    }
}