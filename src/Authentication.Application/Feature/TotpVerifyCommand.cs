namespace Authentication.Application.Feature;

public sealed class TotpVerifyCommand : TotpVerifyRequest
{
    public required string UserId { get; init; }
}

internal sealed class TotpVerifyCommandHandler(IUserIdentityManager userIdentityManager)
{
    public async Task<IResponse> HandleAsync(TotpVerifyCommand command, CancellationToken cancellationToken)
    {
        var isVerified = await userIdentityManager.VerifyTimeBasedOneTimePinLinkAsync(command, command.UserId, cancellationToken);
        
        if (!isVerified)
        {
            return Response.Fail("Invalid 2FA token.", StatusCodes.Status400BadRequest);
        }

        return Response.Success("2FA successfully enabled and verified.");
    }
}