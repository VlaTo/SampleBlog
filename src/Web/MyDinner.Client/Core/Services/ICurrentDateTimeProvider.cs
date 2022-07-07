namespace SampleBlog.Web.Client.Core.Services;

public interface ICurrentDateTimeProvider
{
    DateTime CurrentDateTime
    {
        get;
    }
}