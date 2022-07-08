using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using SampleBlog.Web.Client.Store.Order;
using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Shared
{
    public partial class Calculator
    {
        [Inject]
        public IState<OrderState> Order
        {
            get;
            set;
        }

        [Parameter]
        public RenderFragment<DishEntry> RowTemplate
        {
            get;
            set;
        }

        [Parameter]
        public RenderFragment HeadContent
        {
            get;
            set;
        }

        public Calculator()
        {
            HeadContent = DefaultHeadContent;
        }
        
        private static void DefaultHeadContent(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            //builder.AddContent(1, "Registration is not supported.");
            builder.CloseElement();
        }
    }
}
