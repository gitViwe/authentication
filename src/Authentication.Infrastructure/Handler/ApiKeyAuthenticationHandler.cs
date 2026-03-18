namespace Authentication.Infrastructure.Handler;

internal static class ApiKeyAuthenticationDefault
{
    internal const string AuthenticationScheme = "ApiKey";
}

internal sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationOption> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<ApiKeyAuthenticationOption>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        OpenTelemetryActivity.InternalProcess.StartActivity("ApiKeyAuthenticationHandler", "HandleAuthenticateAsync");

        if (Context.Request.Headers.TryGetValue(Options.ApiKeyHeaderName, out var apiKeyHeaderValues)
            && apiKeyHeaderValues.Contains(Options.ApiKeyHeaderValue)
            && Context.Request.Headers.TryGetValue(Options.OriginServiceHeaderName, out var originHeaderValues)
            && originHeaderValues.Count != 0)
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, originHeaderValues[0]!)], ApiKeyAuthenticationDefault.AuthenticationScheme));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
        }

        return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
    }
}