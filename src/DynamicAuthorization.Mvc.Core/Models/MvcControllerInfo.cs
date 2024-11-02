namespace DynamicAuthorization.Mvc.Core;

public class MvcControllerInfo
{
    public string Id => $"{AreaName}:{Name}";

    public string Name { get; init; } = null!;

    public string? DisplayName { get; init; }

    public string? AreaName { get; init; }

    public IEnumerable<MvcActionInfo> Actions { get; set; } = null!;
}