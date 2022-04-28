using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleBlog.Core.Application.Configuration;
using SampleBlog.Core.Application.Extensions;
using SampleBlog.Core.Application.Services;
using SampleBlog.IdentityServer.DependencyInjection.Extensions;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.EntityFramework.Extensions;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.Infrastructure.Database;
using SampleBlog.Infrastructure.Database.Contexts;
using SampleBlog.Infrastructure.Extensions;
using SampleBlog.Infrastructure.Models.Identity;
using SampleBlog.Web.Server.Controllers.Identity;
using SampleBlog.Web.Server.Extensions;
using SampleBlog.Web.Server.Services;
using SampleBlog.Web.Shared.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment}.json", optional: true)
    .AddUserSecrets<AuthenticationController>();

// Add services to the container.
builder.Services
    .AddServerOptions()
    .AddJwtAuthentication(builder.Configuration)
    .AddIdentity<BlogUser, BlogUserRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddEntityFrameworkStores<BlogContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddDbContext<BlogContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("Database");
        options.UseSqlServer(connectionString);
    })
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

/*builder.Services.AddDbContext<BlogContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});*/

builder.Services.AddMvc(options =>
{
    options.EnableEndpointRouting = true;
});

builder.Services.AddRazorPages();

builder.Services
    .AddMediatR(typeof(ApplicationOptions).Assembly)
    .AddApplicationOptions()
    .AddApplicationServices()
    .AddInfrastructure();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services
    .AddHttpContextAccessor()
    .AddScoped<ICurrentUserProvider, CurrentHttpUserProvider>()
    .AddSingleton<IEventQueue, LogEventQueue>()
    .AddSingleton<IMakeBlogPathService, MakeBlogPathService>()
    .AddTransient<ISignInService, SignInService>()
    .AddTransient<IDatabaseSeeder, BlogDatabaseSeeder>();

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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<BlogContext>();

        if (context.Database.IsSqlServer())
        {
            await context.Database.EnsureCreatedAsync();
            await context.Database.MigrateAsync();

            var seeder = scope.ServiceProvider.GetService<IDatabaseSeeder>();

            if (null != seeder)
            {
                await seeder.SeedAsync();
            }
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogError(ex, "An error occurred while migrating or seeding the database.");

        throw;
    }
}

await app.RunAsync();