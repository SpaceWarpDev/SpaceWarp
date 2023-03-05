using UnityEngine;

namespace SpaceWarp.API.UI;

// This exposes the SpaceWarp internal skins
public static class Skins
{
    public static GUISkin ConsoleSkin => SpaceWarpManager.Skin;
}