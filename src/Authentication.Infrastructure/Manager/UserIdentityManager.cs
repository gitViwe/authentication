namespace Authentication.Infrastructure.Manager;

internal sealed class UserIdentityManager(
    HubDbContext context,
    ILogger<UserIdentityManager> logger,
    UserManager<HubIdentityUser> userManager) : IUserIdentityManager
{
    public async Task<ClaimsPrincipal?> CreateClaimsPrincipalAsync(string userId, CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByIdAsync(userId);

        if (existingUser is null)
        {
            logger.UserNotFound(userId);
            return null;
        }

        return await CreateClaimsPrincipalAsync(existingUser, cancellationToken);
    }

    public async Task<string> GenerateTimeBasedOneTimePinLinkAsync(string userId)
    {
        var existingUser = await userManager.FindByIdAsync(userId);

        if (existingUser is null)
        {
            logger.UserNotFound(userId);
            return string.Empty;
        }

        return await userManager.GenerateTwoFactorTokenAsync(existingUser, TimeBasedOneTimePinTokenProvider.ProviderKey);
    }

    public async Task<UserDetailResponse> GetUserDetailAsync(string userId)
    {
        var existingUser = await userManager.FindByIdAsync(userId);

        if (existingUser is null)
        {
            logger.UserNotFound(userId);
            return new();
        }

        return new UserDetailResponse()
        {
            FirstName = existingUser.FirstName,
            LastName = existingUser.LastName,
            Email = existingUser.Email!,
            Username = existingUser.UserName!,
            ImageUrl = existingUser.ProfileImageExpiry > DateTime.UtcNow ? existingUser.ProfileImageUri : string.Empty,
        };
    }

    public async Task<ClaimsPrincipal?> LoginUserAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);

        if (existingUser is null || false == await userManager.CheckPasswordAsync(existingUser, request.Password))
        {
            logger.FailedToLogin(request, existingUser);
            return null;
        }

        return await CreateClaimsPrincipalAsync(existingUser, cancellationToken);
    }

    public async Task<ClaimsPrincipal?> LoginUserAsync(TimeBasedOneTimePinLoginRequest request, CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);

        if (existingUser is null
            || false == await userManager.VerifyTwoFactorTokenAsync(existingUser, TimeBasedOneTimePinTokenProvider.ProviderKey, request.Token))
        {
            logger.FailedToLoginUsingTimeBasedOneTimePin(request, existingUser);
            return null;
        }

        return await CreateClaimsPrincipalAsync(existingUser, cancellationToken);
    }

    public async Task<ClaimsPrincipal?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var newUser = new HubIdentityUser { Email = request.Email, UserName = request.UserName, FirstName = request.FirstName, LastName = request.LastName };

        var result = await userManager.CreateAsync(newUser, request.Password);

        if (false == result.Succeeded)
        {
            logger.FailedToRegister(request, result);
            return null;
        }

        return await CreateClaimsPrincipalAsync(newUser, cancellationToken);
    }

    public async Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        var existingUser = await userManager.FindByIdAsync(userId);

        if (existingUser is null)
        {
            logger.UserNotFound(userId, request);
            return false;
        }

        existingUser.FirstName = request.FirstName;
        existingUser.LastName = request.LastName;

        return await UpdateUserAsync(existingUser);
    }

    public async Task<bool> UpdateUserAsync(string userId, string profileImageUri, int expirationInSeconds)
    {
        var existingUser = await userManager.FindByIdAsync(userId);

        if (existingUser is null)
        {
            logger.FailedToUpdateUser(userId, profileImageUri, expirationInSeconds);
            return false;
        }

        existingUser.ProfileImageUri = profileImageUri;
        existingUser.ProfileImageExpiry = DateTime.UtcNow.AddSeconds(expirationInSeconds);

        return await UpdateUserAsync(existingUser);
    }

    public async Task<bool> VerifyTimeBasedOneTimePinLinkAsync(TOTPVerifyRequest request, string userId, CancellationToken cancellation)
    {
        var existingUser = await userManager.FindByIdAsync(userId);
 
        if (existingUser is null
            || false == await userManager.VerifyTwoFactorTokenAsync(existingUser, TimeBasedOneTimePinTokenProvider.ProviderKey, request.Token))
        {
            logger.FailedToVerifyTwoFactorToken(request, userId, existingUser);
            return false;
        }

        return true;
    }
    
    private async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(HubIdentityUser user, CancellationToken cancellationToken)
    {
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        ];

        // get claims that are assigned to the user...
        var userClaims = await userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        // get roles that are assigned to the user
        foreach (var role in await GetRolesAsync(user, cancellationToken))
        {
            if (false == string.IsNullOrWhiteSpace(role.Name))
            {
                // add the role to the claims collection
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

                // get all claims associated with that role
                var roleClaims = role.RoleClaims.Select(x => new Claim(x.ClaimType!, x.ClaimValue!));

                claims.AddRange(roleClaims);
            }
        }

        async Task<IEnumerable<HubIdentityRole>> GetRolesAsync(HubIdentityUser user, CancellationToken cancellationToken)
        {
            // force the enumerable to execute rather than joining [cosmos db]
            var userRoleIds = await context.UserRoles
                                            .AsNoTracking()
                                            .Where(x => x.UserId == user.Id)
                                            .Select(x => x.RoleId)
                                            .ToArrayAsync(cancellationToken);

            if (userRoleIds.Length > 0)
            {
                var roles = await context.Roles
                                            .AsNoTracking()
                                            .Where(x => userRoleIds.AsEnumerable().Contains(x.Id))
                                            .ToArrayAsync(cancellationToken);

                return roles.Length > 0 ? roles : [];
            }

            return [];
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims));
    }

    private async Task<bool> UpdateUserAsync(HubIdentityUser user)
    {
        var result = await userManager.UpdateAsync(user);

        if (false == result.Succeeded)
        {
            logger.LogWarning("Failed to Update User: {HubIdentityUser} {IdentityResult}", user, result);
        }

        return result.Succeeded;
    }
}