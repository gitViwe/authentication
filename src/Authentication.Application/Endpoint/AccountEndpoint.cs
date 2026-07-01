namespace Authentication.Application.Endpoint;

public static class AccountEndpoint
{
    public static IEndpointRouteBuilder MapAccountEndpoint(this IEndpointRouteBuilder app)
    {
        var accountGroup = app.MapGroup("account")
            .WithTags("Account")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        
        accountGroup.MapPost("register", RegisterAsync)
            .AllowAnonymous()
            .WithName(nameof(RegisterAsync))
            .DataAnnotationValidation<RegisterRequest>()
            .Produces<TokenResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .AddOpenApiOperationTransformer((operation, _, _) =>
            {
                operation.Summary = "Register new user.";
                operation.Description = "Provide a new username, email and password to create the account.";
                return Task.CompletedTask;
            });
        
        accountGroup.MapPost("login", LoginAsync)
            .AllowAnonymous()
            .WithName(nameof(LoginAsync))
            .DataAnnotationValidation<LoginRequest>()
            .Produces<TokenResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .AddOpenApiOperationTransformer((operation, _, _) =>
            {
                operation.Summary = "Login existing user.";
                operation.Description = "Provide an email and password to get the JSON web token for this user.";
                return Task.CompletedTask;
            });
        
        accountGroup.MapGet("login/{email}", GetLoginOptionsAsync)
            .AllowAnonymous()
            .WithName(nameof(GetLoginOptionsAsync))
            .Produces<LoginOptionsResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .AddOpenApiOperationTransformer((operation, _, _) =>
            {
                operation.Summary = "Get login options for user.";
                operation.Description = "Provide an email to get the available login options for this user.";
                return Task.CompletedTask;
            });
        
        accountGroup.MapGet("detail", GetUserDetailAsync)
            .WithName(nameof(GetUserDetailAsync))
            .Produces<UserDetailResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .AddOpenApiOperationTransformer((operation, _, _) =>
            {
                operation.Summary = "Get user details.";
                operation.Description = "Get the current user's details.";
                return Task.CompletedTask;
            });
        
        accountGroup.MapPut("detail", UpdateDetailsAsync)
            .WithName(nameof(UpdateDetailsAsync))
            .DataAnnotationValidation<UpdateUserRequest>()
            .Produces(StatusCodes.Status204NoContent)
            .AddOpenApiOperationTransformer((operation, _, _) =>
            {
                operation.Summary = "Update user details.";
                operation.Description = "Update the current user's first name and last name.";
                return Task.CompletedTask;
            });
        
        accountGroup.MapGet("2fa/setup", GetTwoFactorSetupAsync)
            .WithName(nameof(GetTwoFactorSetupAsync))
            .Produces<TOTPAuthenticatorLinkResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .AddOpenApiOperationTransformer((operation, _, _) =>
            {
                operation.Summary = "Get 2FA setup link.";
                operation.Description = "Generates an authenticator URI for the user to scan into Authy or Google Authenticator.";
                return Task.CompletedTask;
            });

        accountGroup.MapPost("2fa/verify", VerifyTwoFactorSetupAsync)
            .WithName(nameof(VerifyTwoFactorSetupAsync))
            .DataAnnotationValidation<TotpVerifyRequest>()
            .Produces(StatusCodes.Status200OK)
            .AddOpenApiOperationTransformer((operation, _, _) =>
            {
                operation.Summary = "Verify 2FA setup.";
                operation.Description = "Verifies the token from the authenticator app to fully enable 2FA.";
                return Task.CompletedTask;
            });

        accountGroup.MapPost("2fa/login", LoginTotpAsync)
            .AllowAnonymous()
            .WithName(nameof(LoginTotpAsync))
            .DataAnnotationValidation<TimeBasedOneTimePinLoginRequest>()
            .Produces<TokenResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .AddOpenApiOperationTransformer((operation, _, _) =>
            {
                operation.Summary = "Login using 2FA.";
                operation.Description = "Provide an email and the 6-digit TOTP token to get the JSON web token.";
                return Task.CompletedTask;
            });
        
        accountGroup.MapGet("passkey/register", GetPasskeyRegisterOptionsAsync)
            .WithName(nameof(GetPasskeyRegisterOptionsAsync))
            .Produces<CredentialCreateOptions>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .AddOpenApiOperationTransformer((operation, _, _) =>
            {
                operation.Summary = "Get passkey registration options.";
                operation.Description = "Initiate a passkey registration using the allowed options.";
                return Task.CompletedTask;
            });
        
        accountGroup.MapPost("passkey/register", PasskeyRegisterCredentialAsync)
            .WithName(nameof(PasskeyRegisterCredentialAsync))
            .DataAnnotationValidation<AuthenticatorAttestationRawResponse>()
            .Produces(StatusCodes.Status204NoContent)
            .AddOpenApiOperationTransformer((operation, _, _) =>
            {
                operation.Summary = "Register passkey.";
                operation.Description = "Register a new passkey credential.";
                return Task.CompletedTask;
            });
        
        accountGroup.MapGet("passkey/login/{email}", GetPasskeyAssertionOptionsAsync)
            .WithName(nameof(GetPasskeyAssertionOptionsAsync))
            .Produces<AssertionOptions>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .AddOpenApiOperationTransformer((operation, _, _) =>
            {
                operation.Summary = "Get passkey verification / assertion options.";
                operation.Description = "Initiate a passkey verification / assertion using the allowed options.";
                return Task.CompletedTask;
            });
        
        accountGroup.MapPost("passkey/login/{email}", PasskeyVerifyAssertionAsync)
            .WithName(nameof(PasskeyVerifyAssertionAsync))
            .DataAnnotationValidation<AuthenticatorAssertionRawResponse>()
            .Produces<TokenResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .AddOpenApiOperationTransformer((operation, _, _) =>
            {
                operation.Summary = "Login using a passkey.";
                operation.Description = "Provide an email and authenticate using a passkey.";
                return Task.CompletedTask;
            });
        
        return app;
    }
    
    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        [FromServices] RegisterUserCommandHandler handler,
        HttpContext httpContext,
        CancellationToken cancellation = default)
    {
        var response = await handler.HandleAsync(new RegisterUserCommand
        {
            Origin = httpContext.Request.Headers.Origin!,
            Email = request.Email,
            Password = request.Password,
            PasswordConfirmation = request.PasswordConfirmation,
            UserName = request.UserName,
            FirstName = request.FirstName,
            LastName = request.LastName,
        }, cancellation);

        return response.Succeeded
            ? Results.Ok(response.Data)
            : ProblemDetailFactory.CreateProblemResult(httpContext, response.StatusCode, response.Message);
    }
    
    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        [FromServices] LoginUserCommandHandler handler,
        HttpContext httpContext,
        CancellationToken cancellation = default)
    {
        var response = await handler.HandleAsync(new LoginUserCommand
        {
            Origin = httpContext.Request.Headers.Origin!,
            Email = request.Email,
            Password = request.Password,
        }, cancellation);

        return response.Succeeded
            ? Results.Ok(response.Data)
            : ProblemDetailFactory.CreateProblemResult(httpContext, response.StatusCode, response.Message);
    }
    
    private static async Task<IResult> GetUserDetailAsync(
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
    
    private static async Task<IResult> UpdateDetailsAsync(
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
    
    private static async Task<IResult> GetTwoFactorSetupAsync(
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

    private static async Task<IResult> VerifyTwoFactorSetupAsync(
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

    private static async Task<IResult> LoginTotpAsync(
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
    
    private static async Task<IResult> GetPasskeyRegisterOptionsAsync(
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
    
    private static async Task<IResult> PasskeyRegisterCredentialAsync(
        AuthenticatorAttestationRawResponse attestationRawResponse,
        [FromServices] PasskeyRegisterCredentialCommandHandler handler,
        HttpContext httpContext,
        CancellationToken cancellation = default)
    {
        var response = await handler.HandleAsync(new PasskeyRegisterCredentialCommand
        {
            UserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            AttestationRawResponse = attestationRawResponse
        }, cancellation);

        return response.Succeeded
            ? Results.NoContent()
            : ProblemDetailFactory.CreateProblemResult(httpContext, response.StatusCode, response.Message);
    }
    
    private static async Task<IResult> GetPasskeyAssertionOptionsAsync(
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
    
    private static async Task<IResult> PasskeyVerifyAssertionAsync(
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
    
    private static async Task<IResult> GetLoginOptionsAsync(
        [FromRoute] string email,
        [FromServices] LoginOptionsQueryHandler handler,
        HttpContext httpContext,
        CancellationToken cancellation = default)
    {
        var response = await handler.HandleAsync(new LoginOptionsQuery
        {
            Email = email,
            Origin = httpContext.Request.Headers.Origin!,
        }, cancellation);

        return response.Succeeded
            ? Results.Ok(response.Data)
            : ProblemDetailFactory.CreateProblemResult(httpContext, response.StatusCode, response.Message);
    }
}