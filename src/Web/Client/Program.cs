using Fluxor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SampleBlog.Web.Client;
using SampleBlog.Web.Client.Middlewares;
using SampleBlog.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    //.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>()
    .AddMudServices()
    .AddFluxor(options =>
    {
        options
            .ScanAssemblies(typeof(Program).Assembly)
            .AddMiddleware<LoggingMiddleware>()
            .UseRouting();
    })
    .AddOptions()
    .AddApiAuthorization(authorization =>
    {
        const string clientId = "blog.spa.client";
        authorization.ProviderOptions.ConfigurationEndpoint = $"{builder.HostEnvironment.BaseAddress}_configuration/authorization/{clientId}";
    });
builder.Services
    .AddScoped<AuthorizationMessageHandler, BaseAddressAuthorizationMessageHandler>()
    .AddHttpClient<BlogClient>(client =>
    {
        client.BaseAddress = new Uri("https://localhost:5001/api/v1");
    })
    .AddHttpMessageHandler(services =>
    {
        var handler = services.GetRequiredService<AuthorizationMessageHandler>();
        handler.ConfigureHandler(
            authorizedUrls: new[] { "https://localhost:5001" },
            scopes: new[] { "blog.api.blogs", "blog.api.comments" }
        );
        return handler;
    });

var host = builder.Build();
    
await host.RunAsync();