namespace Authentication.Application.Extension;

public static class EndpointFilterExtension
{
    public static TBuilder WithFeatureFlag<TBuilder>(this TBuilder builder, string feature) where TBuilder : IEndpointConventionBuilder
        => builder.AddEndpointFilter(new FeatureFlagFilter(feature));
    
    public static RouteHandlerBuilder DataAnnotationValidation<T>(this RouteHandlerBuilder builder) where T : class
        => builder.AddEndpointFilter(new DataAnnotationValidationFilter(typeof(T)));
    
    public static RouteGroupBuilder DataAnnotationValidation<T>(this RouteGroupBuilder builder) where T : class
        => builder.AddEndpointFilter(new DataAnnotationValidationFilter(typeof(T)));
}

internal sealed class FeatureFlagFilter(string feature) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var featureManager = context.HttpContext.RequestServices.GetRequiredService<IFeatureFlagManager>();
        var isEnabled = await featureManager.IsEnabledAsync(feature);

        Dictionary<string, object?> tags = new()
        {
            { OpenTelemetryTagKey.EndpointFilter.FILTER_TYPE, GetType().FullName },
            { OpenTelemetryTagKey.PREFIX + "endpoint_filter.feature.name", feature },
            { OpenTelemetryTagKey.PREFIX + "endpoint_filter.feature.is_enabled", isEnabled },
        };
        
        OpenTelemetryActivity.InternalProcess.StartActivity("FeatureFlagFilter", "Evaluating Feature Flag", tags: tags);

        return isEnabled ? await next(context) : Results.NotFound();
    }
}

internal sealed class DataAnnotationValidationFilter(Type targetType) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Find the first argument that matches the specified type
        var argument = context.Arguments.FirstOrDefault(x => x is not null && x.GetType() == targetType);

        if (argument is not null)
        {
            Dictionary<string, object?> tags = new()
            {
                { OpenTelemetryTagKey.PREFIX + "data_annotation_validation_filter.type", targetType.FullName },
            };
            
            OpenTelemetryActivity.InternalProcess.StartActivity("DataAnnotationValidationFilter", "Validating request", tags: tags);
            
            var validationContext = new ValidationContext(argument, context.HttpContext.RequestServices, items: null);
            List<ValidationResult> validationResults = [];

            if (false == Validator.TryValidateObject(argument, validationContext, validationResults, validateAllProperties: true))
            {
                var errors = validationResults
                    .SelectMany(result => result.MemberNames.Select(member => new { member, message = result.ErrorMessage }))
                    .GroupBy(x => x.member)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.message ?? "Invalid value.").ToArray()
                    );
                
                return ProblemDetailFactory.CreateValidationProblemResult(context.HttpContext, StatusCodes.Status400BadRequest, errors);
            }
        }

        return await next(context);
    }
}