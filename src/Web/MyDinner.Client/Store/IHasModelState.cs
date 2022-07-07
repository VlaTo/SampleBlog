namespace SampleBlog.Web.Client.Store;

public interface IHasModelState
{
    ModelState State
    {
        get;
    }
}