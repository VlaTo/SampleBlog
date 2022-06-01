using SampleBlog.Infrastructure.Database.Contexts;

namespace SampleBlog.Infrastructure.Repositories;

public sealed class BlogRepository : IBlogRepository
{
    private readonly BlogContext context;
    private bool disposed;

    public BlogRepository(BlogContext context)
    {
        this.context = context;
    }

    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool dispose)
    {
        if (disposed)
        {
            return;
        }

        try
        {
            if (dispose)
            {
                ;
            }
        }
        finally
        {
            disposed = true;
        }
    }
}