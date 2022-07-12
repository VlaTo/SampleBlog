using System.Windows.Input;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SampleBlog.Web.Client.Core;

namespace SampleBlog.Web.Client.Shared;

public partial class DishPreviewDialog
{
    [CascadingParameter]
    public MudDialogInstance DialogInstance
    {
        get;
        set;
    }

    public ICommand Close
    {
        get;
    }

    public DishPreviewDialog()
    {
        Close = new DelegateCommand(DoClose);
    }

    private void DoClose()
    {
        DialogInstance.Close(DialogResult.Ok(true));
    }
}