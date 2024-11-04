namespace DynamicAuthorization.Mvc.Core;

public class RoleAccess
{
    public int Id { get; set; }

    public string RoleId { get; set; } = null!;

    public IEnumerable<MvcControllerInfo> Controllers { get; set; } = new List<MvcControllerInfo>();
}
