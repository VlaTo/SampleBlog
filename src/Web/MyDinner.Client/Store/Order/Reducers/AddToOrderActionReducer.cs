using Fluxor;
using SampleBlog.Web.Client.Store.Order.Actions;

namespace SampleBlog.Web.Client.Store.Order.Reducers;

// ReSharper disable once UnusedMember.Global
public sealed class AddToOrderActionReducer : Reducer<OrderState, AddToOrderAction>
{
    public override OrderState Reduce(OrderState state, AddToOrderAction action)
    {
        return new OrderState(ModelState.Success, state.Dishes.Add(action.Entry));
    }
}