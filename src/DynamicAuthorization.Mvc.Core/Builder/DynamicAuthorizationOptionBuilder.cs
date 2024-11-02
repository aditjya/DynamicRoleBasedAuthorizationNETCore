using DynamicAuthorization.Mvc.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DynamicAuthorization.Mvc.Core;

/// <inheritdoc cref="IDynamicAuthorizationOptionBuilder"/>
public class DynamicAuthorizationOptionBuilder : IDynamicAuthorizationOptionBuilder
{
    private readonly IServiceCollection _services;

    public DynamicAuthorizationOptionBuilder(IServiceCollection services)
    {
        _services = services;
    }

    IServiceCollection IDynamicAuthorizationOptionBuilder.Services => _services;

    /// <inheritdoc/>
    public IDynamicAuthorizationOptionBuilder AddDefaultAllowedAdminsAndRoles(Action<DynamicAuthorizationOptions> allowedUserRoleOptions)
    {
        ArgumentNullException.ThrowIfNull(allowedUserRoleOptions);

        DynamicAuthorizationOptions dynamicAuthorizationOptions = new();
        allowedUserRoleOptions.Invoke(dynamicAuthorizationOptions);

        if (dynamicAuthorizationOptions.DefaultAllowedAdmins.Count == 0 &&
            dynamicAuthorizationOptions.DefaultAllowedRoles.Count == 0)
        {
            throw new InvalidOperationException($"One of {nameof(DynamicAuthorizationOptions.DefaultAllowedAdmins)} or {nameof(DynamicAuthorizationOptions.DefaultAllowedRoles)}" +
                                                 " properties should be initialized.");
        }

        foreach (string user in dynamicAuthorizationOptions.DefaultAllowedAdmins)
        {
            DynamicAuthorizationOptionsInternals.DefaultAllowedAdmins.Add(user);
        }

        foreach (string role in dynamicAuthorizationOptions.DefaultAllowedRoles)
        {
            DynamicAuthorizationOptionsInternals.DefaultAllowedRoles.Add(role);
        }

        return this;
    }
}