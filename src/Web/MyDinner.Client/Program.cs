using Fluxor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SampleBlog.Web.Application.MyDinner.Client;
using SampleBlog.Web.Application.MyDinner.Client.Middlewares;
using SampleBlog.Web.Application.MyDinner.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddMudServices()
    .AddFluxor(options =>
    {
        options
            .ScanAssemblies(typeof(Program).Assembly)
            .AddMiddleware<LoggingMiddleware>()
            .UseRouting();
    })
    .AddOptions();

builder.Services
    .AddOidcAuthentication(options =>
    {
        options.AuthenticationPaths.RemoteRegisterPath = "/register";
        builder.Configuration.Bind("Local", options.ProviderOptions);
    });

builder.Services.AddApiAuthorization();

/*
builder.Services.AddHttpClient("BlazorApp1.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BlazorApp1.ServerAPI"));
*/

builder.Services
    .AddScoped<AuthorizationMessageHandler, BaseAddressAuthorizationMessageHandler>()
    .AddHttpClient<BlogClient>(client =>
    {
        client.BaseAddress = new Uri("http://localhost:5026/api/v1");
    })
    .AddHttpMessageHandler(services =>
    {
        var handler = services.GetRequiredService<AuthorizationMessageHandler>();
        handler.ConfigureHandler(
            authorizedUrls: new[] { "http://localhost:5026/api/" },
            scopes: new[] { "blog.api.blogs", "blog.api.comments" }
        );
        return handler;
    });

var host = builder.Build();

await host.RunAsync();