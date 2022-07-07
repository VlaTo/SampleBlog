using MediatR;
using SampleBlog.Core.Domain.Entities;
using SampleBlog.Shared;

namespace SampleBlog.Web.APi.MyDinner.Features.Queries.GetMenu;

public class GetMenuQuery : IRequest<IResult<IMenu>>
{
    public DateTime Date
    {
        get;
    }

    public GetMenuQuery(DateTime date)
    {
        Date = date;
    }
}