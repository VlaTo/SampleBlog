using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SampleBlog.Shared;
using SampleBlog.Shared.Contracts.Permissions;
using SampleBlog.Web.Server.Configuration;

namespace SampleBlog.Web.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServerOptions(this IServiceCollection services)
    {
        services
            .AddOptions<ServerOptions>(nameof(ServerOptions))
            .Configure(configuration =>
            {
                ;
            })
            .Validate(configuration =>
            {
                return true;
            });

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, ServerOptions options)
    {
        var key = Encoding.ASCII.GetBytes(options.Secret);

        services
            .AddAuthentication(authentication =>
            {
                authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(bearer =>
            {
                bearer.RequireHttpsMetadata = false;
                bearer.SaveToken = true;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.Zero
                };

                bearer.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            return context.Response.WriteJsonAsync(
                                HttpStatusCode.Unauthorized,
                                Result.Fail("The Token is expired.")
                            );

                            //context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            //context.Response.ContentType = MediaTypeNames.Application.Json;

                            //var result = JsonConvert.SerializeObject(Result.Fail("The Token is expired."));

                            //return context.Response.WriteAsync(result);
                        }

                        return context.Response.WriteJsonAsync(
                            HttpStatusCode.InternalServerError,
                            Result.Fail("An unhandled error has occurred.")
                        );

                        //context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        //context.Response.ContentType = MediaTypeNames.Application.Json;

                        //var result = JsonConvert.SerializeObject(Result.Fail("An unhandled error has occurred."));

                        //return context.Response.WriteAsync(result);
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();

                        if (false == context.Response.HasStarted)
                        {
                            return context.Response.WriteJsonAsync(
                                HttpStatusCode.Unauthorized,
                                Result.Fail("You are not Authorized.")
                            );

                            //context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            //context.Response.ContentType = MediaTypeNames.Application.Json;

                            //var result = JsonConvert.SerializeObject(Result.Fail("You are not Authorized."));

                            //return context.Response.WriteAsync(result);
                        }

                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        return context.Response.WriteJsonAsync(
                            HttpStatusCode.Forbidden,
                            Result.Fail("You are not authorized to access this resource.")
                        );

                        //context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        ///context.Response.ContentType = MediaTypeNames.Application.Json;

                        //var result = JsonConvert.SerializeObject(Result.Fail("You are not authorized to access this resource."));

                        //return context.Response.WriteAsync(result);
                    }
                };

            });

        services
            .AddAuthorization(authorization =>
            {
                const string permission = "Permission";
                var fieldInfos = GetPermissionTypes(typeof(Permissions));

                foreach (var prop in fieldInfos)
                {
                    var propertyValue = prop.GetValue(null);

                    if (propertyValue is not null)
                    {
                        var name = propertyValue.ToString();
                        authorization.AddPolicy(name, policy => policy.RequireClaim(permission, name));
                    }
                }
            });

        return services;
    }

    private static IEnumerable<FieldInfo> GetPermissionTypes(Type permissionType)
    {
        return permissionType
            .GetNestedTypes()
            .SelectMany(
                fieldInfo => fieldInfo.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            );
    }
}