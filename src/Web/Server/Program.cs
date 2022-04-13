using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using SampleBlog.Core.Application.Configuration;
using SampleBlog.Core.Application.Extensions;
using SampleBlog.Core.Application.Models.Identity;
using SampleBlog.Core.Application.Services;
using SampleBlog.IdentityServer.DependencyInjection.Extensions;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.EntityFramework.Extensions;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.Infrastructure.Database.Contexts;
using SampleBlog.Infrastructure.Extensions;
using SampleBlog.Web.Server.Extensions;
using SampleBlog.Web.Server.Services;
using SampleBlog.Web.Shared.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddServerOptions()
    .AddJwtAuthentication(builder.GetServerOptions())
    .AddIdentity<BlogUser, BlogUserRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddEntityFrameworkStores<BlogContext>()
    .AddDefaultTokenProviders();
builder.Services
    .AddIdentityServer(options =>
    {
        options.IssuerUri = "http://sampleblog.net";

        options.Events = new EventsOptions
        {
            RaiseErrorEvents = true,
            RaiseFailureEvents = true,
            RaiseInformationEvents = true,
            RaiseSuccessEvents = true
        };
    })
    .AddApiAuthorization<BlogUser, BlogContext>(options =>
    {
        options.Clients.AddSPA("blog.spa.client", client =>
        {
            client
                .WithScopes(DefinedScopes.Blog.Api.Blogs, DefinedScopes.Blog.Api.Comments, "profile", "id")
                .WithRedirectUri("/redirect")
                .WithLogoutRedirectUri("/logout")
                ;
        });
    })
    .AddConfigurationStore(options =>
    {
        options.DefaultSchema = "http://sampleblog.net/database/configuration";
    });
builder.Services.AddDbContext<BlogContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services
    .AddMediatR(typeof(ApplicationOptions).Assembly)
    .AddApplicationOptions()
    .AddApplicationServices()
    .AddInfrastructure()
    ;
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
builder.Services
    .AddHttpContextAccessor()
    .AddScoped<ICurrentUserProvider, CurrentHttpUserProvider>()
    .AddSingleton<IMqttFactory, MqttFactory>()
    .AddSingleton<IEventQueueProvider, MqttEventQueueProvider>()
    .AddSingleton<IMakeBlogPathService, MakeBlogPathService>()
    .AddTransient<ISignInService, EntityFrameworkSignInService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app
    .UseBlazorFrameworkFiles()
    .UseStaticFiles()
    .UseRouting()
    .UseIdentityServer()
    .UseAuthorization()
    .UseAuthentication();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

await app.RunAsync();