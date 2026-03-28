namespace Authentication.Application.Feature;

public sealed class TotpGenerateLinkQuery
{
    public required string UserId { get; init; }
}

internal sealed class TotpGenerateLinkQueryHandler(IUserIdentityManager userIdentityManager)
{
    public async Task<ITypedResponse<TOTPAuthenticatorLinkResponse>> HandleAsync(TotpGenerateLinkQuery query, CancellationToken cancellationToken)
    {
        var link = await userIdentityManager.GenerateTimeBasedOneTimePinLinkAsync(query.UserId);
        
        if (string.IsNullOrWhiteSpace(link))
        {
            return TypedResponse<TOTPAuthenticatorLinkResponse>.Fail("Failed to generate 2FA link.", StatusCodes.Status400BadRequest);
        }

        var response = new TOTPAuthenticatorLinkResponse { Link = link };
        return TypedResponse<TOTPAuthenticatorLinkResponse>.Success("2FA setup link generated.", response);
    }
}