using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace Authentication.Configuration;

internal static class ConfigurationManager
{
    private const string AppConfigKey = "AZURE_APP_CONFIGURATION_CONNECTION_STRING";
    
    internal static IConfigurationBuilder AddConfigurationProvider(this IConfigurationBuilder builder)
    {
        var connectionString = Environment.GetEnvironmentVariable(AppConfigKey);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return builder;
        }
        
        return builder.AddAzureAppConfiguration(options =>
        {
            options.Connect(connectionString)
                .UseFeatureFlags()
                .Select(KeyFilter.Any)
                .ConfigureRefresh(config =>
                {
                    config.Register(KeyFilter.Any, true)
                        .SetRefreshInterval(TimeSpan.FromMinutes(5));
                });
        });
    }
}