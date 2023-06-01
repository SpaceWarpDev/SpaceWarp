using System;
using UnityEngine;

namespace SpaceWarp.API.UI;

// This exposes the SpaceWarp internal skins
[Obsolete("Spacewarps support for IMGUI will not be getting updates, please use UITK instead")]
public static class Skins
{
    [Obsolete("Spacewarps support for IMGUI will not be getting updates, please use UITK instead")]
    public static GUISkin ConsoleSkin => SpaceWarpManager.Skin;
}