namespace Authentication.Application.Endpoint.Account;

internal static class TwoFactorAuthentication
{
    internal static async Task<IResult> SetupAsync(
        [FromServices] TotpGenerateLinkQueryHandler handler,
        HttpContext httpContext,
        CancellationToken cancellation = default)
    {
        var response = await handler.HandleAsync(new TotpGenerateLinkQuery
        {
            UserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!
        }, cancellation);

        return response.Succeeded
            ? Results.Ok(response.Data)
            : ProblemDetailFactory.CreateProblemResult(httpContext, response.StatusCode, response.Message);
    }

    internal static async Task<IResult> VerifyAsync(
        TotpVerifyCommand request,
        [FromServices] TotpVerifyCommandHandler handler,
        HttpContext httpContext,
        CancellationToken cancellation = default)
    {
        var response = await handler.HandleAsync(new TotpVerifyCommand
        {
            UserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            Token = request.Token
        }, cancellation);

        return response.Succeeded
            ? Results.Ok(new { message = response.Message })
            : ProblemDetailFactory.CreateProblemResult(httpContext, response.StatusCode, response.Message);
    }

    internal static async Task<IResult> TwoFactorLoginAsync(
        TimeBasedOneTimePinLoginRequest request,
        [FromServices] TotpLoginCommandHandler handler,
        HttpContext httpContext,
        CancellationToken cancellation = default)
    {
        var response = await handler.HandleAsync(new TotpLoginCommand
        {
            Origin = httpContext.Request.Headers.Origin!,
            Email = request.Email,
            Token = request.Token
        }, cancellation);

        return response.Succeeded
            ? Results.Ok(response.Data)
            : ProblemDetailFactory.CreateProblemResult(httpContext, response.StatusCode, response.Message);
    }
}