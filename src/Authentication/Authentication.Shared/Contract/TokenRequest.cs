namespace Authentication.Shared.Contract;

public sealed record TokenRequest
{
    [Required]
    public string Token { get; init; } = string.Empty;
    
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}