using MediatR;
using Microsoft.AspNetCore.Mvc;
using SampleBlog.Core.Application.Extensions;
using SampleBlog.Core.Application.Services;
using SampleBlog.Core.Domain.Services;
using SampleBlog.Web.APi.Blog.Configuration;
using SampleBlog.Web.APi.Blog.Controllers.v1;
using SampleBlog.Web.APi.Blog.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment}.json", optional: true)
    //.AddUserSecrets<AuthenticationController>()
    ;

builder.Services
    .AddTransient<BlogOptionsDefaults>()
    .AddSingleton<IBlogService, BlogService>()
    .AddSingleton<IMakeBlogPathService, MakeBlogPathService>()
    .AddHttpContextAccessor()
    .AddApplicationServices()
    .AddMediatR(new[] { typeof(BlogController).Assembly }, options =>
    {
        //options.
    });

builder.Services
    .AddOptions<BlogOptions>()
    .BindConfiguration("Blog", options =>
    {
        options.BindNonPublicProperties = false;
    })
    .PostConfigure((BlogOptions options, BlogOptionsDefaults defaults) =>
    {
        if (String.IsNullOrEmpty(options.HashId.Salt))
        {
            options.HashId.Salt = defaults.DefaultHashSalt;
        }
    })
    .Validate((BlogOptions options, BlogOptionsDefaults defaults) =>
    {
        if (null == options.HashId)
        {
            return false;
        }

        return true;
    });

builder.Services.AddControllers(options =>
{
    options.EnableEndpointRouting = true;
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

app
    .UseRouting()
    //.UseIdentityServer()
    //.UseAuthentication()
    //.UseAuthorization()
    //.UseMvc()
    .UseApiVersioning()
    ;

app.MapControllers();

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