using MediatR;
using SampleBlog.Core.Domain.Entities;
using SampleBlog.Shared;

namespace SampleBlog.Core.Application.Features.Queries.GetBlog;

public sealed class GetBlogQuery : IRequest<IResult<IBlog>>
{
    public long Id
    {
        get;
    }

    public string? CurrentUserId
    {
        get;
    }

    public GetBlogQuery(long id, string? currentUserId)
    {
        Id = id;
        CurrentUserId = currentUserId;
    }
}