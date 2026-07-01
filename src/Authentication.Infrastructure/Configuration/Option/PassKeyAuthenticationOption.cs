namespace Authentication.Infrastructure.Configuration.Option;

internal sealed class PassKeyAuthenticationOption
{
    public const string SectionName = "PassKeyAuthenticationOption";
    
    [Required]
    public string ServerDomain { get; init; } = string.Empty;
    
    [Required]
    public string ServerName { get; init; } = string.Empty;
    
    [Required]
    [MinimumItems(1)]
    public IEnumerable<string> AllowedOrigins { get; init; } = [];
}