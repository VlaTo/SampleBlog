using System.Collections.Immutable;
using Fluxor;
using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Store.Order;

// ReSharper disable once UnusedMember.Global
public sealed class OrderStateFeature : Feature<OrderState>
{
    public override string GetName() => nameof(OrderState);

    protected override OrderState GetInitialState() => new(ModelState.Success, ImmutableArray<DishEntry>.Empty);
}

public sealed class OrderState : StateBase
{
    public ImmutableArray<DishEntry> Dishes
    {
        get;
    }

    public decimal TotalPrice => Dishes.Sum(entry => entry.Price);

    public OrderState(ModelState state, ImmutableArray<DishEntry> dishes)
        : base(state)
    {
        Dishes = dishes;
    }
}