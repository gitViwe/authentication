namespace Authentication.Shared.Contract;

public class UpdateUserRequest
{
    [Required]
    public string FirstName { get; init; } = string.Empty;
    
    [Required]
    public string LastName { get; init; } = string.Empty;
}