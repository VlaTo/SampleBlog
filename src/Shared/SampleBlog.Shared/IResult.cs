namespace SampleBlog.Shared;

public interface IResult
{
    bool Succeeded
    {
        get;
    }

    string? Error
    {
        get;
    }
}

public interface IResult<out T> : IResult
{
    T Data
    {
        get;
    }
}