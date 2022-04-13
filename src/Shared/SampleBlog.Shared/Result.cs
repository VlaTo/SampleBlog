namespace SampleBlog.Shared;

public class Result : IResult
{
    public bool Succeeded
    {
        get;
        private protected set;
    }

    public string? Error
    {
        get;
        private protected set;
    }

    public static IResult Success() => new Result
    {
        Succeeded = true,
        Error = null
    };

    public static Task<IResult> SuccessAsync() => Task.FromResult<IResult>(new Result
    {
        Succeeded = true,
        Error = null
    });

    public static IResult Fail() => new Result
    {
        Succeeded = false,
        Error = null
    };

    public static IResult Fail(string error) => new Result
    {
        Succeeded = false,
        Error = error
    };

    public static Task<IResult> FailAsync() => Task.FromResult<IResult>(new Result
    {
        Succeeded = false,
        Error = null
    });
}

public sealed class Result<T> : Result, IResult<T>
{
    public T Data
    {
        get;
        private init;
    }

    public Result()
    {
        Data = default!;
    }

    public static Result<T> Success(T value) => new()
    {
        Succeeded = true,
        Error = null,
        Data = value
    };

    public static Task<IResult<T>> SuccessAsync(T value) => Task.FromResult<IResult<T>>(new Result<T>
    {
        Succeeded = true,
        Error = null,
        Data = value
    });

    public new static IResult<T> Fail() => new Result<T>
    {
        Succeeded = false,
        Error = null,
        Data = default!
    };

    public new static IResult<T> Fail(string error) => new Result<T>
    {
        Succeeded = false,
        Error = error,
        Data = default!
    };

    public new static Task<IResult<T>> FailAsync() => Task.FromResult<IResult<T>>(new Result<T>
    {
        Succeeded = false,
        Error = null,
        Data = default!
    });

    public static Task<IResult<T>> FailAsync(string error) => Task.FromResult<IResult<T>>(new Result<T>
    {
        Succeeded = false,
        Error = error,
        Data = default!
    });
}