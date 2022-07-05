using System.ComponentModel.DataAnnotations;

namespace SampleBlog.Web.Identity.ViewModels;

public sealed class ResetPasswordModel
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
}