using Fluxor;
using SampleBlog.Web.Client.Store.Order.Actions;
using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Store.Order.Reducers;

// ReSharper disable once UnusedMember.Global
public sealed class AddToOrderActionReducer : Reducer<OrderState, AddToOrderAction>
{
    public AddToOrderActionReducer()
    {
    }

    public override OrderState Reduce(OrderState state, AddToOrderAction action)
    {
        var dishes = new List<DishEntry>(state.Dishes)
        {
            action.Entry
        };

        return new OrderState(ModelState.Success, dishes);
    }
}