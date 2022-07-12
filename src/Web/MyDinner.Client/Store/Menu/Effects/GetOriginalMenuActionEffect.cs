using Fluxor;
using SampleBlog.Web.Client.Core.Services;
using SampleBlog.Web.Client.Store.Menu.Actions;

namespace SampleBlog.Web.Client.Store.Menu.Effects;

// ReSharper disable once UnusedMember.Global
public sealed class GetOriginalMenuActionEffect : Effect<GetOriginalMenuAction>
{
    private readonly IMenuClient client;

    public GetOriginalMenuActionEffect(IMenuClient client)
    {
        this.client = client;
    }
    
    /// <inheritdoc cref="Effect{TTriggerAction}.HandleAsync" />
    public override async Task HandleAsync(GetOriginalMenuAction action, IDispatcher dispatcher)
    {
        try
        {
            var menu = await client.FetchOriginalMenuAsync(action.DateTime);

            if (null == menu)
            {
                dispatcher.Dispatch(new NoOriginalMenuAction(action.DateTime, null));
                return;
            }

            dispatcher.Dispatch(new OriginalMenuAcquiredAction(menu.Date, menu.Dishes, menu.IsOpen));
        }
        catch (Exception exception)
        {
            dispatcher.Dispatch(new NoOriginalMenuAction(action.DateTime, exception.Message));
        }
    }
}