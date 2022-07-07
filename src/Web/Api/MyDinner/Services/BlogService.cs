using SampleBlog.Core.Domain.Entities;
using SampleBlog.Core.Domain.Services;
using SampleBlog.Infrastructure.Database.Contexts;
using SampleBlog.Infrastructure.Repositories;
using SampleBlog.Web.APi.Blog.Models;

namespace SampleBlog.Web.APi.MyDinner.Services;

internal class BlogService : IBlogService
{
    private readonly BlogContext context;

    public BlogService(BlogContext context)
    {
        this.context = context;
    }

    public Task<IBlog?> GetBlogAsync(long blogId, CancellationToken cancellationToken = default)
    {
        BlogModel blog;

        using (var repository = new BlogRepository(context))
        {
            var author = new BlogAuthor("2u54yui45")
            {
                IsActive = true,
                UserName = "435kj6h34 j456hk34"
            };
            
            blog = new BlogModel(blogId, "Sample Blog", "sample-blog", author);
        }

        return Task.FromResult<IBlog?>(blog);
    }
}