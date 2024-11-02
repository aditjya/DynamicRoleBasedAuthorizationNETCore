using System.Reflection;
using DynamicAuthorization.Mvc.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DynamicAuthorization.Mvc.Core.Filters;

/// <summary>
///   Represents a dynamic authorization filter for a specific DbContext type.
/// </summary>
/// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
internal class DynamicAuthorizationFilter<TDbContext> : DynamicAuthorizationFilter<TDbContext, IdentityUser, IdentityRole, string>
        where TDbContext : IdentityDbContext
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="DynamicAuthorizationFilter{TDbContext}"/> class.
    /// </summary>
    /// <param name="dbContext">The instance of the DbContext.</param>
    /// <param name="roleAccessStore">The instance of the role access store.</param>
    public DynamicAuthorizationFilter(TDbContext dbContext, IRoleAccessStore roleAccessStore)
        : base(dbContext, roleAccessStore)
    {
    }
}

/// <summary>
///   Represents a dynamic authorization filter for a specific DbContext and User type.
/// </summary>
/// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
/// <typeparam name="TUser">The type of the User.</typeparam>
internal class DynamicAuthorizationFilter<TDbContext, TUser> : DynamicAuthorizationFilter<TDbContext, TUser, IdentityRole, string>
    where TDbContext : IdentityDbContext<TUser>
    where TUser : IdentityUser
{
    /// <summary>
    ///   Initializes a new instance of the
    ///   <see cref="DynamicAuthorizationFilter{TDbContext, TUser}"/> class.
    /// </summary>
    /// <param name="dbContext">The instance of the DbContext.</param>
    /// <param name="roleAccessStore">The instance of the role access store.</param>
    public DynamicAuthorizationFilter(TDbContext dbContext, IRoleAccessStore roleAccessStore)
        : base(dbContext, roleAccessStore)
    {
    }
}

/// <summary>
///   Represents a dynamic authorization filter for a specific DbContext, User, Role, and Key type.
/// </summary>
/// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
/// <typeparam name="TUser">The type of the User.</typeparam>
/// <typeparam name="TRole">The type of the Role.</typeparam>
/// <typeparam name="TKey">The type of the Key.</typeparam>
internal class DynamicAuthorizationFilter<TDbContext, TUser, TRole, TKey>
    : DynamicAuthorizationFilter<TDbContext, TUser, TRole, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>,
        IdentityUserLogin<TKey>, IdentityRoleClaim<TKey>, IdentityUserToken<TKey>>
    where TDbContext : IdentityDbContext<TUser, TRole, TKey>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    ///   Initializes a new instance of the
    ///   <see cref="DynamicAuthorizationFilter{TDbContext, TUser, TRole, TKey}"/> class.
    /// </summary>
    /// <param name="dbContext">The instance of the DbContext.</param>
    /// <param name="roleAccessStore">The instance of the role access store.</param>
    public DynamicAuthorizationFilter(TDbContext dbContext, IRoleAccessStore roleAccessStore)
        : base(dbContext, roleAccessStore)
    {
    }
}

