using Client;
using Client.Manager;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddScoped<KanBanManager>()
    .AddScoped<WebAuthenticationManager>()
	.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"))
    .AddHttpClient("API", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddMudServices();

await builder.Build().RunAsync();
