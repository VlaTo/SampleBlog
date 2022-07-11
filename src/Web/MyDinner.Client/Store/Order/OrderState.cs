using System.Collections.Immutable;
using Fluxor;
using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Store.Order;

// ReSharper disable once UnusedMember.Global
public sealed class OrderStateFeature : Feature<OrderState>
{
    public override string GetName() => nameof(OrderState);

    protected override OrderState GetInitialState() => new(ModelState.Success, ImmutableDictionary<DishEntry, int>.Empty);
}

public sealed class OrderState : StateBase
{
    public ImmutableDictionary<DishEntry, int> Entries
    {
        get;
    }

    public float TotalCalories
    {
        get
        {
            return Entries.Sum(kvp => kvp.Key.Calories * kvp.Value);
        }
    }

    public decimal TotalPrice
    {
        get
        {
            return Entries.Sum(kvp => kvp.Key.Price * kvp.Value);
        }
    }

    public OrderState(ModelState state, ImmutableDictionary<DishEntry, int> entries)
        : base(state)
    {
        Entries = entries;
    }
}