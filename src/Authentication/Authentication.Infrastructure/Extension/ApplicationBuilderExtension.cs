using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;

namespace Authentication.Infrastructure.Extension;

public static class ApplicationBuilderExtension
{
    public static IApplicationBuilder UseHubExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseExceptionHandler(options =>
        {
            options.Run(async context =>
            {
                var handlerFeature = context.Features.GetRequiredFeature<IExceptionHandlerPathFeature>();

                var response = ProblemDetailFactory.CreateProblemDetails(context, StatusCodes.Status500InternalServerError);

                OpenTelemetryActivity.InternalProcess.StartActivity("HubExceptionHandler", "ExceptionHandled", handlerFeature.Error);
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                await context.Response.CompleteAsync();
            });
        });
    }
}
