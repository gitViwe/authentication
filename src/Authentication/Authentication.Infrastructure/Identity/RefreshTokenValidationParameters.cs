namespace Authentication.Infrastructure.Identity;

internal sealed class RefreshTokenValidationParameters(TokenValidationParameters validationParameters)
    : TokenValidationParameters(validationParameters) { }
