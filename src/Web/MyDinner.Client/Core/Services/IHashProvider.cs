namespace SampleBlog.Web.Client.Core.Services;

public interface IHashProvider
{
    string GetHash(DateTime value);
}