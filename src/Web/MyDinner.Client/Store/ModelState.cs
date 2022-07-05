namespace SampleBlog.Web.Application.MyDinner.Client.Store;

public enum ModelStatus
{
    Error = -1,
    None,
    Loading,
    Success
}

public class ModelState : IEquatable<ModelState>
{
    public static readonly ModelState None;

    public static readonly ModelState Loading;
    
    public static readonly ModelState Success;

    public ModelStatus Status
    {
        get;
    }

    public string? Error
    {
        get;
    }

    private ModelState(ModelStatus status)
    {
        Status = status;
        Error = null;
    }

    private ModelState(ModelStatus status, string error)
    {
        Status = status;
        Error = error;
    }

    static ModelState()
    {
        None = new ModelState(ModelStatus.None);
        Loading = new ModelState(ModelStatus.Loading);
        Success = new ModelState(ModelStatus.Success);
    }

    public static ModelState Failed(string error)
    {
        return new ModelState(ModelStatus.Error, error);
    }

    public bool Equals(ModelState? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Status == other.Status;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ModelState)obj);
    }

    public override int GetHashCode() => (int)Status;
}