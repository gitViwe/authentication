using Microsoft.Extensions.Compliance.Classification;

namespace Authentication.Shared.Compliance;

public sealed class HubTaxonomy
{
    public static string TaxonomyName => "HubTaxonomy";

    public static DataClassification RedactSensitiveData => new(TaxonomyName, nameof(RedactSensitiveData));
}