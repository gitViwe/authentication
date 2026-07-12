namespace Authentication.Infrastructure.Option;

internal sealed class TokenValidationParameterOption
{
    public const string SectionName = "TokenValidationParameterOption";

    [Required]
    [MinLength(32)]
    public string Secret { get; init; } = string.Empty;
    
    [DefaultValue(true)]
    public bool ValidateIssuer { get; init; }
    
    [DefaultValue(true)]
    public bool ValidateAudience { get; init; }
    
    [DefaultValue(true)]
    public bool IssuerSigningKey { get; init; }
    
    [Required]
    [MinimumItems(1)]
    public IEnumerable<string> ValidAudiences { get; init; } = [];
    
    [Required]
    [MinimumItems(1)]
    public IEnumerable<string> ValidIssuers { get; init; } = [];
}