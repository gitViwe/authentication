namespace Authentication.Infrastructure.Option;

internal sealed class DatabaseConfigurationOption
{
    public const string SectionName = "DatabaseConfigurationOption";
    
    [Required]
    public string ConnectionString { get; init; } = string.Empty;
}