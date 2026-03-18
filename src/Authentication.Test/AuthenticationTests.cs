using System.Net.Http.Headers;
using System.Net.Http.Json;
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