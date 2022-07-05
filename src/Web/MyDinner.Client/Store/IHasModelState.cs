namespace SampleBlog.Web.Application.MyDinner.Client.Store;

public interface IHasModelState
{
    ModelState State
    {
        get;
    }
}