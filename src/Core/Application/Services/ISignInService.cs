using SampleBlog.Core.Domain.Entities;

namespace SampleBlog.Core.Application.Services;

public record SignInResult
{
    private readonly SignStatus status;

    public bool IsNotFound
    {
        init => status = value ? SignStatus.NotFound : SignStatus.Unknown;
        get => SignStatus.NotFound == status;
    }

    public bool IsNotAllowed
    {
        init => status = value ? SignStatus.NotAllowed : SignStatus.Unknown;
        get => SignStatus.NotAllowed == status;
    }

    public bool IsLockedOut
    {
        init => status = value ? SignStatus.LockedOut : SignStatus.Unknown;
        get => SignStatus.LockedOut == status;
    }

    public bool IsSuccess
    {
        init => status = value ? SignStatus.Success : SignStatus.Unknown;
        get => SignStatus.Success == status;
    }

    public IBlogUser? User
    {
        init;
        get;
    }

    public string? Token
    {
        init;
        get;
    }

    private enum SignStatus
    {
        Unknown = -4,
        NotFound,
        NotAllowed,
        LockedOut,
        Success
    }
}

public interface ISignInService
{
    Task<SignInResult> SignInAsync(string email, string password, bool rememberMe);
}