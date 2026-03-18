namespace Authentication.Shared.Contract;

public sealed class TOTPVerifyRequest
{
    [Required]
    [MinLength(8)]
    public string Token { get; init; } = string.Empty;
}