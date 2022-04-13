using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SampleBlog.Core.Application.Models.Identity;
using SampleBlog.IdentityServer.EntityFramework.Storage;
using SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

namespace SampleBlog.Infrastructure.Database.Contexts;

public abstract class AuditableContext : IdentityDbContext<BlogUser, BlogUserRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, BlogUserRoleClaim, IdentityUserToken<string>>, IPersistedGrantDbContext
{
    public DbSet<PersistedGrant> PersistedGrants
    {
        get;
        set;
    }

    public DbSet<Key> Keys
    {
        get;
        set;
    }

    protected AuditableContext(DbContextOptions options)
        : base(options)
    {
    }
}