using UnityEngine;

namespace SpaceWarp;

internal static class InternalExtensions
{
    public static void Persist(this UnityObject obj)
    {
        UnityObject.DontDestroyOnLoad(obj);
        obj.hideFlags |= HideFlags.HideAndDontSave;
    }
}
