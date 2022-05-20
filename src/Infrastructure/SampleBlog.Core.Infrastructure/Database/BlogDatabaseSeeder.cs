using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SampleBlog.Core.Application.Services;
using SampleBlog.Infrastructure.Models.Identity;
using SampleBlog.Shared;
using SampleBlog.Shared.Contracts;
using SampleBlog.Shared.Contracts.Permissions;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SampleBlog.IdentityServer.EntityFramework.Storage.Entities;
using SampleBlog.IdentityServer.EntityFramework.Storage.Stores;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.Infrastructure.Database.Contexts;

namespace SampleBlog.Infrastructure.Database;

public sealed class BlogDatabaseSeeder : IDatabaseSeeder
{
    private readonly UserManager<BlogUser> userManager;
    private readonly RoleManager<BlogUserRole> roleManager;

    private readonly ClientStore clientStore;

    private readonly BlogContext context;
    private readonly ILogger<BlogDatabaseSeeder> logger;

    public BlogDatabaseSeeder(
        UserManager<BlogUser> userManager,
        RoleManager<BlogUserRole> roleManager,
        BlogContext context,
        ClientStore clientStore,
        ILogger<BlogDatabaseSeeder> logger)
    {
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.clientStore = clientStore;
        this.context = context;
        this.logger = logger;
    }

    public async Task SeedAsync()
    {
        await TryAddAdministratorAsync();
        await TryAddClientsAsync();
    }

    private async Task TryAddAdministratorAsync()
    {
        var adminRole = await roleManager.FindByNameAsync(RoleNames.Administator);

        if (null == adminRole)
        {
            adminRole = new BlogUserRole(RoleNames.Administator, "Administrator role with full permissions")
            {
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            };

            var result = await roleManager.CreateAsync(adminRole);

            if (false == result.Succeeded)
            {
                throw new Exception();
            }

            logger.LogInformation("Seeded Administrator Role.");
        }

        const string email = "superuser@sampleblog.net";
        var superUser = await userManager.FindByEmailAsync(email);

        if (null == superUser)
        {
            superUser = new BlogUser
            {
                Email = email,
                UserName = "superuser",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(superUser);

            if (false == result.Succeeded)
            {
                throw new Exception();
            }

            logger.LogInformation("Seeded Default SuperUser with email: {email}", email);

            if (userManager.SupportsUserPassword)
            {
                result = await userManager.AddPasswordAsync(superUser, "a1B2c.3");

                if (false == result.Succeeded)
                {
                    throw new Exception();
                }

                logger.LogInformation("Adding default password for SuperUser");
            }
            else
            {
                logger.LogInformation("Users don't support passwords");
            }

            if (userManager.SupportsUserRole)
            {
                result = await userManager.AddToRoleAsync(superUser, RoleNames.Administator);

                if (false == result.Succeeded)
                {
                    throw new Exception();
                }

                logger.LogInformation("Adding SuperUser to role: {role}", RoleNames.Administator);
            }
            else
            {
                logger.LogInformation("Users don't support roles");
            }

            foreach (var permission in Permissions.GetRegisteredPermissions())
            {
                result = await AddPermissionClaim(adminRole, permission);

                if (false == result.Succeeded)
                {
                    throw new Exception();
                }
            }
        }
    }

    private async Task TryAddClientsAsync()
    {
        const string clientId = "blog.spa.client";

        var exists = context.Clients
            .Where(client => client.ClientId == clientId)
            .AsNoTracking()
            .Any();

        if (!exists)
        {
            var client = new Client
            {
                AccessTokenType = AccessTokenType.Jwt,
                ClientClaimsPrefix = "sample_blog_",
                ClientId = clientId,
                ClientName = "Sample Blog SPA Client",
                ClientUri = "http://localhost:5000"
            };

            var result = await context.Clients.AddAsync(client);

            await context.SaveChangesAsync();
        }
    }

    private async Task<IdentityResult> AddPermissionClaim(BlogUserRole userRole, string permission)
    {
        var allClaims = await roleManager.GetClaimsAsync(userRole);

        if (false == allClaims.Any(claim => String.Equals(claim.Type, ApplicationClaimTypes.Permission) && String.Equals(claim.Value, permission)))
        {
            var claim = new Claim(ApplicationClaimTypes.Permission, permission);
            return await roleManager.AddClaimAsync(userRole, claim);
        }

        return IdentityResult.Failed();
    }
}