using DynamicAuthorization.Mvc.Core.Filters;
using DynamicAuthorization.Mvc.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DynamicAuthorization.Mvc.Core;

/// <summary>
///   Extension methods for setting up Dynamic Authorization related services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///   Registers the Dynamic Authorization as a service in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="optionsBuilder">
    ///   An action to configure the <see cref="DynamicAuthorizationOptionBuilder"/> for the Dynamic Authorization.
    /// </param>
    /// <exception cref="ArgumentNullException">services</exception>
    /// <exception cref="ArgumentNullException">optionsBuilder</exception>
    /// <exception cref="ArgumentNullException">defaultAdminUser</exception>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IDynamicAuthorizationOptionBuilder AddDynamicAuthorization<TDbContext>(
        this IServiceCollection services,
        Action<DynamicAuthorizationOptionBuilder> optionsBuilder)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        Type baseType = typeof(TDbContext).BaseType!;
        int paramsLength = baseType.GetGenericArguments().Length;
        Type userType;
        Type roleType;
        Type keyType;

        switch (paramsLength)
        {
            case 1:
                userType = baseType.GetGenericArguments()[0];
                DynamicAuthorizationOptionsInternals.UserType = userType;
                DynamicAuthorizationOptionsInternals.RoleType = typeof(IdentityRole);
                DynamicAuthorizationOptionsInternals.KeyType = typeof(string);
                services.Configure<MvcOptions>(mvcOptions =>
                {
                    mvcOptions.Filters.Add(typeof(DynamicAuthorizationFilter<,>).MakeGenericType(typeof(TDbContext), userType));
                });
                break;

            case 3:
                userType = baseType.GetGenericArguments()[0];
                roleType = baseType.GetGenericArguments()[1];
                keyType = baseType.GetGenericArguments()[2];
                DynamicAuthorizationOptionsInternals.UserType = userType;
                DynamicAuthorizationOptionsInternals.RoleType = roleType;
                DynamicAuthorizationOptionsInternals.KeyType = keyType;
                services.Configure<MvcOptions>(mvcOptions =>
                {
                    mvcOptions.Filters.Add(typeof(DynamicAuthorizationFilter<,,,>)
                        .MakeGenericType(typeof(TDbContext), userType, roleType, keyType));
                });
                break;

            case 8:
                userType = baseType.GetGenericArguments()[0];
                roleType = baseType.GetGenericArguments()[1];
                keyType = baseType.GetGenericArguments()[2];

                Type userClaimType = baseType.GetGenericArguments()[3];
                Type userRoleType = baseType.GetGenericArguments()[4];
                Type userLoginType = baseType.GetGenericArguments()[5];
                Type roleClaimType = baseType.GetGenericArguments()[6];
                Type userTokenType = baseType.GetGenericArguments()[7];

                DynamicAuthorizationOptionsInternals.UserType = userType;
                DynamicAuthorizationOptionsInternals.RoleType = roleType;
                DynamicAuthorizationOptionsInternals.KeyType = keyType;
                DynamicAuthorizationOptionsInternals.UserClaimType = userClaimType;
                DynamicAuthorizationOptionsInternals.UserRoleType = userRoleType;
                DynamicAuthorizationOptionsInternals.UserLoginType = userLoginType;
                DynamicAuthorizationOptionsInternals.RoleClaimType = roleClaimType;
                DynamicAuthorizationOptionsInternals.UserTokenType = userTokenType;
                services.Configure<MvcOptions>(mvcOptions =>
                {
                    mvcOptions.Filters.Add(typeof(DynamicAuthorizationFilter<,,,,,,,,>)
                        .MakeGenericType(typeof(TDbContext), userType, roleType, keyType, userClaimType, userRoleType, userLoginType, roleClaimType, userTokenType));
                });
                break;

            default:
                DynamicAuthorizationOptionsInternals.UserType = typeof(IdentityUser);
                DynamicAuthorizationOptionsInternals.RoleType = typeof(IdentityRole);
                DynamicAuthorizationOptionsInternals.KeyType = typeof(string);
                services.Configure<MvcOptions>(mvcOptions =>
                {
                    mvcOptions.Filters.Add(typeof(DynamicAuthorizationFilter<>).MakeGenericType(typeof(TDbContext)));
                });
                break;
        }

        DynamicAuthorizationOptionsInternals.DbContextType = typeof(TDbContext);

        services.AddSingleton<IMvcControllerDiscovery, MvcControllerDiscovery>();

        IDynamicAuthorizationOptionBuilder builder = new DynamicAuthorizationOptionBuilder(services);

        return builder;
    }
}