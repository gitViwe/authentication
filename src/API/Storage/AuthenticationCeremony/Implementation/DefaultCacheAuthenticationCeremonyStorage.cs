namespace API.Storage.AuthenticationCeremony.Implementation;

public class DefaultCacheAuthenticationCeremonyStorage<TContext> : IAuthenticationCeremonyStorage<TContext>
    where TContext : class, IWebAuthnContext
{
    public DefaultCacheAuthenticationCeremonyStorage(IServiceProvider provider)
    {
        using var scope = provider.CreateAsyncScope();
        _cache = scope.ServiceProvider.GetRequiredService<IRedisDistributedCache>();
    }

    private readonly IRedisDistributedCache _cache;

    public Task<AuthenticationCeremonyParameters?> FindAsync(TContext context, string authenticationCeremonyId, CancellationToken cancellationToken)
    {
        return _cache.GetAsync<AuthenticationCeremonyParameters>(authenticationCeremonyId, cancellationToken);
    }

    public Task RemoveAsync(TContext context, string authenticationCeremonyId, CancellationToken cancellationToken)
    {
        return _cache.RemoveAsync(authenticationCeremonyId, cancellationToken);
    }

    public async Task<string> SaveAsync(TContext context, AuthenticationCeremonyParameters authenticationCeremony, CancellationToken cancellationToken)
    {
        string authenticationCeremonyId = Guid.NewGuid().ToString();

        await _cache.SetAsync(key: authenticationCeremonyId, authenticationCeremony, token: cancellationToken);

        return authenticationCeremonyId;
    }
}
