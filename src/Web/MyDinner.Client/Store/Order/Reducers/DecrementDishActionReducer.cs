using Fluxor;
using SampleBlog.Web.Client.Store.Order.Actions;

namespace SampleBlog.Web.Client.Store.Order.Reducers;

// ReSharper disable once UnusedMember.Global
public sealed class DecrementDishActionReducer : Reducer<OrderState, DecrementDishAction>
{
    public override OrderState Reduce(OrderState state, DecrementDishAction action)
    {
        if (false == state.Entries.TryGetValue(action.Entry, out var count) || 1 == count)
        {
            return state;
        }

        var entries = action.Count >= count
            ? state.Entries.Remove(action.Entry)
            : state.Entries.SetItem(action.Entry, count - action.Count);

        return new OrderState(state.State, entries);
    }
}