using Fluxor;
using SampleBlog.Web.Client.Store.Order.Actions;

namespace SampleBlog.Web.Client.Store.Order.Reducers;

// ReSharper disable once UnusedMember.Global
public sealed class RemoveFromOrderActionReducer : Reducer<OrderState, RemoveDishFromOrderAction>
{
    public override OrderState Reduce(OrderState state, RemoveDishFromOrderAction action)
    {
        return new OrderState(state.State, state.Entries.Remove(action.Entry));
    }
}