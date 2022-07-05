namespace SampleBlog.Web.Application.MyDinner.Client.Store;

public class StateBase : IHasModelState
{
    public ModelState State
    {
        get;
    }

    protected StateBase(ModelState state)
    {
        State = state;
    }
}