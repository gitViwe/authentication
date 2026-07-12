using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace Authentication.Configuration;

internal static class ConfigurationManager
{
    private const string AppConfigKey = "AZURE_APP_CONFIGURATION_CONNECTION_STRING";
    
    internal static IConfigurationBuilder AddConfigurationProvider(this IConfigurationBuilder builder)
    {
        var connectionString = Environment.GetEnvironmentVariable(AppConfigKey);

        if (Uri.TryCreate(connectionString, UriKind.RelativeOrAbsolute, out var endpoint))
        {
            return builder.AddAzureAppConfiguration(options =>
            {
                options.Connect(endpoint, new DefaultAzureCredential())
                    .UseFeatureFlags()
                    .Select(KeyFilter.Any, "authentication")
                    .ConfigureRefresh(config =>
                    {
                        config.Register(KeyFilter.Any, true)
                            .SetRefreshInterval(TimeSpan.FromMinutes(5));
                    });
            });
        }
        
        return builder;
    }
}