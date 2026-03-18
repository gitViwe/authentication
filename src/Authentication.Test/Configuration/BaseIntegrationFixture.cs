using DotNet.Testcontainers.Containers;
using Testcontainers.PostgreSql;

namespace Authentication.Test.Configuration;

public sealed class BaseIntegrationFixture : IAsyncLifetime
{
    private static readonly IContainer PostgreSqlContainer = new PostgreSqlBuilder("postgres:16.3-alpine")
        .WithDatabase("auth-db")
        .WithUsername("user")
        .WithPassword("pass")
        .Build();
    
    public async Task InitializeAsync()
    {
        await PostgreSqlContainer.StartAsync();
        AuthenticationClient =
            AuthenticationWebApplicationFactory
                .CreateAuthenticationApiHttpClient(PostgreSqlContainer.GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        await PostgreSqlContainer.DisposeAsync();
    }

    public HttpClient AuthenticationClient { get; private set; }
}

/// <summary>
/// All tests in this collection should share the same instance of BaseIntegrationFixture
/// </summary>
[CollectionDefinition(nameof(BaseIntegrationFixtureCollection))]
public sealed class BaseIntegrationFixtureCollection : ICollectionFixture<BaseIntegrationFixture> { }

/// <summary>
/// The class tests will inherit from.
/// By using the [Collection] attribute, it gains access to the already-running Postgres container and the pre-configured HttpClient via the integrationFixture parameter
/// </summary>
[Collection(nameof(BaseIntegrationFixtureCollection))]
public class BaseIntegrationTest(BaseIntegrationFixture integrationFixture)
{
    public BaseIntegrationFixture IntegrationFixture { get; } = integrationFixture;
}