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
using Client = SampleBlog.IdentityServer.EntityFramework.Storage.Entities.Client;
using ApiResource = SampleBlog.IdentityServer.EntityFramework.Storage.Entities.ApiResource;
using ApiScope = SampleBlog.IdentityServer.EntityFramework.Storage.Entities.ApiScope;

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
        await TryAddApiResourcesAsync();
        await TryAddApiScopesAsync();
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

        var client = await context.Clients
            .Where(client => client.ClientId == clientId)
            .AsNoTracking()
            .SingleOrDefaultAsync();

        if (null != client)
        {
            return;
        }

        client = new Client
        {
            ClientId = clientId,
            ClientName = "Sample Blog SPA Client",
            ClientUri = "http://localhost:5000",
            AllowedIdentityTokenSigningAlgorithms = "RS256",
            AccessTokenType = AccessTokenType.Jwt,
            ClientClaimsPrefix = "sample_blog_",
            Description = "Sample Blog SPA Client",
            BackChannelLogoutUri = "http://localhost:5000/back-logout",
            FrontChannelLogoutUri = "http://localhost:5000/front-logout",
            LogoUri = "http://localhost:5000/logo.png",
            PairWiseSubjectSalt = "k356hr3k45h6rl32kh456lk536h3k4lhr",
            UserCodeType = "Numeric",
            EnableLocalLogin = true
        };

        client.AllowedGrantTypes = new List<ClientGrantType>(new[]
        {
            new ClientGrantType { Client = client, GrantType = "authorization_code" },
            new ClientGrantType { Client = client, GrantType = "client_credentials" },
            new ClientGrantType { Client = client, GrantType = "refresh_token" }
        });

        client.AllowedScopes = new List<ClientScope>(new[]
        {
            new ClientScope { Client = client, Scope = "openid" },
            new ClientScope { Client = client, Scope = "profile" },
            new ClientScope { Client = client, Scope = "email" },
            new ClientScope { Client = client, Scope = "address" },
            new ClientScope { Client = client, Scope = "blog.api.blogs" },
            new ClientScope { Client = client, Scope = "blog.api.comments" },
        });

        client.RedirectUris = new List<ClientRedirectUri>(new[]
        {
            new ClientRedirectUri { Client = client, RedirectUri = "http://localhost:5000/redirect" }
        });

        client.ClientSecrets = new List<ClientSecret>(new[]
        {
            new ClientSecret
            {
                Client = client,
                Type = "SharedSecret",
                Value = "4u56hk435uk324h23jk4hrk2j34",
                Created = DateTime.UtcNow,
                Description = "Sample user secret"
            }
        });

        var result = await context.Clients.AddAsync(client);

        await context.SaveChangesAsync();
    }

    private async Task TryAddApiResourcesAsync()
    {
        const string name = "SampleBlog API";

        var blogsApi = await context.ApiResources
            .Where(resource => resource.Name == name)
            .SingleOrDefaultAsync();

        if (null != blogsApi)
        {
            return;
        }

        blogsApi = new ApiResource
        {
            Name = name,
            DisplayName = "SampleBlog API",
            Description = "SampleBlog Blogs API",
            Enabled = true,
            Created = DateTime.UtcNow,
            ShowInDiscoveryDocument = true,
            AllowedAccessTokenSigningAlgorithms = "RS256"
        };

        blogsApi.Scopes = new List<ApiResourceScope>(new[]
        {
            new ApiResourceScope { ApiResource = blogsApi, Scope = "blog.api.blogs" },
            new ApiResourceScope { ApiResource = blogsApi, Scope = "blog.api.comments" },
        });

        var result = await context.ApiResources.AddAsync(blogsApi);
        
        await context.SaveChangesAsync();
    }
    
    private async Task TryAddApiScopesAsync()
    {
        const string apiBlogs = "blog.api.blogs";
        const string apiComments = "blog.api.comments";
        const string openId = "openid";
        const string profile = "profile";

        #region blog.api.blogs

        var apiScope = await context.ApiScopes
            .Where(scope => scope.Name == apiBlogs)
            .SingleOrDefaultAsync();

        if (null == apiScope)
        {
            apiScope = new ApiScope
            {
                Name = apiBlogs,
                DisplayName = "SampleBlog API scope",
                Description = "SampleBlog Blogs API scope",
                Enabled = true,
                Created = DateTime.UtcNow,
                ShowInDiscoveryDocument = true,
                Required = false
            };

            var result = await context.ApiScopes.AddAsync(apiScope);
        }

        #endregion

        #region blog.api.comments

        apiScope = await context.ApiScopes
            .Where(scope => scope.Name == apiComments)
            .SingleOrDefaultAsync();

        if (null == apiScope)
        {
            apiScope = new ApiScope
            {
                Name = apiComments,
                DisplayName = "SampleBlog API comments",
                Description = "SampleBlog Blogs API comments",
                Enabled = true,
                Created = DateTime.UtcNow,
                ShowInDiscoveryDocument = true,
                Required = false
            };

            var result = await context.ApiScopes.AddAsync(apiScope);
        }

        #endregion

        #region openid

        apiScope = await context.ApiScopes
            .Where(scope => scope.Name == openId)
            .SingleOrDefaultAsync();

        if (null == apiScope)
        {
            apiScope = new ApiScope
            {
                Name = openId,
                DisplayName = "OpenID id scope",
                Description = "OpenID id scope",
                Enabled = true,
                Created = DateTime.UtcNow,
                ShowInDiscoveryDocument = true,
                Required = false
            };

            var result = await context.ApiScopes.AddAsync(apiScope);
        }

        #endregion

        #region profile

        apiScope = await context.ApiScopes
            .Where(scope => scope.Name == profile)
            .SingleOrDefaultAsync();

        if (null == apiScope)
        {
            apiScope = new ApiScope
            {
                Name = profile,
                DisplayName = "OpenID id profile",
                Description = "OpenID id profile",
                Enabled = true,
                Created = DateTime.UtcNow,
                ShowInDiscoveryDocument = true,
                Required = false
            };

            var result = await context.ApiScopes.AddAsync(apiScope);
        }

        #endregion

        // save changes
        await context.SaveChangesAsync();
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