using Microsoft.Extensions.Compliance.Classification;

namespace Authentication.Shared.Compliance;

public sealed class RedactSensitiveDataAttribute() : DataClassificationAttribute(HubTaxonomy.RedactSensitiveData);