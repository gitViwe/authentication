namespace Authentication.Infrastructure.Manager;

internal sealed partial class PasskeyManager(
    IFido2 fido2,
    HubDbContext context,
    ILogger<PasskeyManager> logger,
    UserManager<HubIdentityUser> userManager) : IPasskeyManager
{
    public async Task<CredentialCreateOptions?> GetRegisterOptionsAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        
        if (user is null)
        {
            logger.UserNotFound(userId);
            return null;
        }
        
        var existingKeys = await context.HubPasskeyCredentials
            .AsNoTracking()
            .Where(c => c.UserId == user.Id)
            .Select(c => c.Descriptor)
            .ToListAsync(cancellationToken);

        var fidoUser = new Fido2User
        {
            Name = user.UserName,
            DisplayName = user.FirstName,
            Id = Encoding.UTF8.GetBytes(user.Id.ToString())
        };

        var options = fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = fidoUser,
            ExcludeCredentials = existingKeys,
            AuthenticatorSelection = new AuthenticatorSelection
            {
                UserVerification = UserVerificationRequirement.Preferred,
                ResidentKey = ResidentKeyRequirement.Discouraged
            }
        });

        return options;
    }

    public async Task<bool> RegisterCredentialAsync(
        string userId,
        string? friendlyName,
        CredentialCreateOptions originalOptions,
        AuthenticatorAttestationRawResponse response,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        
        if (user is null)
        {
            logger.UserNotFound(userId);
            return false;
        }

        var credential = await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
        {
            AttestationResponse = response,
            OriginalOptions = originalOptions,
            IsCredentialIdUniqueToUserCallback = async (args, ct) =>
            {
                var exists = await context.HubPasskeyCredentials.AnyAsync(c => c.CredentialId == args.CredentialId, ct);
                return !exists;
            }
        }, cancellationToken);

        var passkey = new HubPasskeyCredential()
        {
            CredentialId = credential.Id,
            PublicKey = credential.PublicKey,
            UserHandle = credential.User.Id,
            SignCount = credential.SignCount,
            RegDate = DateTimeOffset.UtcNow,
            FriendlyName = friendlyName,
            AaGuid = credential.AaGuid,
            UserId = user.Id
        };

        context.HubPasskeyCredentials.Add(passkey);
        await context.SaveChangesAsync(CancellationToken.None);

        return true;
    }

    public async Task<AssertionOptions?> GetAssertionOptionsAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        
        if (user is null)
        {
            LogUserNotFoundEmail(email);
            return null;
        }
        
        var existingKeys = await context.HubPasskeyCredentials
            .AsNoTracking()
            .Where(c => c.UserId == user.Id)
            .Select(c => c.Descriptor)
            .ToListAsync(cancellationToken);

        var options = fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = existingKeys,
            UserVerification = UserVerificationRequirement.Preferred
        });

        return options;
    }

    public async Task<Guid?> VerifyAssertionAsync(
        AssertionOptions originalOptions,
        AuthenticatorAssertionRawResponse response,
        CancellationToken cancellationToken)
    {
        var clientData = JsonSerializer.Deserialize<AuthenticatorResponse>(response.Response.ClientDataJson);

        if (clientData is null)
        {
            LogCouldNotParseClientData();
            return null;
        }

        var cred = await context.HubPasskeyCredentials.FirstOrDefaultAsync(c => c.CredentialId == response.RawId,
            cancellationToken);

        if (cred is null)
        {
            LogUnknownCredentialIdId(response.Id);
            return null;
        }

        var res = await fido2.MakeAssertionAsync(new MakeAssertionParams
        {
            AssertionResponse = response,
            OriginalOptions = originalOptions,
            StoredPublicKey = cred.PublicKey,
            StoredSignatureCounter = cred.SignCount,
            IsUserHandleOwnerOfCredentialIdCallback = async (args, ct) =>
            {
                return await context.HubPasskeyCredentials.AnyAsync(c => c.CredentialId == args.CredentialId && c.UserHandle == args.UserHandle, ct);
            }
        }, cancellationToken);
        
        cred.SignCount = res.SignCount;
        await context.SaveChangesAsync(CancellationToken.None);
        
        return cred.UserId;
    }

    [LoggerMessage(LogLevel.Warning, "User not found: {email}")]
    partial void LogUserNotFoundEmail(string email);

    [LoggerMessage(LogLevel.Warning, "Could not parse client data.")]
    partial void LogCouldNotParseClientData();

    [LoggerMessage(LogLevel.Warning, "Unknown credential id: {id}")]
    partial void LogUnknownCredentialIdId(string id);
}