using Client.Manager;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Shared;
using System.ComponentModel.DataAnnotations;

namespace Client.Pages;

public partial class Home
{
    [Inject] public required WebAuthenticationManager WebAuthenticationManager { get; set; }
    [Inject] public required KanBanManager KanBanManager { get; set; }
	[Inject] public required IDialogService DialogService { get; set; }
    public UserData Model { get; set; } = new(string.Empty, string.Empty);

    private bool _processing = false;
    private IJSObjectReference module = default!;
    private AuthenticationType _authenticationType = AuthenticationType.Register;
    private enum AuthenticationType
    {
        Register = 1,
        Login = 2,
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/web_authentication.js");
            ArgumentNullException.ThrowIfNull(module, nameof(module));
        }
    }

    private async Task RegisterAsync()
    {
        _processing = true;

        _ = await WebAuthenticationManager.ProcessRegistrationAsync(registrationId: Guid.NewGuid().ToString(), Model.RegisterUserEmail);

        _processing = false;
    }

    private async Task AuthenticateAsync()
    {
        _processing = true;

        string userHandleBase64 = await WebAuthenticationManager.ProcessAuthenticationAsync(authenticationId: Guid.NewGuid().ToString());

        _processing = false;

        await ShowDialog(userHandleBase64);
	}

    private async Task ShowDialog(string userHandleBase64)
    {
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true, DisableBackdropClick = true };

		var data = await KanBanManager.GetUserDetailAsKanBanDialogDataAsync(userHandleBase64);

        if (data is not null)
        {
            var parameters = new DialogParameters<KanBanDialog>
            {
                { x => x.Model, data }
            };

            var dialogReference = DialogService.Show<KanBanDialog>($"Welcome {data.UserName}", parameters, options);
            var result = await dialogReference.Result;

            if (false == result.Canceled)
            {
                await KanBanManager.UpdateKanBanDataAsync((KanBanDialogData)result.Data);
            } 
        }
    }
}

public class UserData(string RegisterUserName, string AuthenticateUserName)
{
    [EmailAddress]
    public string RegisterUserEmail { get; set; } = RegisterUserName;
    public string AuthenticateUserName { get; set; } = AuthenticateUserName;
}


