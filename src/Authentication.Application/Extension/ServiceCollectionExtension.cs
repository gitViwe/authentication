namespace Authentication.Application.Extension;

internal static class ServiceCollectionExtension
{
    internal static IServiceCollection RegisterFeatures(this IServiceCollection services)
    {
        return services
            .AddScoped<UserDetailQueryHandler>()
            .AddScoped<LoginUserCommandHandler>()
            .AddScoped<RegisterUserCommandHandler>()
            .AddScoped<UserDetailUpdateCommandHandler>();
    }
}