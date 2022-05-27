using MediatR;
using SampleBlog.Core.Domain.Services;
using SampleBlog.Shared;

namespace SampleBlog.Web.APi.Blog.Features.Commands.AddBlog;

public sealed class AddBlogRequestHandler : IRequestHandler<AddBlogCommand, IResult<string>>
{
    private readonly IBlogService blogService;

    public AddBlogRequestHandler(IBlogService blogService)
    {
        this.blogService = blogService;
    }

    public Task<IResult<string>> Handle(AddBlogCommand request, CancellationToken cancellationToken)
    {
        return Result<string>.FailAsync();
    }
}