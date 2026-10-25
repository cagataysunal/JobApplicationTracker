using JobTracker.Web;
using JobTracker.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// The API runs on its own origin; its address comes from wwwroot/appsettings.json.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");

builder.Services.AddScoped(_ => new ApiClient(new HttpClient { BaseAddress = new Uri(apiBaseUrl) }));

await builder.Build().RunAsync();
