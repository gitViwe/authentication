namespace Authentication.Infrastructure.Extension;

internal static partial class TimeBasedOneTimePinTokenProviderLoggerExtension
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to GenerateTwoFactorToken: {Purpose} {HubIdentityUser}")]
    public static partial void FailedToGenerateToken(
        this ILogger logger,
        string purpose,
        [LogProperties] HubIdentityUser? hubIdentityUser);
    
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to GenerateTwoFactorToken: {Purpose} {HubIdentityUser} {IdentityResult}")]
    public static partial void FailedToGenerateToken(
        this ILogger logger,
        string purpose,
        [LogProperties] HubIdentityUser hubIdentityUser,
        [LogProperties] IdentityResult identityResult);
    
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to ValidateTwoFactorToken: {Purpose} {Token} {HubIdentityUser}")]
    public static partial void FailedToValidateToken(
        this ILogger logger,
        string purpose,
        string token,
        [LogProperties] HubIdentityUser? hubIdentityUser);
}

internal static partial class HubIdentityUserServiceLoggerExtension
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to Register: {RegisterRequest} {IdentityResult}")]
    public static partial void FailedToRegister(
        this ILogger logger,
        [LogProperties] RegisterRequest registerRequest,
        [LogProperties] IdentityResult identityResult);
    
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to Login: {LoginRequest} {HubIdentityUser}")]
    public static partial void FailedToLogin(
        this ILogger logger,
        [LogProperties] LoginRequest loginRequest,
        [LogProperties] HubIdentityUser? hubIdentityUser);
    
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to Login: {TimeBasedOneTimePinLoginRequest} {HubIdentityUser}")]
    public static partial void FailedToLoginUsingTimeBasedOneTimePin(
        this ILogger logger,
        [LogProperties] TimeBasedOneTimePinLoginRequest timeBasedOneTimePinLoginRequest,
        [LogProperties] HubIdentityUser? hubIdentityUser);
    
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "User not found: {userId}")]
    public static partial void UserNotFound(this ILogger logger, string userId);
    
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "User not found: {userId} {UpdateUserRequest}")]
    public static partial void UserNotFound(
        this ILogger logger,
        string userId,
        [LogProperties] UpdateUserRequest updateUserRequest);
    
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to Update User: {userId} {profileImageUri} {expirationInSeconds}")]
    public static partial void FailedToUpdateUser(
        this ILogger logger,
        string userId,
        string profileImageUri,
        int expirationInSeconds);
    
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to Verify TimeBased One-Time-Pin: {tOtpVerifyRequest} {userId} {HubIdentityUser}")]
    public static partial void FailedToVerifyTwoFactorToken(
        this ILogger logger,
        [LogProperties] TOTPVerifyRequest tOtpVerifyRequest,
        string userId,
        [LogProperties] HubIdentityUser? hubIdentityUser);
}

internal static partial class JsonWebTokenServiceLoggerExtension
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to Validate Refresh Token: {TokenRequest}")]
    public static partial void FailedToValidateRefreshToken(this ILogger logger, [LogProperties] TokenRequest tokenRequest);
    
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to Validate Refresh Token: {TokenRequest} {ClaimsPrincipal} {RefreshToken}")]
    public static partial void FailedToValidateRefreshToken(
        this ILogger logger,
        [LogProperties] TokenRequest tokenRequest,
        [LogProperties] ClaimsPrincipal claimsPrincipal,
        [LogProperties] RefreshToken? refreshToken);
}