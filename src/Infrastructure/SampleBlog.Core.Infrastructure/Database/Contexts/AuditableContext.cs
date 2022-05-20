using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SampleBlog.IdentityServer.EntityFramework.Storage;
using SampleBlog.IdentityServer.EntityFramework.Storage.Entities;
using SampleBlog.Infrastructure.Models;
using SampleBlog.Infrastructure.Models.Identity;

namespace SampleBlog.Infrastructure.Database.Contexts;

public abstract class AuditableContext : 
    IdentityDbContext<BlogUser, BlogUserRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, BlogUserRoleClaim, IdentityUserToken<string>>,
    IPersistedGrantDbContext
{
    #region IPersistedGrantDbContext implementation

    /// <inheritdoc />
    public DbSet<PersistedGrant> PersistedGrants
    {
        get;
        set;
    }

    /// <inheritdoc />
    public DbSet<Key> Keys
    {
        get;
        set;
    }

    #endregion

    #region IConfigurationDbContext implementation

    /// <inheritdoc />
    public DbSet<Client> Clients
    {
        get;
        set;
    }

    /// <inheritdoc />
    public DbSet<ClientCorsOrigin> ClientCorsOrigins
    {
        get;
        set;
    }

    /// <inheritdoc />
    public DbSet<IdentityResource> IdentityResources
    {
        get;
        set;
    }

    /// <inheritdoc />
    public DbSet<ApiResource> ApiResources
    {
        get;
        set;
    }

    /// <inheritdoc />
    public DbSet<ApiScope> ApiScopes
    {
        get;
        set;
    }

    /// <inheritdoc />
    public DbSet<IdentityProvider> IdentityProviders
    {
        get;
        set;
    }

    #endregion

    protected AuditableContext(DbContextOptions options)
        : base(options)
    {
    }

    public async Task<int> SaveChangesAsync(string userId, CancellationToken cancellationToken = new())
    {
        var auditEntries = OnBeforeSaveChanges(userId);
        var result = await base.SaveChangesAsync(cancellationToken);

        await OnAfterSaveChanges(auditEntries, cancellationToken);

        return result;
    }

    private IReadOnlyList<AuditEntry> OnBeforeSaveChanges(string userId)
    {
        ChangeTracker.DetectChanges();

        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            ;
        }

        return auditEntries;
    }

    private Task OnAfterSaveChanges(IReadOnlyList<AuditEntry> auditEntries, CancellationToken cancellationToken = new())
    {
        if (0 == auditEntries.Count)
        {
            return Task.CompletedTask;
        }

        foreach (var auditEntry in auditEntries)
        {
            /*foreach (var prop in auditEntry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                }
                else
                {
                    auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                }
            }
            AuditTrails.Add(auditEntry.ToAudit());*/
        }

        return SaveChangesAsync(cancellationToken);
    }
}