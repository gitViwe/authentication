using MudBlazor;
using Shared;
using System.Net.Http.Json;

namespace Client.Manager;

public class KanBanManager(IHttpClientFactory Factory, ISnackbar snackbar)
{
	public HttpClient Client { get; } = Factory.CreateClient("API");

	public async Task<KanBanDialogData?> GetUserDetailAsKanBanDialogDataAsync(string userHandleBase64)
	{
		var response = await Client.GetAsync($"/users/{userHandleBase64}", CancellationToken.None);

		if (response.IsSuccessStatusCode)
		{
			var detail = await response.Content.ReadFromJsonAsync<UserDetailDTO>();

			return new KanBanDialogData()
			{
				Id = detail.Id,
				UserName = detail.UserName,
				KanBanSections = detail.KanBanSections,
				KanBanTaskItems = detail.KanBanTaskItems,
			};
		}

		snackbar.Add(response.ReasonPhrase, Severity.Error);

		return null;
	}

	public async Task UpdateKanBanDataAsync(KanBanDialogData data)
	{
		var response = await Client.PutAsJsonAsync("kanban", data, CancellationToken.None);

		if (response.IsSuccessStatusCode)
		{
			snackbar.Add("Updated.", Severity.Info);
		}
		else
		{
			snackbar.Add(response.ReasonPhrase, Severity.Error);
		}
	}
}
