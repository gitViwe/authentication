namespace Authentication.Application.Feature;

public sealed class PasskeyRegisterOptionsQuery
{
    public required string UserId { get; init; }
}

internal sealed class PasskeyRegisterOptionsQueryHandler(IPasskeyManager passkeyManager, IMemoryCache memoryCache)
{
    public async Task<ITypedResponse<CredentialCreateOptions>> HandleAsync(
        PasskeyRegisterOptionsQuery query,
        CancellationToken cancellationToken)
    {
        var options = await passkeyManager.GetRegisterOptionsAsync(query.UserId, cancellationToken);

        if (options is null)
        {
            return TypedResponse<CredentialCreateOptions>.Fail("Failed to retrieve registration options.",
                StatusCodes.Status401Unauthorized);
        }
        
        memoryCache.Set(HubCacheKeys.Passkey.CredentialCreateOptions + query.UserId, options, TimeSpan.FromSeconds(60));
        
        return TypedResponse<CredentialCreateOptions>.Success("Retrieve registration options.", options);
    }
}