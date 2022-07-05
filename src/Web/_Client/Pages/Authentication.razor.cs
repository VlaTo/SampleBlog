using Microsoft.AspNetCore.Components;

namespace SampleBlog.Web.Client.Pages;

public partial class Authentication
{
    [Parameter]
    public string? Action
    {
        get;
        set;
    }
}