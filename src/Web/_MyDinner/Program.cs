
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SampleBlog.Core.Application.Extensions;
using SampleBlog.Core.Application.Services;
using SampleBlog.IdentityServer;
using SampleBlog.IdentityServer.DependencyInjection.Extensions;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.EntityFramework.Extensions;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.Infrastructure.Database;
using SampleBlog.Infrastructure.Database.Contexts;
using SampleBlog.Infrastructure.Extensions;
using SampleBlog.Infrastructure.Models.Identity;
using SampleBlog.Web.MyDinner.Core;
using SampleBlog.Web.MyDinner.Core.Extensions;
using SampleBlog.Web.MyDinner.Core.Services;
using SampleBlog.Web.Shared.Core;
using IdentityOptions = SampleBlog.Web.MyDinner.Configuration.IdentityOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment}.json", optional: true)
    .AddUserSecrets<IdentityOptions>();

builder.Services
    .AddSingleton<IEventQueue, EventQueueLogger>()
    .AddTransient<ISignInService, SignInService>()
    .AddTransient<IDatabaseSeeder, BlogDatabaseSeeder>()
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
    /*.Configure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })*/
    .ConfigureApplicationCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

builder.Services
    .AddDbContext<BlogContext>(db =>
    {
        var connectionString = builder.Configuration.GetConnectionString("Database");
        db.UseSqlServer(connectionString);
    })
    .AddIdentityServer(options =>
    {
        options.UserInteraction.LoginUrl = "http://localhost:5276/Authenticate/login";

        options.Events = new EventsOptions
        {
            RaiseErrorEvents = true,
            RaiseFailureEvents = true,
            RaiseInformationEvents = true,
            RaiseSuccessEvents = true
        };

        options.Authentication.CookieAuthenticationScheme = IdentityServerConstants.DefaultCookieAuthenticationScheme;
        options.Authentication.CookieLifetime = TimeSpan.FromMinutes(10.0d);
        options.Authentication.CookieSlidingExpiration = true;
    })
    .AddApiAuthorization<BlogUser, BlogContext>(options =>
    {
        options.Clients.AddSPA("blog.spa.client", client =>
            client
                .WithScopes(
                    DefinedScopes.Blog.Api.Blogs,
                    DefinedScopes.Blog.Api.Comments,
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile
                )
        );
    })
    .AddJwtBearerClientAuthentication()
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = db =>
        {
            var connectionString = builder.Configuration.GetConnectionString("Database");
            db.UseSqlServer(connectionString);
        };
    });

builder.Services
    .AddCors(options =>
    {
        options.AddPolicy(
            name: Constants.ClientPolicy,
            configurePolicy: policy =>
            {
                policy
                    .WithOrigins("https://localhost:5000")
                    .AllowAnyMethod();
            }
        );
    });

builder.Services
    .AddMediatR(typeof(IdentityOptions));

// Add services to the container.
/*builder.Services.AddMvc(options =>
{
    options.EnableEndpointRouting = false;
    options.RespectBrowserAcceptHeader = true;
});*/

builder.Services.AddControllersWithViews(options =>
{
    options.EnableEndpointRouting = false;
    options.RespectBrowserAcceptHeader = true;
});
builder.Services.AddRazorPages(options =>
{

});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app
        .UseMigrationsEndPoint()
        .UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
}

app
    .UseCors()
    .UseIdentityServer()
    .UseAuthentication()
    .UseAuthorization()
    .UseBlazorFrameworkFiles()
    .UseStaticFiles()
    .UseRouting();
    //.UseMvc();

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
            //await context.Database.MigrateAsync();

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
