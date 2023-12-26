using JetBrains.Annotations;
using SpaceWarp.API.Assets;
using UnityEngine;

namespace SpaceWarp.API.UI;

// This exposes the SpaceWarp internal skins
[Obsolete("SpaceWarp's support for IMGUI will not be getting updates, please use UITK instead")]
[PublicAPI]
public static class Skins
{
    private static GUISkin _skin;

    [Obsolete("SpaceWarp's support for IMGUI will not be getting updates, please use UITK instead")]
    public static GUISkin ConsoleSkin
    {
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