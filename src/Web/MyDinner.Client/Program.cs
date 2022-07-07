using Fluxor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SampleBlog.Web.Client;
using SampleBlog.Web.Client.Core.Configuration;
using SampleBlog.Web.Client.Core.Services;
using SampleBlog.Web.Client.Middleware;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddSingleton<ICurrentDateTimeProvider, CurrentDateTimeProvider>();

builder.Services
    .AddOidcAuthentication(options =>
    {
        builder.Configuration.Bind("Oidc", options.ProviderOptions);
    });

builder.Services
    .AddOptions<ClientOptions>()
    .Configure(options =>
    {
        builder.Configuration.Bind("HashIds", options.HashIdOptions);
    });

builder.Services
    .AddMudServices()
    .AddFluxor(options =>
    {
        options
            .ScanAssemblies(typeof(Program).Assembly)
            .AddMiddleware<LoggingMiddleware>()
            .UseRouting();
    });

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
