using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using WebAuthn.Net.Models.Protocol.Json.AuthenticationCeremony.CreateOptions;
using WebAuthn.Net.Models.Protocol.Json.AuthenticationCeremony.VerifyAssertion;

namespace API.Endpoint;

public static class AuthenticationEndpoint
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/authentication-options", GetAuthenticationOptionsAsync)
            .WithName("Authentication Options")
            .Produces<PublicKeyCredentialRequestOptionsJSON>()
            .WithOpenApi();

        app.MapPost("/complete-authentication", CompleteAuthenticationAsync)
            .WithName("Complete Authentication")
            .Produces<string>()
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> GetAuthenticationOptionsAsync(
        [FromServices] WebAuthentication auth,
        HttpContext context)
    {
        var options = await auth.GetAuthenticationOptionsAsync(context);

        return Results.Ok(options);
    }

    private static async Task<IResult> CompleteAuthenticationAsync(
        [FromBody] AuthenticationResponseJSON json,
        [FromServices] WebAuthentication auth,
        HttpContext context)
    {
        byte[] userHandle = await auth.CompleteAuthenticationAsync(context, json);

        if (userHandle.Length <= 0)
        {
            return Results.BadRequest("Invalid credentials.");
        }

        string userHandleBase64 = WebEncoders.Base64UrlEncode(userHandle);

        return Results.Ok(userHandleBase64);
    }
}
