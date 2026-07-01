namespace Authentication.Infrastructure.Persistence.Entity;

public sealed class HubPasskeyCredential
{
    public required byte[] CredentialId { get; init; } // Primary Key
    public required byte[] PublicKey { get; init; }
    public required byte[] UserHandle { get; init; }
    public uint SignCount { get; set; }
    public string? AttestationFormat { get; init; }
    public DateTimeOffset RegDate { get; init; }
    public Guid AaGuid { get; init; }
    public byte[]? AttestationObject { get; init; }
    public byte[]? AttestationClientDataJson { get; init; }
    

    public Guid UserId { get; init; }
    public HubIdentityUser User { get; init; }

    public PublicKeyCredentialDescriptor Descriptor => new(PublicKeyCredentialType.PublicKey, CredentialId, null);
}