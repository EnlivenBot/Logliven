using Logliven.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddServiceDiscovery();
builder.Services.ConfigureHttpClientDefaults(static http =>
{
    http.AddServiceDiscovery();
});

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
builder.Services.AddScoped<IGuildService, HttpGuildService>();
builder.Services.AddHttpClient<IGuildService, HttpGuildService>(
    client =>
    {
        client.BaseAddress = new Uri("https+http://logliven");
    });


await builder.Build().RunAsync();