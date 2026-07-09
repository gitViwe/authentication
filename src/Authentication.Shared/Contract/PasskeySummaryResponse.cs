namespace Authentication.Shared.Contract;

public sealed class PasskeySummaryResponse
{
    public string? FriendlyName { get; init; }
    public DateTimeOffset? LastUsedAt { get; init; }
    public required DateTimeOffset RegisteredAt { get; init; }
    
    /// <summary>
    /// Authenticator model identifier (AAGUID). Useful for the UI to render
    /// a device icon / vendor name via a lookup table.
    /// </summary>
    public required Guid AaGuid { get; init; }
    
    /// <summary>
    /// Base64url-encoded credential id, safe to use in URLs/JSON.
    /// </summary>
    public required string CredentialId { get; init; }

}