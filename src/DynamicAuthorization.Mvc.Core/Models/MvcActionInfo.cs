namespace DynamicAuthorization.Mvc.Core;

public record MvcActionInfo
{
    public string Id => $"{ControllerId}:{Name}";

    public string Name { get; init; } = null!;

    public string? DisplayName { get; init; }

    public string ControllerId { get; init; } = null!;
}