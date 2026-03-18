namespace Authentication.Infrastructure.Option;

internal sealed class ApiKeyAuthenticationOption: AuthenticationSchemeOptions
{
    public const string SectionName = "ApiKeyAuthenticationOption";
    
    [Required]
    public string ApiKeyHeaderName { get; init; } = string.Empty;
    
    [Required]
    public string ApiKeyHeaderValue { get; init; } = string.Empty;
    
    [Required]
    public string OriginServiceHeaderName { get; init; } = string.Empty;
}