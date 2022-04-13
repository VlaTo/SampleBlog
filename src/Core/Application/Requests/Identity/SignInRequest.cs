using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SampleBlog.Core.Application.Requests.Identity;

public class SignInRequest
{
    [Required]
    [DataType(DataType.EmailAddress)]
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
}