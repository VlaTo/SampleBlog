using MediatR;
using Microsoft.AspNetCore.Mvc;
using SampleBlog.Core.Application.Extensions;
using SampleBlog.Core.Application.Services;
using SampleBlog.Core.Domain.Services;
using SampleBlog.Infrastructure.Database.Contexts;
using SampleBlog.Infrastructure.Repositories;
using SampleBlog.Web.APi.Blog.Controllers.v1;
using SampleBlog.Web.APi.Blog.Services;
using SampleBlog.Web.APi.MyDinner.Configuration;
using SampleBlog.Web.APi.MyDinner.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment}.json", optional: true)
    //.AddUserSecrets<AuthenticationController>()
    ;

builder.Services
    .AddDbContext<BlogContext>();

builder.Services
    .AddTransient<MyDinnerOptionsDefaults>()
    //.AddTransient<BlogContext>()
    .AddSingleton<IMakeBlogPathService, MakeBlogPathService>()
    .AddTransient<IBlogRepository, BlogRepository>()
    .AddTransient<IBlogService, BlogService>()
    .AddTransient<IMenuService, MenuService>()
    .AddHttpContextAccessor()
    .AddApplicationServices()
    .AddMediatR(new[] { typeof(MenuController).Assembly }, options =>
    {
        //options.
    });

builder.Services
    .AddOptions<MyDinnerOptions>()
    .BindConfiguration("Blog", options =>
    {
        options.BindNonPublicProperties = false;
    })
    .PostConfigure((MyDinnerOptions options, MyDinnerOptionsDefaults defaults) =>
    {
        if (String.IsNullOrEmpty(options.HashId.Salt))
        {
            options.HashId.Salt = defaults.DefaultHashSalt;
        }
    })
    .Validate((MyDinnerOptions options, MyDinnerOptionsDefaults defaults) =>
    {
        if (null == options.HashId)
        {
            return false;
        }

        return true;
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:5000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
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
    .UseCors()
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