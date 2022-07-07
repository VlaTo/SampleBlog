using SampleBlog.Core.Domain.Entities;

namespace SampleBlog.Core.Domain.Services;

public interface IMenuService
{
    Task<IMenu?> GetMenuAsync(DateTime dateTime, CancellationToken cancellationToken = default);
}