using Authentication.Infrastructure.Handler;
using Authentication.Infrastructure.Manager;
using Authentication.Shared.Compliance;
using Microsoft.Extensions.Compliance.Classification;

namespace Authentication.Infrastructure.Extension;

internal static class ServiceCollectionExtension
{
    private static TokenValidationParameters CreateTokenValidationParameters(TokenValidationParameterOption option, bool isRefreshToken = false)
    {
        var key = Encoding.ASCII.GetBytes(option.Secret);

        return new TokenValidationParameters
        {
            ValidIssuer = option.ValidIssuer,
            ValidAudiences = option.ValidAudiences,
            // specify the security key used for 
            IssuerSigningKey = new SymmetricSecurityKey(key),
            // validates the signature of the key
            ValidateIssuerSigningKey = option.IssuerSigningKey,
            ValidateAudience = option.ValidateAudience,
            ValidateIssuer = option.ValidateIssuer,
            ValidateLifetime = false == isRefreshToken,
        };
    }
    
    internal static IServiceCollection RegisterOptions(this IServiceCollection services)
    {
        services.AddOptionsWithValidateOnStart<ApiKeyAuthenticationOption>()
            .BindConfiguration(ApiKeyAuthenticationOption.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptionsWithValidateOnStart<CorsPolicyOption>()
            .BindConfiguration(CorsPolicyOption.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptionsWithValidateOnStart<DatabaseConfigurationOption>()
            .BindConfiguration(DatabaseConfigurationOption.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptionsWithValidateOnStart<OpenTelemetryConfigurationOption>()
            .BindConfiguration(OpenTelemetryConfigurationOption.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptionsWithValidateOnStart<TokenValidationParameterOption>()
            .BindConfiguration(TokenValidationParameterOption.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        return services;
    }
    
    internal static IServiceCollection RegisterAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // configures authentication using JWT and API keys
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = ApiKeyAuthenticationDefault.AuthenticationScheme;
        })
        .AddScheme<ApiKeyAuthenticationOption, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationDefault.AuthenticationScheme, options =>
        {
            options.ForwardDefaultSelector = context =>
                // Forward Authentication to JwtBearer if API Key header is not present
                context.Request.Headers.TryGetValue(options.ApiKeyHeaderName, out _)
                    ? null
                    : JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            // set the parameters used to validate
            var tokenValidationParameterOption = services
                .BuildServiceProvider()
                .GetRequiredService<IOptions<TokenValidationParameterOption>>();
            options.TokenValidationParameters = CreateTokenValidationParameters(tokenValidationParameterOption.Value);

            options.Events = new JwtBearerEvents()
            {
                OnAuthenticationFailed = context =>
                {
                    OpenTelemetryActivity.InternalProcess.StartActivity("JwtBearerEvents", "OnAuthenticationFailed", context.Exception);

                    // JWT token has expired
                    var problem = context.Exception is SecurityTokenExpiredException
                        ? ProblemDetailFactory.CreateProblemDetails(context.HttpContext, StatusCodes.Status401Unauthorized, "Token expired")
                        : ProblemDetailFactory.CreateProblemDetails(context.HttpContext, StatusCodes.Status401Unauthorized);

                    return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
                },
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    if (false == context.Response.HasStarted)
                    {
                        OpenTelemetryActivity.InternalProcess.StartActivity("JwtBearerEvents", "OnChallenge");
                        var response = ProblemDetailFactory.CreateProblemDetails(context.HttpContext, StatusCodes.Status401Unauthorized);
                        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    }

                    return Task.CompletedTask;
                },
                OnForbidden = context =>
                {
                    OpenTelemetryActivity.InternalProcess.StartActivity("JwtBearerEvents", "OnForbidden");
                    var response = ProblemDetailFactory.CreateProblemDetails(context.HttpContext, StatusCodes.Status403Forbidden);
                    return context.Response.WriteAsync(JsonSerializer.Serialize(response));
                },
                OnTokenValidated = context =>
                {
                    Dictionary<string, object?> tags = new()
                    {
                        { OpenTelemetryTagKey.JWT.USER_ID, context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier) },
                        { OpenTelemetryTagKey.JWT.JWT_ID, context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti) },
                        { OpenTelemetryTagKey.JWT.JWT_ISSUER, context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Iss) },
                        { OpenTelemetryTagKey.JWT.JWT_AUDIENCE, context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Aud) },
                    };

                    OpenTelemetryActivity.InternalProcess.StartActivity("JwtBearerEvents", "OnTokenValidated", tags: tags);

                    return Task.CompletedTask;
                }
            };
        });

        // add authorization to apply policy
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(ApiKeyAuthenticationDefault.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();

            foreach (var prop in typeof(HubPermissions).GetNestedTypes().SelectMany(c => c.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)))
            {
                var propertyValue = prop.GetValue(null);

                if (propertyValue is not null)
                {
                    // add new permission policy
                    options.AddPolicy(propertyValue.ToString()!, policy => policy.RequireClaim("Permission", propertyValue.ToString()!)
                           // add JWT Bearer authentication scheme to this policy
                           .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));
                }
            }
        });

        return services;
    }
    
    internal static IServiceCollection RegisterCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var corsPolicyOption = services
            .BuildServiceProvider()
            .GetRequiredService<IOptions<CorsPolicyOption>>();
        
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder =>
                {
                    builder
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins(corsPolicyOption.Value.AllowedCorsOrigins.ToArray());
                });
        });

        return services;
    }
    
    internal static IServiceCollection RegisterOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var config = services
            .BuildServiceProvider()
            .GetRequiredService<IOptions<OpenTelemetryConfigurationOption>>();

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(config.Value.ServiceName, config.Value.ServiceNamespace, config.Value.ServiceVersion))
            .WithTracing(builder =>
            {
                builder.AddSource(OpenTelemetrySource.INTERNAL_PROCESS, OpenTelemetrySource.EXTERNAL_PROCESS)
                       .AddHttpClientInstrumentation(options =>
                       {
                           options.RecordException = true;
                           options.EnrichWithException = (activity, exception) => activity.AddException(exception);
                           options.EnrichWithHttpRequestMessage = (activity, request) => OpenTelemetryActivity.Instrumentation.EnrichWithHttpRequestHeaders(activity, request.Headers);
                           options.EnrichWithHttpResponseMessage = (activity, response) => OpenTelemetryActivity.Instrumentation.EnrichWithHttpResponseHeaders(activity, response.Headers);
                       })
                       .AddEntityFrameworkCoreInstrumentation()
                       .AddAspNetCoreInstrumentation(options =>
                       {
                           options.RecordException = true;
                           options.EnrichWithException = (activity, exception) => activity.AddException(exception);
                           options.EnrichWithHttpRequest = (activity, request) => OpenTelemetryActivity.Instrumentation.EnrichWithHttpRequestHeaders(activity, request.Headers);
                           options.EnrichWithHttpResponse = (activity, response) => OpenTelemetryActivity.Instrumentation.EnrichWithHttpResponseHeaders(activity, response.Headers);
                       });

                builder.AddOtlpExporter(option =>
                {
                    option.Endpoint = new Uri(config.Value.Endpoint);
                    option.Headers = config.Value.Headers;
                });
            })
            .WithMetrics(builder =>
            {
                builder.AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel")
                       .AddOtlpExporter(option =>
                       {
                           option.Endpoint = new Uri(config.Value.Endpoint);
                           option.Headers = config.Value.Headers;
                       });
            })
            .WithLogging(builder =>
            {
                builder.AddOtlpExporter(option =>
                {
                    option.Endpoint = new Uri(config.Value.Endpoint);
                    option.Headers = config.Value.Headers;
                });
            });

        return services;
    }
    
    internal static IServiceCollection RegisterManagerImplementation(this IServiceCollection services)
    {
        return services
            .AddScoped<ITokenManager, TokenManager>()
            .AddScoped<IUserIdentityManager, UserIdentityManager>()
            .AddGitViweTimeBasedOneTimePassword()
            .AddGitViweJsonWebToken();
    }

    internal static IServiceCollection RegisterDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var configOptions = services
            .BuildServiceProvider()
            .GetRequiredService<IOptions<DatabaseConfigurationOption>>();

        return services.AddDbContext<HubDbContext>(options =>
        {
            options.UseNpgsql(configOptions.Value.ConnectionString, optionsBuilder =>
            {
                optionsBuilder.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                optionsBuilder.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
            });
        });
    }
    
    internal static IServiceCollection RegisterIdentity(this IServiceCollection services)
    {
        services.AddIdentity<HubIdentityUser, HubIdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            }).AddEntityFrameworkStores<HubDbContext>()
            .AddTokenProvider<TimeBasedOneTimePinTokenProvider>(TimeBasedOneTimePinTokenProvider.ProviderKey)
            .AddDefaultTokenProviders();

        return services;
    }

    internal static IServiceCollection RegisterLoggingRedaction(this IServiceCollection services)
    {
        return services.AddRedaction(options =>
        {
            options.SetRedactor<HubErasingRedactor>(new DataClassificationSet(HubTaxonomy.RedactSensitiveData));
        });
    }

    internal static IServiceCollection RegisterApiDocumentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        throw new NotImplementedException();
    }
}