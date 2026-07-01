namespace Authentication.Application.Manager;

public interface IPasskeyManager
{
    Task<CredentialCreateOptions?> GetRegisterOptionsAsync(string userId, CancellationToken cancellationToken);
    Task<bool> RegisterCredentialAsync(string userId, CredentialCreateOptions originalOptions, AuthenticatorAttestationRawResponse response, CancellationToken cancellationToken);
    Task<AssertionOptions?> GetAssertionOptionsAsync(string email, CancellationToken cancellationToken);
    Task<Guid?> VerifyAssertionAsync(AssertionOptions originalOptions, AuthenticatorAssertionRawResponse response, CancellationToken cancellationToken);
}