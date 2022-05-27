using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SampleBlog.Web.Identity.ViewModels;

public sealed class SignInModel
{
    [Required]
    [DataType(DataType.EmailAddress)]
    [EmailAddress]
    [Display(Name = "Email address", Description = "Email address description")]
    public string Email
    {
        get;
        set;
    }

    [Required]
    [PasswordPropertyText]
    [DataType(DataType.Password)]
    [Display(Name = "Enter password", Description = "Password description")]
    public string Password
    {
        get;
        set;
    }

    [Display(Name = "Remember me")]
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