using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SampleBlog.IdentityServer.EntityFramework.Storage.Options;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Extensions;

public static class EntityTypeBuilderExtensions
{
    public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        TableConfiguration configuration)
        where TEntity : class =>
        String.IsNullOrWhiteSpace(configuration.Schema)
            ? entityTypeBuilder.ToTable(configuration.Name)
            : entityTypeBuilder.ToTable(configuration.Name, configuration.Schema);
}