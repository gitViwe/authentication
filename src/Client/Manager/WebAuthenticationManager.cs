using Client.Json.AuthenticationCeremony.CreateOptions;
using Client.Json.AuthenticationCeremony.VerifyAssertion;
using Client.Json.RegistrationCeremony.CreateCredential;
using Client.Json.RegistrationCeremony.CreateOptions;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net.Http.Json;

namespace Client.Manager;

public class WebAuthenticationManager(IHttpClientFactory Factory, ISnackbar snackbar, IJSRuntime runtime)
{
    public HttpClient Client { get; } = Factory.CreateClient("API");

    public async Task<string> ProcessRegistrationAsync(string registrationId, string userName)
    {
        try
        {
            var options = await GetRegistrationOptionsAsync(registrationId, userName);

            var response = await runtime.InvokeAsync<RegistrationResponseJSON>("ProcessRegistration", [options]);

            return await CompleteRegistrationAsync(response, userName);
        }
        catch (Exception ex)
        {
            snackbar.Add(ex.Message, Severity.Error);
            return string.Empty;
        }
    }

    public async Task<string> ProcessAuthenticationAsync(string authenticationId)
    {
        try
        {
            var options = await GetAuthenticationOptionsAsync(authenticationId);

            var response = await runtime.InvokeAsync<AuthenticationResponseJSON>("ProcessAuthentication", [options]);

            return await CompleteAuthenticationAsync(response);
        }
        catch (Exception ex)
        {
            snackbar.Add(ex.Message, Severity.Error);
            return string.Empty;
        }
    }

    private async Task<PublicKeyCredentialCreationOptionsJSON> GetRegistrationOptionsAsync(string registrationId, string userName)
    {
        Client.DefaultRequestHeaders.Remove("X-WebAuthn-Registration-Id");
        Client.DefaultRequestHeaders.Add("X-WebAuthn-Registration-Id", registrationId);

        var options = await Client.GetFromJsonAsync<PublicKeyCredentialCreationOptionsJSON>($"registration-options?UserName={userName}", CancellationToken.None);
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        return options;
    }

    private async Task<string> CompleteRegistrationAsync(RegistrationResponseJSON json, string userName)
    {
        var responseMessage = await Client.PostAsJsonAsync($"complete-registration?UserName={userName}", json, CancellationToken.None);

        if (responseMessage.IsSuccessStatusCode)
        {
            string userHandleBase64 = await responseMessage.Content.ReadAsStringAsync();
		    snackbar.Add("Registered.", Severity.Info);
		    return userHandleBase64;
        }

        throw new Exception("Invalid credentials");
    }

    private async Task<PublicKeyCredentialRequestOptionsJSON> GetAuthenticationOptionsAsync(string authenticationId)
    {
        Client.DefaultRequestHeaders.Remove("X-WebAuthn-Authentication-Id");
        Client.DefaultRequestHeaders.Add("X-WebAuthn-Authentication-Id", authenticationId); 

        var options = await Client.GetFromJsonAsync<PublicKeyCredentialRequestOptionsJSON>("authentication-options", CancellationToken.None);
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        return options;
    }

    private async Task<string> CompleteAuthenticationAsync(AuthenticationResponseJSON json)
    {
        var responseMessage = await Client.PostAsJsonAsync("complete-authentication", json, CancellationToken.None);

        if (responseMessage.IsSuccessStatusCode)
        {
            string userHandleBase64 = await responseMessage.Content.ReadAsStringAsync();
            return userHandleBase64;
        }

        throw new Exception("Invalid credentials");
    }
}
