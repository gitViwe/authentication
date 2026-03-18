namespace Authentication.Shared.Contract;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    [RedactSensitiveData]
    public string Password { get; set; } =  string.Empty;
}

public class TimeBasedOneTimePinLoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Token { get; set; } = string.Empty;
}