namespace Authentication.Infrastructure.Option;

internal sealed class OpenTelemetryConfigurationOption
{
    public const string SectionName = "OpenTelemetryConfigurationOption";
    
    [Required]
    public string ServiceName { get; init; } = string.Empty;
    
    [Required]
    public string ServiceNamespace { get; init; } = string.Empty;
    
    [Required]
    public string ServiceVersion { get; init; } = string.Empty;
    
    [Required]
    public string Endpoint { get; init; } = string.Empty;
    
    public string Headers { get; init; } = string.Empty;
}