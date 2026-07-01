namespace Authentication.Infrastructure.Configuration.Option;

internal sealed class DatabaseConfigurationOption
{
    public const string SectionName = "DatabaseConfigurationOption";

    [DefaultValue(DatabaseProviderType.PostgreSql)]
    public DatabaseProviderType DatabaseProviderType { get; init; }
    
    [Required]
    public required DatabaseProviderConfiguration DatabaseProviderConfiguration { get; init; }
}

internal sealed class DatabaseProviderConfiguration
{
    [Required]
    public string ConnectionString { get; init; } = string.Empty;
}

internal enum  DatabaseProviderType
{
    Sqlite = 0,
    PostgreSql
}