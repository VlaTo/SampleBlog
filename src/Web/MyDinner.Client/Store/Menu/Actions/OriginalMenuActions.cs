using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Store.Menu.Actions;

// ACTION: GetOriginalMenuAction
public record GetOriginalMenuAction(DateTime DateTime);

// ACTION: OriginalMenuAcquiredAction
public record OriginalMenuAcquiredAction(DateTime DateTime, IReadOnlyList<Dish> OriginalMenu, bool IsOpen);

// ACTION: NoOriginalMenuAction
public record NoOriginalMenuAction(DateTime DateTime, string? error);

// ACTION: FilterMenuAction
public record FilterOriginalMenuAction(string FoodCategoryKey);

// ACTION: ResetOriginalMenuFilterAction
public record ResetOriginalMenuFilterAction();