using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SampleBlog.Web.Server.ViewModels;

public sealed class SignInModel
{
    [Required]
    [DataType(DataType.EmailAddress)]
    [EmailAddress]
    [DisplayName("Email address")]
    [Description("Email address description")]
    public string Email
    {
        get;
        set;
    }

    [Required]
    [PasswordPropertyText]
    [DataType(DataType.Password)]
    public string Password
    {
        get;
        set;
    }

    public bool RememberMe
    {
        get;
        set;
    }

    //[DataType(DataType.Url)]
    public string? RedirectUrl
    {
        get;
        set;
    }
}