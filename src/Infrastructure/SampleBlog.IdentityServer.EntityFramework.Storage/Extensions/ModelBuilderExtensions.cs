using Microsoft.EntityFrameworkCore;
using SampleBlog.IdentityServer.EntityFramework.Storage.Entities;
using SampleBlog.IdentityServer.EntityFramework.Storage.Options;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Extensions;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Configures the client context.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="options">The store options.</param>
    public static void ConfigureClientContext(this ModelBuilder modelBuilder, ConfigurationStoreOptions options)
    {
        if (String.IsNullOrWhiteSpace(options.DefaultSchema))
        {
            modelBuilder.HasDefaultSchema(options.DefaultSchema);
        }

        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable(options.Client);
            entity.HasKey(x => x.Id);

            entity
                .Property(x => x.ClientId)
                .HasMaxLength(200)
                .IsRequired();
            entity
                .Property(x => x.ProtocolType)
                .HasMaxLength(200)
                .IsRequired();
            entity
                .Property(x => x.ClientName)
                .HasMaxLength(200);
            entity
                .Property(x => x.ClientUri)
                .HasMaxLength(2000);
            entity
                .Property(x => x.LogoUri)
                .HasMaxLength(2000);
            entity
                .Property(x => x.Description)
                .HasMaxLength(1000);
            entity
                .Property(x => x.FrontChannelLogoutUri)
                .HasMaxLength(2000);
            entity
                .Property(x => x.BackChannelLogoutUri)
                .HasMaxLength(2000);
            entity
                .Property(x => x.ClientClaimsPrefix)
                .HasMaxLength(200);
            entity
                .Property(x => x.PairWiseSubjectSalt)
                .HasMaxLength(200);
            entity
                .Property(x => x.UserCodeType)
                .HasMaxLength(100);
            entity
                .Property(x => x.AllowedIdentityTokenSigningAlgorithms)
                .HasMaxLength(100);

            entity
                .HasIndex(x => x.ClientId)
                .IsUnique();

            entity
                .HasMany(x => x.AllowedGrantTypes)
                .WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasMany(x => x.RedirectUris)
                .WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasMany(x => x.PostLogoutRedirectUris)
                .WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasMany(x => x.AllowedScopes)
                .WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasMany(x => x.ClientSecrets)
                .WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasMany(x => x.Claims)
                .WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasMany(x => x.IdentityProviderRestrictions)
                .WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasMany(x => x.AllowedCorsOrigins)
                .WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasMany(x => x.Properties)
                .WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ClientGrantType>(entity =>
        {
            entity.ToTable(options.ClientGrantType);
            
            entity
                .Property(x => x.GrantType)
                .HasMaxLength(250)
                .IsRequired();

            entity
                .HasIndex(x => new { x.ClientId, x.GrantType })
                .IsUnique();
        });

        modelBuilder.Entity<ClientRedirectUri>(entity =>
        {
            entity.ToTable(options.ClientRedirectUri);

            entity
                .Property(x => x.RedirectUri)
                .HasMaxLength(400)
                .IsRequired();

            entity
                .HasIndex(x => new { x.ClientId, x.RedirectUri })
                .IsUnique();
        });

        /*modelBuilder.Entity<ClientPostLogoutRedirectUri>(postLogoutRedirectUri =>
        {
            postLogoutRedirectUri.ToTable(options.ClientPostLogoutRedirectUri);
            postLogoutRedirectUri.Property(x => x.PostLogoutRedirectUri).HasMaxLength(400).IsRequired();

            postLogoutRedirectUri.HasIndex(x => new { x.ClientId, x.PostLogoutRedirectUri }).IsUnique();
        });*/

        modelBuilder.Entity<ClientScope>(entity =>
        {
            entity.ToTable(options.ClientScopes);

            entity
                .Property(x => x.Scope)
                .HasMaxLength(200)
                .IsRequired();

            entity
                .HasIndex(x => new { x.ClientId, x.Scope })
                .IsUnique();
        });

        modelBuilder.Entity<ClientSecret>(entity =>
        {
            entity.ToTable(options.ClientSecret);

            entity
                .Property(x => x.Value)
                .HasMaxLength(4000)
                .IsRequired();
            entity
                .Property(x => x.Type)
                .HasMaxLength(250)
                .IsRequired();
            entity
                .Property(x => x.Description)
                .HasMaxLength(2000);
        });

        modelBuilder.Entity<ClientClaim>(entity =>
        {
            entity.ToTable(options.ClientClaim);

            entity
                .Property(x => x.Type)
                .HasMaxLength(250)
                .IsRequired();
            entity
                .Property(x => x.Value)
                .HasMaxLength(250)
                .IsRequired();

            entity
                .HasIndex(x => new { x.ClientId, x.Type, x.Value })
                .IsUnique();
        });

        modelBuilder.Entity<ClientIdPRestriction>(entity =>
        {
            entity.ToTable(options.ClientIdPRestriction);

            entity
                .Property(x => x.Provider)
                .HasMaxLength(200)
                .IsRequired();

            entity
                .HasIndex(x => new { x.ClientId, x.Provider })
                .IsUnique();
        });

        modelBuilder.Entity<ClientCorsOrigin>(entity =>
        {
            entity.ToTable(options.ClientCorsOrigin);

            entity
                .Property(x => x.Origin)
                .HasMaxLength(150)
                .IsRequired();

            entity
                .HasIndex(x => new { x.ClientId, x.Origin })
                .IsUnique();
        });

        modelBuilder.Entity<ClientProperty>(entity =>
        {
            entity.ToTable(options.ClientProperty);

            entity
                .Property(x => x.Key)
                .HasMaxLength(250)
                .IsRequired();
            entity
                .Property(x => x.Value)
                .HasMaxLength(2000)
                .IsRequired();

            entity
                .HasIndex(x => new { x.ClientId, x.Key })
                .IsUnique();
        });
    }
    
    /// <summary>
    /// Configures the persisted grant context.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="storeOptions">The store options.</param>
    public static void ConfigurePersistedGrantContext(this ModelBuilder modelBuilder, OperationalStoreOptions storeOptions)
    {
        if (false == String.IsNullOrWhiteSpace(storeOptions.DefaultSchema))
        {
            modelBuilder.HasDefaultSchema(storeOptions.DefaultSchema);
        }

        modelBuilder.Entity<PersistedGrant>(entity =>
        {
            entity.ToTable(storeOptions.PersistedGrants);

            entity.Property(x => x.Key).HasMaxLength(200);
            entity.Property(x => x.Type).HasMaxLength(50).IsRequired();
            entity.Property(x => x.SubjectId).HasMaxLength(200);
            entity.Property(x => x.SessionId).HasMaxLength(100);
            entity.Property(x => x.ClientId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(200);
            entity.Property(x => x.CreationTime).IsRequired();
            // 50000 chosen to be explicit to allow enough size to avoid truncation, yet stay beneath the MySql row size limit of ~65K
            // apparently anything over 4K converts to nvarchar(max) on SqlServer
            entity.Property(x => x.Data).HasMaxLength(50000).IsRequired();

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Key).IsUnique();
            entity.HasIndex(x => new { x.SubjectId, x.ClientId, x.Type });
            entity.HasIndex(x => new { x.SubjectId, x.SessionId, x.Type });
            entity.HasIndex(x => x.Expiration);
            entity.HasIndex(x => x.ConsumedTime);
        });

        /*modelBuilder.Entity<DeviceFlowCodes>(codes =>
        {
            codes.ToTable(storeOptions.DeviceFlowCodes);

            codes.Property(x => x.DeviceCode).HasMaxLength(200).IsRequired();
            codes.Property(x => x.UserCode).HasMaxLength(200).IsRequired();
            codes.Property(x => x.SubjectId).HasMaxLength(200);
            codes.Property(x => x.SessionId).HasMaxLength(100);
            codes.Property(x => x.ClientId).HasMaxLength(200).IsRequired();
            codes.Property(x => x.Description).HasMaxLength(200);
            codes.Property(x => x.CreationTime).IsRequired();
            codes.Property(x => x.Expiration).IsRequired();
            // 50000 chosen to be explicit to allow enough size to avoid truncation, yet stay beneath the MySql row size limit of ~65K
            // apparently anything over 4K converts to nvarchar(max) on SqlServer
            codes.Property(x => x.Data).HasMaxLength(50000).IsRequired();

            codes.HasKey(x => new { x.UserCode });

            codes.HasIndex(x => x.DeviceCode).IsUnique();
            codes.HasIndex(x => x.Expiration);
        });*/

        modelBuilder.Entity<Key>(entity =>
        {
            entity.ToTable(storeOptions.Keys);

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Use);
            entity.Property(x => x.Algorithm).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Data).IsRequired();
        });


        /*modelBuilder.Entity<ServerSideSession>(entity =>
        {
            entity.ToTable(storeOptions.ServerSideSessions);

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Key).IsUnique();
            entity.Property(x => x.Key).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Scheme).HasMaxLength(100).IsRequired();
            entity.Property(x => x.SubjectId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.SessionId).HasMaxLength(100);
            entity.Property(x => x.DisplayName).HasMaxLength(100);
            entity.Property(x => x.Data).IsRequired();

            entity.HasIndex(x => x.Expires);
            entity.HasIndex(x => x.SubjectId);
            entity.HasIndex(x => x.SessionId);
            entity.HasIndex(x => x.DisplayName);
        });*/
    }

    /// <summary>
    /// Configures the resources context.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="options">The store options.</param>
    public static void ConfigureResourcesContext(this ModelBuilder modelBuilder, ConfigurationStoreOptions options)
    {
        if (false == String.IsNullOrWhiteSpace(options.DefaultSchema))
        {
            modelBuilder.HasDefaultSchema(options.DefaultSchema);
        }

        modelBuilder.Entity<IdentityResource>(entity =>
        {
            entity
                .ToTable(options.IdentityResource)
                .HasKey(x => x.Id);

            entity
                .Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();
            entity
                .Property(x => x.DisplayName)
                .HasMaxLength(200);
            entity
                .Property(x => x.Description)
                .HasMaxLength(1000);

            entity
                .HasIndex(x => x.Name)
                .IsUnique();

            entity
                .HasMany(x => x.UserClaims)
                .WithOne(x => x.IdentityResource)
                .HasForeignKey(x => x.IdentityResourceId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasMany(x => x.Properties)
                .WithOne(x => x.IdentityResource)
                .HasForeignKey(x => x.IdentityResourceId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IdentityResourceClaim>(entity =>
        {
            entity
                .ToTable(options.IdentityResourceClaim)
                .HasKey(x => x.Id);

            entity
                .Property(x => x.Type)
                .HasMaxLength(200)
                .IsRequired();

            entity
                .HasIndex(x => new { x.IdentityResourceId, x.Type })
                .IsUnique();
        });

        modelBuilder.Entity<IdentityResourceProperty>(entity =>
        {
            entity.ToTable(options.IdentityResourceProperty);

            entity
                .Property(x => x.Key)
                .HasMaxLength(250)
                .IsRequired();
            entity
                .Property(x => x.Value)
                .HasMaxLength(2000)
                .IsRequired();

            entity
                .HasIndex(x => new { x.IdentityResourceId, x.Key })
                .IsUnique();
        });
        
        modelBuilder.Entity<ApiResource>(entity =>
        {
            entity
                .ToTable(options.ApiResource)
                .HasKey(x => x.Id);

            entity
                .Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();
            entity
                .Property(x => x.DisplayName)
                .HasMaxLength(200);
            entity
                .Property(x => x.Description)
                .HasMaxLength(1000);
            entity
                .Property(x => x.AllowedAccessTokenSigningAlgorithms)
                .HasMaxLength(100);

            entity
                .HasIndex(x => x.Name)
                .IsUnique();

            entity
                .HasMany(x => x.Secrets)
                .WithOne(x => x.ApiResource)
                .HasForeignKey(x => x.ApiResourceId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasMany(x => x.Scopes)
                .WithOne(x => x.ApiResource)
                .HasForeignKey(x => x.ApiResourceId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasMany(x => x.UserClaims)
                .WithOne(x => x.ApiResource)
                .HasForeignKey(x => x.ApiResourceId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasMany(x => x.Properties)
                .WithOne(x => x.ApiResource)
                .HasForeignKey(x => x.ApiResourceId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApiResourceSecret>(entity =>
        {
            entity
                .ToTable(options.ApiResourceSecret)
                .HasKey(x => x.Id);

            entity
                .Property(x => x.Description)
                .HasMaxLength(1000);
            entity
                .Property(x => x.Value)
                .HasMaxLength(4000)
                .IsRequired();
            entity
                .Property(x => x.Type)
                .HasMaxLength(250)
                .IsRequired();
        });

        modelBuilder.Entity<ApiResourceClaim>(entity =>
        {
            entity
                .ToTable(options.ApiResourceClaim)
                .HasKey(x => x.Id);

            entity
                .Property(x => x.Type)
                .HasMaxLength(200)
                .IsRequired();

            entity
                .HasIndex(x => new { x.ApiResourceId, x.Type })
                .IsUnique();
        });

        modelBuilder.Entity<ApiResourceScope>(entity =>
        {
            entity
                .ToTable(options.ApiResourceScope)
                .HasKey(x => x.Id);

            entity
                .Property(x => x.Scope)
                .HasMaxLength(200)
                .IsRequired();

            entity
                .HasIndex(x => new { x.ApiResourceId, x.Scope })
                .IsUnique();
        });

        modelBuilder.Entity<ApiResourceProperty>(entity =>
        {
            entity.ToTable(options.ApiResourceProperty);

            entity
                .Property(x => x.Key)
                .HasMaxLength(250)
                .IsRequired();
            entity
                .Property(x => x.Value)
                .HasMaxLength(2000)
                .IsRequired();

            entity
                .HasIndex(x => new { x.ApiResourceId, x.Key })
                .IsUnique();
        });
        
        modelBuilder.Entity<ApiScope>(entity =>
        {
            entity
                .ToTable(options.ApiScope)
                .HasKey(x => x.Id);

            entity
                .Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();
            entity
                .Property(x => x.DisplayName)
                .HasMaxLength(200);
            entity
                .Property(x => x.Description)
                .HasMaxLength(1000);

            entity
                .HasIndex(x => x.Name)
                .IsUnique();

            entity
                .HasMany(x => x.UserClaims)
                .WithOne(x => x.Scope)
                .HasForeignKey(x => x.ScopeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApiScopeClaim>(entity =>
        {
            entity
                .ToTable(options.ApiScopeClaim)
                .HasKey(x => x.Id);

            entity
                .Property(x => x.Type)
                .HasMaxLength(200)
                .IsRequired();

            entity
                .HasIndex(x => new { x.ScopeId, x.Type })
                .IsUnique();
        });

        modelBuilder.Entity<ApiScopeProperty>(entity =>
        {
            entity
                .ToTable(options.ApiScopeProperty)
                .HasKey(x => x.Id);

            entity
                .Property(x => x.Key)
                .HasMaxLength(250)
                .IsRequired();

            entity
                .Property(x => x.Value)
                .HasMaxLength(2000)
                .IsRequired();

            entity
                .HasIndex(x => new { x.ScopeId, x.Key })
                .IsUnique();
        });
    }

    /// <summary>
    /// Configures the identity providers.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="options">The store options.</param>
    public static void ConfigureIdentityProviderContext(this ModelBuilder modelBuilder, ConfigurationStoreOptions options)
    {
        if (false == String.IsNullOrWhiteSpace(options.DefaultSchema))
        {
            modelBuilder.HasDefaultSchema(options.DefaultSchema);
        }

        modelBuilder.Entity<IdentityProvider>(entity =>
        {
            entity
                .ToTable(options.IdentityProvider)
                .HasKey(x => x.Id);

            entity
                .Property(x => x.Scheme)
                .HasMaxLength(200)
                .IsRequired();
            entity
                .Property(x => x.Type)
                .HasMaxLength(20)
                .IsRequired();
            entity
                .Property(x => x.DisplayName)
                .HasMaxLength(200);

            entity
                .HasIndex(x => x.Scheme)
                .IsUnique();
        });
    }
}