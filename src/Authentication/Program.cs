using Authentication.Application;
using Authentication.Application.Endpoint;
using Authentication.Infrastructure;
using Authentication.Infrastructure.Extension;
using Authentication.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOpenApi(options =>
    {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    })
    .RegisterApplicationLayer()
    .RegisterInfrastructureLayer();

var app = builder.Build();

app
    .UseCors()
    .UseHubExceptionHandler();

app.MapOpenApi().AllowAnonymous();
app.MapScalarApiReference().AllowAnonymous();

app
    .UseHttpsRedirection()
    .UseAuthentication()
    .UseAuthorization();

app.MapAccountEndpoint();

app.ApplyMigrations();

app.Run();

public class AuthenticationApiMarker;