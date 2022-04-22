using Microsoft.EntityFrameworkCore;
using SampleBlog.Core.Application.Services;

namespace SampleBlog.Infrastructure.Database.Contexts;

public sealed class BlogContext : AuditableContext
{
    private readonly ICurrentUserProvider currentUserProvider;

    public BlogContext(DbContextOptions<BlogContext> options, ICurrentUserProvider currentUserProvider)
        : base(options)
    {
        this.currentUserProvider = currentUserProvider;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
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