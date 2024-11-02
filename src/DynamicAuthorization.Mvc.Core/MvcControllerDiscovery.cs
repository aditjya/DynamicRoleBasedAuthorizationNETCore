using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace DynamicAuthorization.Mvc.Core;

/// <inheritdoc cref="IMvcControllerDiscovery"/>
public class MvcControllerDiscovery : IMvcControllerDiscovery
{
    private readonly List<MvcControllerInfo> _mvcControllers = new();
    private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

    public MvcControllerDiscovery(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider) =>
        _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;

    /// <inheritdoc/>
    public IEnumerable<MvcControllerInfo> GetControllers()
    {
        // Check if the list of MVC controllers has already been populated
        if (_mvcControllers.Count != 0)
        {
            return _mvcControllers;
        }

        // Retrieve the list of action descriptors from the action descriptor collection provider
        List<IGrouping<string?, ControllerActionDescriptor>> items = _actionDescriptorCollectionProvider
            .ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Select(descriptor => descriptor)
            .GroupBy(descriptor => descriptor.ControllerTypeInfo.FullName)
            .ToList();

        // Iterate through the action descriptors grouped by controller type
        foreach (IGrouping<string?, ControllerActionDescriptor> actionDescriptors in items)
        {
            // Skip if there are no action descriptors
            if (!actionDescriptors.Any())
            {
                continue;
            }

            // Get the first action descriptor in the group
            ControllerActionDescriptor actionDescriptor = actionDescriptors.First();
            TypeInfo controllerTypeInfo = actionDescriptor.ControllerTypeInfo;

            // Create a new MvcControllerInfo instance
            MvcControllerInfo currentController = new()
            {
                AreaName = controllerTypeInfo.GetCustomAttribute<AreaAttribute>()?.RouteValue,
                DisplayName = controllerTypeInfo.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName,
                Name = actionDescriptor.ControllerName
            };

            // Create a list to store the MvcActionInfo instances
            List<MvcActionInfo> actions = new();

            // Iterate through the action descriptors grouped by action name
            foreach (ControllerActionDescriptor descriptor in actionDescriptors.GroupBy(a => a.ActionName).Select(g => g.First()))
            {
                MethodInfo methodInfo = descriptor.MethodInfo;

                // Check if the action is protected
                if (IsProtectedAction(controllerTypeInfo, methodInfo))
                {
                    // Create a new MvcActionInfo instance and add it to the actions list
                    actions.Add(new MvcActionInfo
                    {
                        ControllerId = currentController.Id,
                        Name = descriptor.ActionName,
                        DisplayName = methodInfo.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
                    });
                }
            }

            // Check if there are any actions for the current controller
            if (actions.Count > 0)
            {
                // Set the actions property of the current controller and add it to the list of MVC controllers
                currentController.Actions = actions;
                _mvcControllers.Add(currentController);
            }
        }

        // Return the list of MVC controllers
        return _mvcControllers;
    }

    private static bool IsProtectedAction(MemberInfo controllerTypeInfo, MemberInfo actionMethodInfo)
    {
        if (actionMethodInfo.GetCustomAttribute<AllowAnonymousAttribute>(true) != null)
        {
            return false;
        }

        if (controllerTypeInfo.GetCustomAttribute<AuthorizeAttribute>(true) != null)
        {
            return true;
        }

        return actionMethodInfo.GetCustomAttribute<AuthorizeAttribute>(true) != null;
    }
}