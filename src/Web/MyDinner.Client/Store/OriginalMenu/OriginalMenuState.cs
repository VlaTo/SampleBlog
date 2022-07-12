using Fluxor;
using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Store.OriginalMenu;

// ReSharper disable once UnusedMember.Global
public sealed class OriginalMenuStateFeature : Feature<OriginalMenuState>
{
    public override string GetName() => nameof(OriginalMenuState);

    protected override OriginalMenuState GetInitialState() => new(ModelState.None);
}

/// <summary>
/// The original menu store class.
/// </summary>
public sealed class OriginalMenuState : StateBase
{
    public DateTime? DateTime
    {
        get;
    }

    public Dish[]? Dishes
    {
        get;
        set;
    }

    public bool IsAvailable
    {
        get;
        set;
    }

    public OriginalMenuState(ModelState state)
        : this(state, null, null)
    {
    }

    public OriginalMenuState(ModelState state, DateTime dateTime)
        : this(state, dateTime, null)
    {
    }

    public OriginalMenuState(ModelState state, DateTime? dateTime, Dish[]? dishes)
        : base(state)
    {
        DateTime = dateTime;
        Dishes = dishes;
    }
}