/// <summary>
///   Represents a dynamic authorization filter for a specific DbContext, User, Role, Key,
///   UserClaim, UserRole, UserLogin, RoleClaim, and UserToken type.
/// </summary>
/// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
/// <typeparam name="TUser">The type of the User.</typeparam>
/// <typeparam name="TRole">The type of the Role.</typeparam>
/// <typeparam name="TKey">The type of the Key.</typeparam>
/// <typeparam name="TUserClaim">The type of the UserClaim.</typeparam>
/// <typeparam name="TUserRole">The type of the UserRole.</typeparam>
/// <typeparam name="TUserLogin">The type of the UserLogin.</typeparam>
/// <typeparam name="TRoleClaim">The type of the RoleClaim.</typeparam>
/// <typeparam name="TUserToken">The type of the UserToken.</typeparam>
internal class DynamicAuthorizationFilter<TDbContext, TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
    : IAsyncAuthorizationFilter
    where TDbContext : IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>
    where TUserRole : IdentityUserRole<TKey>
    where TUserLogin : IdentityUserLogin<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>
    where TUserToken : IdentityUserToken<TKey>
{
    private static readonly Dictionary<string, bool> s_actionDict = new();
    private readonly TDbContext _dbContext;
    private readonly IRoleAccessStore _roleAccessStore;

    /// <summary>
    ///   Initializes a new instance of the
    ///   <see cref="DynamicAuthorizationFilter{TDbContext, TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken}"/> class.
    /// </summary>
    /// <param name="dbContext">The instance of the DbContext.</param>
    /// <param name="roleAccessStore">The instance of the role access store.</param>
    public DynamicAuthorizationFilter(
        TDbContext dbContext,
        IRoleAccessStore roleAccessStore
    )
    {
        _roleAccessStore = roleAccessStore;
        _dbContext = dbContext;
    }

    /// <summary>
    ///   Performs authorization asynchronously.
    /// </summary>
    /// <param name="context">The authorization filter context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Check if the action is protected
        (bool isProtectedAction, string actionId) = IsProtectedAction(context);

        if (!isProtectedAction)
        {
            return;
        }

        // Check if the user is authenticated
        if (!IsUserAuthenticated(context))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        string userName = context.HttpContext.User.Identity!.Name!;

        // Check if the user is in the list of default allowed admins
        if (DynamicAuthorizationOptionsInternals.DefaultAllowedAdmins.Contains(userName))
        {
            return;
        }

        // Get the roles of the user
        string[] roles = await (
            from user in _dbContext.Users
            join userRole in _dbContext.UserRoles on user.Id equals userRole.UserId
            join role in _dbContext.Roles on userRole.RoleId equals role.Id
            where user.UserName == userName
            select role.Id.ToString()
        ).AsSplitQuery()
         .ToArrayAsync();

        // Check if the user has any of the default allowed roles
        if (roles.Any(DynamicAuthorizationOptionsInternals.DefaultAllowedRoles.Contains))
        {
            return;
        }

        // Check if the user has access to the action
        if (await _roleAccessStore.HasAccessToActionAsync(actionId, roles))
        {
            return;
        }

        // If none of the conditions are met, forbid the request
        context.Result = new ForbidResult();
    }

    private static (bool isProtectedAction, string actionId) IsProtectedAction(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
        {
            return (false, string.Empty);
        }

        string actionId = GetActionId(context);
        if (s_actionDict.TryGetValue(actionId, out bool isProtected))
        {
            return (isProtected, actionId);
        }

        TypeInfo controllerTypeInfo = controllerActionDescriptor.ControllerTypeInfo;

        // Check if the controller has the AllowAnonymous attribute
        AllowAnonymousAttribute? anonymousAttribute = controllerTypeInfo.GetCustomAttribute<AllowAnonymousAttribute>();
        if (anonymousAttribute != null)
        {
            s_actionDict.Add(actionId, false);
            return (false, actionId);
        }

        MethodInfo actionMethodInfo = controllerActionDescriptor.MethodInfo;
        // Check if the action method has the AllowAnonymous attribute
        anonymousAttribute = actionMethodInfo.GetCustomAttribute<AllowAnonymousAttribute>();
        if (anonymousAttribute != null)
        {
            s_actionDict.Add(actionId, false);
            return (false, actionId);
        }

        // Check if the controller has the Authorize attribute
        AuthorizeAttribute? authorizeAttribute = controllerTypeInfo.GetCustomAttribute<AuthorizeAttribute>();
        if (authorizeAttribute != null)
        {
            s_actionDict.Add(actionId, true);
            return (true, actionId);
        }

        // Check if the action method has the Authorize attribute
        authorizeAttribute = actionMethodInfo.GetCustomAttribute<AuthorizeAttribute>();
        isProtected = authorizeAttribute != null;
        s_actionDict.Add(actionId, isProtected);

        return (isProtected, actionId);
    }

    private static bool IsUserAuthenticated(AuthorizationFilterContext context)
        => context.HttpContext.User.Identity?.IsAuthenticated ?? false;

    private static string GetActionId(AuthorizationFilterContext context)
    {
        ControllerActionDescriptor controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
        string? area = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<AreaAttribute>()?.RouteValue;
        string controller = controllerActionDescriptor.ControllerName;
        string action = controllerActionDescriptor.ActionName;

        return $"{area}:{controller}:{action}";
    }
}
