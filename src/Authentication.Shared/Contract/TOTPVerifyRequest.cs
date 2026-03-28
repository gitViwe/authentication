namespace Authentication.Shared.Contract;

public class TotpVerifyRequest
{
    [Required]
    [MinLength(8)]
    public string Token { get; init; } = string.Empty;
}