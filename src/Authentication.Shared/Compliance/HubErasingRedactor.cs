namespace Authentication.Shared.Compliance;

public sealed class HubErasingRedactor : Redactor
{
    private const string ErasedValue = "[REDACTED]";

    public override int GetRedactedLength(ReadOnlySpan<char> input) => ErasedValue.Length;

    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        // The base class ensures destination has sufficient capacity
        ErasedValue.CopyTo(destination);
        return ErasedValue.Length;
    }
}