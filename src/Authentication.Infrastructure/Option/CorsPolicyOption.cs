namespace Authentication.Infrastructure.Option;

internal sealed class CorsPolicyOption
{
    public const string SectionName = "CorsPolicyOption";
    
    [Required]
    [MinimumItems(1)]
    public IEnumerable<string> AllowedCorsOrigins { get; init; } = [];
}