namespace Authentication.Shared.Contract;

public sealed class LoginOptionsResponse
{
    public IEnumerable<string> AvailableFlows { get; init; } = [];
}