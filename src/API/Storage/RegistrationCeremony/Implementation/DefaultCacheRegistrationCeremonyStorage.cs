namespace API.Storage.RegistrationCeremony.Implementation;

public class DefaultCacheRegistrationCeremonyStorage<TContext> : IRegistrationCeremonyStorage<TContext>
    where TContext : class, IWebAuthnContext
{
    public DefaultCacheRegistrationCeremonyStorage(IServiceProvider provider)
    {
        using var scope = provider.CreateAsyncScope();
        _cache = scope.ServiceProvider.GetRequiredService<IRedisDistributedCache>();
    }
    private readonly IRedisDistributedCache _cache;

    public Task<RegistrationCeremonyParameters?> FindAsync(TContext context, string registrationCeremonyId, CancellationToken cancellationToken)
    {
        return _cache.GetAsync<RegistrationCeremonyParameters>(registrationCeremonyId, cancellationToken);
    }

    public Task RemoveAsync(TContext context, string registrationCeremonyId, CancellationToken cancellationToken)
    {
        return _cache.RemoveAsync(registrationCeremonyId, cancellationToken);
    }

    public async Task<string> SaveAsync(TContext context, RegistrationCeremonyParameters registrationCeremonyParameters, CancellationToken cancellationToken)
    {
        string registrationCeremonyId = Guid.NewGuid().ToString();

        await _cache.SetAsync(key: registrationCeremonyId, registrationCeremonyParameters, token: cancellationToken);

        return registrationCeremonyId;
    }
}
