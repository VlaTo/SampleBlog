using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SampleBlog.Core.Application.Extensions;
using SampleBlog.Core.Application.Services;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.EntityFramework.Extensions;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.Infrastructure.Database;
using SampleBlog.Infrastructure.Database.Contexts;
using SampleBlog.Infrastructure.Extensions;
using SampleBlog.Infrastructure.Models.Identity;
using SampleBlog.Web.Identity.Core.Extensions;
using SampleBlog.Web.Identity.Core.Services;
using SampleBlog.Web.Shared.Core;
using IdentityOptions = SampleBlog.Web.Identity.Configuration.IdentityOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment}.json", optional: true)
    .AddUserSecrets<IdentityOptions>();

builder.Services
    //.AddTransient<ISignInService, SignInService>()
    //.AddScoped<ICurrentUserProvider, CurrentHttpUserProvider>()
    .AddSingleton<IEventQueue, EventQueueLogger>()
    //.AddSingleton<IMakeBlogPathService, MakeBlogPathService>()
    .AddTransient<ISignInService, SignInService>()
    .AddTransient<IDatabaseSeeder, BlogDatabaseSeeder>()
    //.AddApplicationOptions()
    .AddApplicationServices()
    .AddIdentityOptions()
    .AddInfrastructure();

builder.Services
    .AddJwtAuthentication(builder.Configuration)
    .AddIdentity<BlogUser, BlogUserRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddEntityFrameworkStores<BlogContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddDbContext<BlogContext>(db =>
    {
        var connectionString = builder.Configuration.GetConnectionString("Database");
        db.UseSqlServer(connectionString);
    })
    .AddIdentityServer(options =>
    {
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
            client
                .WithScopes(
                    DefinedScopes.Blog.Api.Blogs,
                    DefinedScopes.Blog.Api.Comments,
                    SampleBlog.IdentityServer.IdentityServerConstants.StandardScopes.OpenId,
                    SampleBlog.IdentityServer.IdentityServerConstants.StandardScopes.Profile
                )
                .WithRedirectUri("/redirect")
                .WithLogoutRedirectUri("/logout")
                .WithClientSecret("4u56hk435uk324h23jk4hrk2j34")
        );
    })
    .AddConfigurationStore(options =>
    {
        /*options.DefaultSchema = "http://sampleblog.net/database/configuration";
        options.IdentityResource.Name = nameof(IdentityResources);
        options.IdentityResourceClaim.Name = nameof(IdentityResourceClaim);
        options.IdentityResourceProperty.Name = nameof(IdentityResourceProperty);*/
        options.ConfigureDbContext = db =>
        {
            var connectionString = builder.Configuration.GetConnectionString("Database");
            db.UseSqlServer(connectionString);
        };
    });

builder.Services
    .AddMediatR(typeof(IdentityOptions));

// Add services to the container.
builder.Services.AddMvc(options =>
{
    options.EnableEndpointRouting = false;
    options.RespectBrowserAcceptHeader = true;
});

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

//app.UseAuthorization();
app
    .UseStaticFiles()
    .UseRouting()
    .UseMvc();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        /*var context = services.GetRequiredService<BlogContext>();

        if (context.Database.IsSqlServer())
        {
            await context.Database.EnsureCreatedAsync();
            //await context.Database.MigrateAsync();

            var seeder = scope.ServiceProvider.GetService<IDatabaseSeeder>();

            if (null != seeder)
            {
                await seeder.SeedAsync();
            }
        }*/
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogError(ex, "An error occurred while migrating or seeding the database.");

        throw;
    }
}

await app.RunAsync();
