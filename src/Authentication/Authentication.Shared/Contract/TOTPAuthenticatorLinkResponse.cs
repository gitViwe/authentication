namespace Authentication.Shared.Contract;

public sealed class TOTPAuthenticatorLinkResponse
{
    public string Link { get; init; } = string.Empty;
}