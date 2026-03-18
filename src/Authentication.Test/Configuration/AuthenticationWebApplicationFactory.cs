using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Authentication.Test.Configuration;

public class AuthenticationWebApplicationFactory(string postgresConnectionString) : WebApplicationFactory<AuthenticationApiMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://localhost:5056");
        Environment.SetEnvironmentVariable("DatabaseConfigurationOption__ConnectionString", postgresConnectionString);
        builder.UseEnvironment("Development");
    }
    
    public static HttpClient CreateAuthenticationApiHttpClient(string postgresConnectionString)
    {
        var client = new AuthenticationWebApplicationFactory(postgresConnectionString).CreateDefaultClient(new Uri("http://localhost:5056"));
        client.DefaultRequestHeaders.Add("Origin", "http://localhost:5056");
        return client;
    }
}