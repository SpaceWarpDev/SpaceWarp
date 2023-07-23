using System;
using System.Runtime.CompilerServices;
using SpaceWarp.API.Assets;
using UnityEngine;

namespace SpaceWarp.API.UI;

// This exposes the SpaceWarp internal skins
[Obsolete("Spacewarps support for IMGUI will not be getting updates, please use UITK instead")]
public static class Skins
{
    private static GUISkin _skin;
    [Obsolete("Spacewarps support for IMGUI will not be getting updates, please use UITK instead")]
    public static GUISkin ConsoleSkin{
        get
        {
            if (!_skin)
            {
                AssetManager.TryGetAsset($"{SpaceWarpPlugin.ModGuid}/swconsoleui/spacewarpconsole.guiskin", out _skin);
            }

            return _skin;
        }
    }
}