using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Authentication.Shared.Constant;
using Authentication.Shared.Contract;
using Authentication.Test.Configuration;
using gitViwe.Shared;
using gitViwe.Shared.Extension;

namespace Authentication.Test;

public class AuthenticationTests(BaseIntegrationFixture integrationFixture) : BaseIntegrationTest(integrationFixture)
{
    [Fact]
    public async Task LoginAndGetUserDetail()
    {
        // Act
        var (loginRequest, registerRequest, loginResult) = await PerformLoginWithRegisterFallback(IntegrationFixture.AuthenticationClient);

        // Assert
        Assert.True(loginResult.IsSuccessStatusCode, "LoginResponse must be a success status code");

        var loginResponse = await loginResult.ToResponseAsync<TokenResponse>();
        Assert.NotNull(loginResponse);
        Assert.False(string.IsNullOrWhiteSpace(loginResponse.Token), "LoginResponse.Token must contain a value");
        Assert.False(string.IsNullOrWhiteSpace(loginResponse.RefreshToken), "LoginResponse.RefreshToken must contain a value");
        
        // Act
        IntegrationFixture.AuthenticationClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var detailResult = await IntegrationFixture.AuthenticationClient.GetAsync("account/detail");

        // Assert
        Assert.True(detailResult.IsSuccessStatusCode, "DetailResult must be a success status code");

        var detailResponse = await detailResult.ToResponseAsync<UserDetailResponse>();
        Assert.NotNull(detailResponse);
        Assert.False(string.IsNullOrWhiteSpace(detailResponse.Email), "UserDetailResponse.Email must contain a value");
        Assert.Equal(loginRequest.Email, detailResponse.Email);
        Assert.False(string.IsNullOrWhiteSpace(detailResponse.Username), "UserDetailResponse.Username must contain a value");
        Assert.Equal(registerRequest?.UserName ?? "username", detailResponse.Username);
    }
    
    [Fact]
    public async Task GetUserDetailWithoutLoggingInReturnsUnauthorized()
    {
        // Arrange
        IntegrationFixture.AuthenticationClient.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await IntegrationFixture.AuthenticationClient.GetAsync("account/detail");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task GetLoginOptions_ReturnsAvailableFlows_ForRegisteredUser()
    {
        // Arrange
        var (loginRequest, _, _) = await PerformLoginWithRegisterFallback(IntegrationFixture.AuthenticationClient);

        // Act
        var httpResponse = await IntegrationFixture.AuthenticationClient
            .GetAsync($"account/login/{Uri.EscapeDataString(loginRequest.Email)}");

        // Assert
        Assert.NotNull(httpResponse);
        Assert.True(httpResponse.IsSuccessStatusCode,
            $"Expected 2xx but got {(int)httpResponse.StatusCode} ({httpResponse.StatusCode}).");

        var result = await httpResponse.Content.ReadFromJsonAsync<LoginOptionsResponse>();

        Assert.NotNull(result);
        Assert.NotNull(result);
        Assert.NotNull(result.AvailableFlows);

        Assert.NotEmpty(result.AvailableFlows);
        Assert.Contains(HubLoginFlows.Password, result.AvailableFlows);

        // Sanity: no unknown/empty flow entries leaked through.
        Assert.All(result.AvailableFlows, flow => Assert.False(string.IsNullOrWhiteSpace(flow)));
    }
    
    [Fact]
public async Task UpdateUserDetail_ThenGetDetail_ReturnsUpdatedValues()
{
    // Arrange - guarantee a user + a valid access token (same helper used by other tests)
    var (loginRequest, registerRequest, loginResult) =
        await PerformLoginWithRegisterFallback(IntegrationFixture.AuthenticationClient);

    var loginResponse = await loginResult.ToResponseAsync<TokenResponse>();
    Assert.NotNull(loginResponse);
    Assert.False(string.IsNullOrWhiteSpace(loginResponse.Token),
        "Expected a bearer token from PerformLoginWithRegisterFallback.");

    // Authenticated client (bearer token from the login/register step)
    IntegrationFixture.AuthenticationClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", loginResponse.Token);

    // New details to push
    var updateRequest = new UpdateUserRequest
    {
        FirstName = $"First-{Generator.RandomString(CharacterCombination.Alphabet, 6)}",
        LastName  = $"Last-{Generator.RandomString(CharacterCombination.Alphabet, 6)}",
    };

    // Act 1 - PUT account/detail  (accountGroup.MapPut("detail", User.UpdateDetailsAsync))
    var updateResponse = await IntegrationFixture.AuthenticationClient
        .PutAsJsonAsync("account/detail", updateRequest);

    // Assert - 204 No Content, per .Produces(StatusCodes.Status204NoContent)
    Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

    // Act 2 - GET account/detail  (accountGroup.MapGet("detail", User.DetailAsync))
    var detailResponse = await IntegrationFixture.AuthenticationClient
        .GetAsync("account/detail");

    Assert.True(detailResponse.IsSuccessStatusCode,
        $"Expected 2xx but got {(int)detailResponse.StatusCode} ({detailResponse.StatusCode}).");

    // Assert - typed payload matches what we just PUT
    var detail = await detailResponse.Content.ReadFromJsonAsync<UserDetailResponse>();

    Assert.NotNull(detail);
    Assert.Equal(updateRequest.FirstName, detail!.FirstName);
    Assert.Equal(updateRequest.LastName,  detail.LastName);

    // Sanity: identity-bound fields must remain the ones we logged in with
    Assert.Equal(loginRequest.Email, detail.Email);
    Assert.Equal(registerRequest.UserName, detail.Username);
}
    
    private static async Task<(RegisterRequest, HttpResponseMessage)> PerformRegister(HttpClient authClient, Action<RegisterRequest>? requestTransform = null)
    {
        var registerRequest = new RegisterRequest
        {
            Email = "example@mail.com",
            Password = "Password",
            PasswordConfirmation = "Password",
            UserName = "username",
        };

        if (requestTransform is not null)
        {
            requestTransform(registerRequest);
        }

        return (registerRequest, await authClient.PostAsJsonAsync("account/register", registerRequest));
    }

    private static async Task<(LoginRequest, RegisterRequest?, HttpResponseMessage)> PerformLoginWithRegisterFallback(HttpClient authClient, Action<LoginRequest>? requestTransform = null)
    {
        var loginRequest = new LoginRequest
        {
            Email = "example@mail.com",
            Password = "Password",
        };

        if (requestTransform is not null)
        {
            requestTransform(loginRequest);
        }

        var response = await authClient.PostAsJsonAsync("account/login", loginRequest);

        if (response.IsSuccessStatusCode) return (loginRequest, null, response);

        var (registerRequest, _) = await PerformRegister(authClient, register =>
        {
            register.Email = $"{Generator.RandomString(CharacterCombination.Alphabet)}@mail.com";
            register.UserName = Generator.RandomString(CharacterCombination.Alphabet);
        });

        loginRequest.Email = registerRequest.Email;

        return (loginRequest, registerRequest, await authClient.PostAsJsonAsync("account/login", loginRequest));
    }
}