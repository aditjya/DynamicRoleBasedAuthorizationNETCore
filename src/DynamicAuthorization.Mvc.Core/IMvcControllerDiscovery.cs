namespace DynamicAuthorization.Mvc.Core;

/// <summary>
///   Represents a contract for discovering MVC controllers.
/// </summary>
public interface IMvcControllerDiscovery
{
    /// <summary>
    ///   Gets the list of MVC controllers.
    /// </summary>
    /// <returns>The list of MVC controllers.</returns>
    IEnumerable<MvcControllerInfo> GetControllers();
}