using System;
using System.Reflection;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace SpaceWarp.InternalUtilities;


#if UNITY_EDITOR
internal static class AddressablesFixes
{
    internal static MethodInfo OriginalThunderkitMethod;

    static AddressablesFixes()
    {
        Type t = Type.GetType("ThunderKit.Addressable.Tools.AddressableGraphicsSettings, ThunderKit.Addressable.Tools")!;
        OriginalThunderkitMethod = t.GetMethod("RedirectInternalIdsToGameDirectory", BindingFlags.Static | BindingFlags.NonPublic)!;
    }
    
    
    internal static string RedirectInternalIdsToGameDirectoryFixed(IResourceLocation location)
    {
        if (location.InternalId.Contains("Mods") || location.InternalId.Contains("Redux/Addressables")) return location.InternalId;
        return (string)OriginalThunderkitMethod.Invoke(null, new object[] {location});
    }
}
#endif