namespace Authentication.Application.Endpoint.Account;

internal static class User
{
    internal static async Task<IResult> DetailAsync(
        [FromServices] UserDetailQueryHandler handler,
        HttpContext httpContext,
        CancellationToken cancellation = default)
    {
        var response = await handler.HandleAsync(new UserDetailQuery()
        {
            UserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!,
        }, cancellation);

        return response.Succeeded
            ? Results.Ok(response.Data)
            : ProblemDetailFactory.CreateProblemResult(httpContext, response.StatusCode, response.Message);
    }
    
    internal static async Task<IResult> UpdateDetailsAsync(
        UpdateUserRequest request,
        [FromServices] UserDetailUpdateCommandHandler handler,
        HttpContext httpContext,
        CancellationToken cancellation = default)
    {
        var response = await handler.HandleAsync(new UserDetailUpdateCommand()
        {
            UserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            FirstName = request.FirstName,
            LastName = request.LastName,
        }, cancellation);

        return response.Succeeded
            ? Results.NoContent()
            : ProblemDetailFactory.CreateProblemResult(httpContext, response.StatusCode, response.Message);
    }
}