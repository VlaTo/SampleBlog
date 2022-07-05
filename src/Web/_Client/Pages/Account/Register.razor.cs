using Microsoft.AspNetCore.Components.Forms;
using SampleBlog.Web.Client.Models;

namespace SampleBlog.Web.Client.Pages.Account;

public partial class Register
{
    public RegisterUserModel UserModel
    {
        get;
        set;
    }

    public Register()
    {
        UserModel = new RegisterUserModel();
    }

    private void DoRegisterUser(EditContext editContext)
    {

    }
}