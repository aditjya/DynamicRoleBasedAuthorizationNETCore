namespace DynamicAuthorization.Mvc.Core.Models;

internal class DynamicAuthorizationOptionsInternals
{
    internal static HashSet<string> DefaultAllowedAdmins { get; set; } = new();

    internal static HashSet<string> DefaultAllowedRoles { get; set; } = new();

    internal static Type DbContextType { get; set; } = null!;

    internal static Type UserType { get; set; } = null!;

    internal static Type RoleType { get; set; } = null!;

    internal static Type KeyType { get; set; } = null!;

    internal static Type UserClaimType { get; set; } = null!;

    internal static Type UserRoleType { get; set; } = null!;

    internal static Type UserLoginType { get; set; } = null!;

    internal static Type RoleClaimType { get; set; } = null!;

    internal static Type UserTokenType { get; set; } = null!;
}
