using Fluxor;
using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Store.Menu;

// ReSharper disable once UnusedMember.Global
public sealed class MenuStateFeature : Feature<MenuState>
{
    public override string GetName() => nameof(MenuState);

    protected override MenuState GetInitialState() => new(ModelState.None);
}

public sealed class MenuState : StateBase
{
    public DateTime? DateTime
    {
        get;
    }

    public DishEntry[]? Dishes
    {
        get;
        set;
    }

    public MenuState(ModelState state)
        : this(state, null, null)
    {
    }

    public MenuState(ModelState state, DateTime dateTime)
        : this(state, dateTime, null)
    {
    }

    public MenuState(ModelState state, DateTime? dateTime, DishEntry[]? dishes)
        : base(state)
    {
        DateTime = dateTime;
        Dishes = dishes;
    }
}