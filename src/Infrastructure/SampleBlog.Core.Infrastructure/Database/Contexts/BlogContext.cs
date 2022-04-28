using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SampleBlog.Core.Application.Services;
using SampleBlog.Infrastructure.Models.Identity;

namespace SampleBlog.Infrastructure.Database.Contexts;

public sealed class BlogContext : AuditableContext
{
    private readonly ICurrentUserProvider currentUserProvider;

    public DbSet<Blog> Blogs
    {
        get;
        set;
    }

    public BlogContext(DbContextOptions<BlogContext> options, ICurrentUserProvider currentUserProvider)
        : base(options)
    {
        this.currentUserProvider = currentUserProvider;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        const string identitySchemaName = "Identity";

        // ;

        base.OnModelCreating(builder);
        
        builder.Entity<BlogUser>(entity =>
        {
            entity
                .ToTable(name: "Users", identitySchemaName)
                .Property(user => user.Id)
                .ValueGeneratedOnAdd();
        });
        builder.Entity<BlogUserRole>(entity =>
        {
            entity.ToTable(name: "Roles", identitySchemaName);
        });
        builder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.ToTable("UserRoles", identitySchemaName);
        });
        builder.Entity<IdentityUserClaim<string>>(entity =>
        {
            entity.ToTable("UserClaims", identitySchemaName);
        });
        builder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.ToTable("UserLogins", identitySchemaName);
        });
        builder.Entity<BlogUserRoleClaim>(entity =>
        {
            entity
                .ToTable(name: "RoleClaims", identitySchemaName)
                .HasOne(d => d.Role)
                .WithMany(p => p.RoleClaims)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.ToTable("UserTokens", identitySchemaName);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        var userId = currentUserProvider.CurrentUserId;

        if (String.IsNullOrEmpty(userId))
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        return SaveChangesAsync(userId, cancellationToken);
    }
}