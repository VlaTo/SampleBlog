using MediatR;
using SampleBlog.Core.Domain.Entities;
using SampleBlog.Core.Domain.Services;
using SampleBlog.Shared;

namespace SampleBlog.Web.APi.MyDinner.Features.Queries.GetMenu;

// ReSharper disable once UnusedMember.Global
public sealed class GetMenuQueryHandler : IRequestHandler<GetMenuQuery, IResult<IMenu>>
{
    private readonly IMenuService service;

    public GetMenuQueryHandler(IMenuService service)
    {
        this.service = service;
    }

    public async Task<IResult<IMenu>> Handle(GetMenuQuery request, CancellationToken cancellationToken)
    {
        var menu = await service.GetMenuAsync(request.Date, cancellationToken);

        if (null != menu)
        {
            return Result<IMenu>.Success(menu);
        }

        return Result<IMenu>.Fail();
    }
}