using JetBrains.Annotations;
using UnityEngine.UIElements;

namespace SpaceWarp.API.UI;

/// <summary>
/// API for the mod list UI
/// </summary>
[PublicAPI]
public static class ModList
{
    /// <summary>
    /// Register a function to generate a foldout in the mods list for your mods details
    /// Use only in PostInitialize()
    /// </summary>
    /// <param name="modGuid">The guid of your mod</param>
    /// <param name="generateFoldoutOnOpenAction">The action that generates your foldout</param>
    public static void RegisterDetailsFoldoutGenerator(string modGuid, Func<VisualElement> generateFoldoutOnOpenAction)
    {
        var modListController = Modules.UI.Instance.ModListController;
        if (modListController.BoundItems.TryGetValue(modGuid, out var controller))
        {
            controller.GetDetails = generateFoldoutOnOpenAction;
        }
    }
}