namespace Authentication.Application.Feature;

public sealed class PasskeyAssertionOptionsQuery
{
    public required string Email { get; init; }
}

internal sealed class PasskeyAssertionOptionsQueryHandler(IPasskeyManager passkeyManager, IMemoryCache memoryCache)
{
    public async Task<ITypedResponse<AssertionOptions>> HandleAsync(
        PasskeyAssertionOptionsQuery query,
        CancellationToken cancellationToken)
    {
        var options = await passkeyManager.GetAssertionOptionsAsync(query.Email, cancellationToken);

        if (options is null)
        {
            return TypedResponse<AssertionOptions>.Fail("Failed to retrieve assertion options.",
                StatusCodes.Status401Unauthorized);
        }
        
        memoryCache.Set(HubCacheKeys.Passkey.AssertionOptions + query.Email, options, TimeSpan.FromSeconds(60));
        
        return TypedResponse<AssertionOptions>.Success("Retrieve assertion options.", options);
    }
}