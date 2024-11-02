using Microsoft.Extensions.DependencyInjection;

namespace DynamicAuthorization.Mvc.Core;

/// <summary>
///   An interface for configuring dynamic authorization options.
/// </summary>
public interface IDynamicAuthorizationOptionBuilder
{
    /// <summary>
    ///   Gets the <see cref="IServiceCollection"/> where essential services are configured.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    ///   Adds the default allowed admins and roles.
    /// </summary>
    /// <param name="allowedUserRoleOptions">The allowed user role options.</param>
    /// <exception cref="ArgumentNullException">Throws if allowedUserRoleOptions is null.</exception>
    /// <exception cref="InvalidOperationException">
    ///   Throws if none of <see cref="DynamicAuthorizationOptions.DefaultAllowedAdmins"/> and
    ///   <see cref="DynamicAuthorizationOptions.DefaultAllowedRoles"/> are initialized.
    /// </exception>
    /// <returns>IDynamicAuthorizationOptionBuilder.</returns>
    IDynamicAuthorizationOptionBuilder AddDefaultAllowedAdminsAndRoles(Action<DynamicAuthorizationOptions> allowedUserRoleOptions);
}
