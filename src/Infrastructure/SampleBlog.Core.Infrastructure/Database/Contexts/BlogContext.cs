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
}