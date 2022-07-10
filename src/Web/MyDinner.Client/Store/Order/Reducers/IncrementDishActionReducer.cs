using Fluxor;
using SampleBlog.Web.Client.Store.Order.Actions;

namespace SampleBlog.Web.Client.Store.Order.Reducers;

// ReSharper disable once UnusedMember.Global
public sealed class IncrementDishActionReducer : Reducer<OrderState, IncrementDishAction>
{
    public override OrderState Reduce(OrderState state, IncrementDishAction action)
    {
        var entries = state.Entries.TryGetValue(action.Entry, out var count)
            ? state.Entries.SetItem(action.Entry, count + action.Count)
            : state.Entries.Add(action.Entry, action.Count);
        return new OrderState(ModelState.Success, entries);
    }
}