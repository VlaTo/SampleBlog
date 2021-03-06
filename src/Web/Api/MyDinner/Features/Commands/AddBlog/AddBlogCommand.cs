using MediatR;
using SampleBlog.Shared;

namespace SampleBlog.Web.APi.Blog.Features.Commands.AddBlog;

public sealed class AddBlogCommand : IRequest<IResult<string>>
{
    public string Content
    {
        get;
    }

    public string? UserId
    {
        get;
    }

    public AddBlogCommand(string? userId, string content)
    {
        UserId = userId;
        Content = content;
    }
}