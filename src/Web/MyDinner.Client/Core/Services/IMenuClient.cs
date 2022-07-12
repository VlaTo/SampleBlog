using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Core.Services;

public interface IMenuClient
{
    Task<Menu?> FetchOriginalMenuAsync(DateTime dateTime);
}