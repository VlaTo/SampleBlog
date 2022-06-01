using Fluxor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SampleBlog.Web.Client;
using SampleBlog.Web.Client.Middlewares;
using SampleBlog.Web.Client.Services;
using SampleBlog.Web.Shared.Extensions;

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
    .AddOptions();

builder.Services
    .AddOidcAuthentication(options =>
    {
        builder.Configuration.Bind("Local", options.ProviderOptions);
    });

/*builder.Services
    .AddAuthorizationCore(authorization =>
    {
        authorization.AddPolicy(
            "blog-editor",
            policy => policy.RequirePermission("Permissions.Blog.Create", "Permissions.Blog.Edit", "Permissions.Blog.View")
        );
    })
    .AddApiAuthorization(authorization =>
    {
        const string clientId = "blog.spa.client";
        //authorization.ProviderOptions.ConfigurationEndpoint = $"{builder.HostEnvironment.BaseAddress}_configuration/authorization/{clientId}";
        authorization.ProviderOptions.ConfigurationEndpoint = $"http://localhost:5276/_configuration/authorization/{clientId}";

        authorization.UserOptions.AuthenticationType = "SampleBlog.IdentityServer";

        authorization.AuthenticationPaths.LogInPath = "/authentication/login";
        authorization.AuthenticationPaths.LogInCallbackPath = "/authentication/cb";
        authorization.AuthenticationPaths.ProfilePath = "/authentication/profile";
    })
    ;*/

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