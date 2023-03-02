using UnityEngine;

namespace SpaceWarp;

internal static class InternalExtensions
{
    public static void Persist(this UnityObject o)
    {
        UnityObject.DontDestroyOnLoad(o);
        o.hideFlags |= HideFlags.HideAndDontSave;
    }
}
