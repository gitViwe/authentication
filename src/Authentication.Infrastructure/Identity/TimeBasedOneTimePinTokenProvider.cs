namespace Authentication.Infrastructure.Identity;

internal sealed class TimeBasedOneTimePinTokenProvider(
    ITimeBasedOneTimePassword timeBasedOtp,
    ILogger<TimeBasedOneTimePinTokenProvider> logger) : IUserTwoFactorTokenProvider<HubIdentityUser>
{
    public const string ProviderKey = "TimeBasedOneTimePinTokenProvider";

    public async Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<HubIdentityUser> manager, HubIdentityUser user)
    {
        var claims = await manager.GetClaimsAsync(user);

        var hasTotpClaim = claims.Any(x => x.Type.Equals("Permission") && x.Value.Equals(HubPermissions.Authentication.VerifiedTotp));

        return false == hasTotpClaim;
    }

    public async Task<string> GenerateAsync(string purpose, UserManager<HubIdentityUser> manager, HubIdentityUser user)
    {
        if (string.IsNullOrWhiteSpace(purpose))
        {
            logger.FailedToGenerateToken(purpose, user);
            return string.Empty;
        }

        var response = timeBasedOtp.GenerateLink(user.UserName!);

        user.TimeBasedOneTimePinKey = response.SecretKey;
        var result = await manager.UpdateAsync(user);

        if (false == result.Succeeded)
        {
            logger.FailedToGenerateToken(purpose, user, result);
            return string.Empty;
        }

        return response.Link;
    }

    public async Task<bool> ValidateAsync(string purpose, string token, UserManager<HubIdentityUser> manager, HubIdentityUser user)
    {
        if (user is null
            || string.IsNullOrWhiteSpace(token)
            || string.IsNullOrWhiteSpace(purpose)
            || string.IsNullOrWhiteSpace(user.TimeBasedOneTimePinKey))
        {
            logger.FailedToValidateToken(purpose, token, user);
            return false;
        }

        if (false == timeBasedOtp.VerifyToken(user.TimeBasedOneTimePinKey, token))
        {
            logger.FailedToValidateToken(purpose, token, user);
            return false;
        }

        var claims = await manager.GetClaimsAsync(user);
        if (false == claims.Any(x => x.Type.Equals("Permission") && x.Value.Equals(HubPermissions.Authentication.VerifiedTotp)))
        {
            await manager.AddClaimAsync(user, new("Permission", HubPermissions.Authentication.VerifiedTotp));
        }

        return true;
    }
}
