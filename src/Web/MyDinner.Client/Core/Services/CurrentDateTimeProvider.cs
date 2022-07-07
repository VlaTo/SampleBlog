namespace SampleBlog.Web.Client.Core.Services;

internal sealed class CurrentDateTimeProvider : ICurrentDateTimeProvider
{
    public DateTime CurrentDateTime => DateTime.UtcNow;
}