namespace Authentication.Infrastructure.Extension;

public static class ConfigurationBuilderExtension
{
    public static IConfigurationBuilder AddInfrastructureConfiguration(this IConfigurationBuilder builder)
    {
        return builder.SetBasePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Configuration"))
            .AddJsonFile($"apikey-authentication.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"cors-policy.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"database-configuration.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"json-web-token.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"open-telemetry.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"pass-key-configuration.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"time-based-one-time-password.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"token-validation-parameter.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    }
}