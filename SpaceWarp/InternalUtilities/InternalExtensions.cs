using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceWarp.InternalUtilities;

internal static class InternalExtensions
{
    internal static void Persist(this UnityObject obj)
    {
        UnityObject.DontDestroyOnLoad(obj);
        obj.hideFlags |= HideFlags.HideAndDontSave;
    }
}