using MediatR;
using SampleBlog.Core.Domain.Entities;
using SampleBlog.Core.Domain.Services;
using SampleBlog.Shared;

namespace SampleBlog.Core.Application.Features.Queries.GetBlog;

public sealed class GetBlogRequestHandler : IRequestHandler<GetBlogQuery, IResult<Blog>>
{
    private readonly IBlogService blogService;

    public GetBlogRequestHandler(IBlogService blogService)
    {
        this.blogService = blogService;
    }

    public async Task<IResult<Blog>> Handle(GetBlogQuery request, CancellationToken cancellationToken)
    {
        var blog = await blogService.GetBlogAsync(request.Id, cancellationToken);

        if (null != blog)
        {
            return Result<Blog>.Success(blog);
        }

        return Result<Blog>.Fail();
    }
}