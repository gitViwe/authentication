namespace API.Extension;

public static class ServiceCollectionExtension
{
    public static IServiceCollection RegisterWebAuthnSqlServer(
        this IServiceCollection services,
        Action<SqlServerOptions> configureSqlServer)
    {
        services
            .AddWebAuthnCore<DefaultSqlServerContext>()
            .AddRegistrationCeremonyStorage<DefaultSqlServerContext, DefaultCacheRegistrationCeremonyStorage<DefaultSqlServerContext>>()
            .AddAuthenticationCeremonyStorage<DefaultSqlServerContext, DefaultCacheAuthenticationCeremonyStorage<DefaultSqlServerContext>>()
            .AddDefaultFidoMetadataStorage()
            .AddContextFactory<DefaultSqlServerContext, DefaultSqlServerContextFactory>()
            .AddCredentialStorage<DefaultSqlServerContext, DefaultSqlServerCredentialStorage<DefaultSqlServerContext>>();

        services
            .AddScoped<WebAuthentication>()
            .AddOptions<SqlServerOptions>()
            .Configure(configureSqlServer);

        return services;
    }

    public static IServiceCollection RegisterRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddGitViweRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "redis_demo";
            options.AbsoluteExpirationInMinutes = 5;
            options.SlidingExpirationInMinutes = 2;
        });
    }

    public static IServiceCollection RegisterDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddDbContext<WebAuthenticationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("MSSQL"));
        });
    }

    public static IServiceCollection RegisterCorsPolicy(this IServiceCollection services)
    {
        return services.AddCors(options =>
        {
            options.AddDefaultPolicy(policyBuilder =>
            {
                policyBuilder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });
        });
    }
}
