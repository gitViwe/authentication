namespace Authentication.Application.Manager;

public interface IUserIdentityManager
{
    Task<ClaimsPrincipal?> CreateClaimsPrincipalAsync(string userId, CancellationToken cancellationToken);
    Task<string> GenerateTimeBasedOneTimePinLinkAsync(string userId);
    Task<UserDetailResponse> GetUserDetailAsync(string userId);
    Task<ClaimsPrincipal?> LoginUserAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<ClaimsPrincipal?> LoginUserAsync(TimeBasedOneTimePinLoginRequest request, CancellationToken cancellationToken);
    Task<ClaimsPrincipal?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request);
    Task<bool> UpdateUserAsync(string userId, string profileImageUri, int expirationInSeconds);
    Task<bool> VerifyTimeBasedOneTimePinLinkAsync(TOTPVerifyRequest request, string userId, CancellationToken cancellation);
}