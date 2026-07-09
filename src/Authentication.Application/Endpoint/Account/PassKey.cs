namespace Authentication.Application.Endpoint.Account;

internal static class PassKey
{
    internal static async Task<IResult> RegisterOptionsAsync(
        [FromServices] PasskeyRegisterOptionsQueryHandler handler,
        HttpContext httpContext,
        CancellationToken cancellation = default)
    {
        var response = await handler.HandleAsync(new PasskeyRegisterOptionsQuery
        {
            UserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!
        }, cancellation);

        return response.Succeeded
            ? Results.Ok(response.Data)
            : ProblemDetailFactory.CreateProblemResult(httpContext, response.StatusCode, response.Message);
    }
    
    internal static async Task<IResult> RegisterCredentialAsync(
        [FromRoute] string? friendlyName,
        AuthenticatorAttestationRawResponse attestationRawResponse,
        [FromServices] PasskeyRegisterCredentialCommandHandler handler,
        HttpContext httpContext,
        CancellationToken cancellation = default)
    {
        var response = await handler.HandleAsync(new PasskeyRegisterCredentialCommand
        {
            UserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            FriendlyName = friendlyName,
            AttestationRawResponse = attestationRawResponse
        }, cancellation);

        return response.Succeeded
            ? Results.NoContent()
            : ProblemDetailFactory.CreateProblemResult(httpContext, response.StatusCode, response.Message);
    }
    
    internal static async Task<IResult> AssertionOptionsAsync(
        [FromRoute] string email,
        [FromServices] PasskeyAssertionOptionsQueryHandler handler,
        HttpContext httpContext,
        CancellationToken cancellation = default)
    {
        var response = await handler.HandleAsync(new PasskeyAssertionOptionsQuery
        {
            Email = email,
        }, cancellation);

        return response.Succeeded
            ? Results.Ok(response.Data)
            : ProblemDetailFactory.CreateProblemResult(httpContext, response.StatusCode, response.Message);
    }
    
    internal static async Task<IResult> VerifyAssertionAsync(
        [FromRoute] string email,
        [FromBody] AuthenticatorAssertionRawResponse assertionRawResponse,
        [FromServices] PasskeyVerifyAssertionCommandHandler handler,
        HttpContext httpContext,
        CancellationToken cancellation = default)
    {
        var response = await handler.HandleAsync(new PasskeyVerifyAssertionCommand
        {
            Email = email,
            Origin = httpContext.Request.Headers.Origin!,
            AssertionRawResponse = assertionRawResponse
        }, cancellation);

        return response.Succeeded
            ? Results.Ok(response.Data)
            : ProblemDetailFactory.CreateProblemResult(httpContext, response.StatusCode, response.Message);
    }
}