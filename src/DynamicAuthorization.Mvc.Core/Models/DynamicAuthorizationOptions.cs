namespace DynamicAuthorization.Mvc.Core;

public class DynamicAuthorizationOptions
{
    public List<string> DefaultAllowedAdmins { get; set; } = new();

    public List<string> DefaultAllowedRoles { get; set; } = new();
}
