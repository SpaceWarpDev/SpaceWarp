using JetBrains.Annotations;
using MoonSharp.Interpreter;
using SpaceWarp.API.Assets;
using SpaceWarp.API.UI.Appbar;
using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceWarp.API.Lua;

/// <summary>
/// Lua API for the AppBar.
/// </summary>
[SpaceWarpLuaAPI("AppBar")]
[PublicAPI]
public static class AppBarInterop
{
    /// <summary>
    /// Gets a sprite from a texture path.
    /// </summary>
    /// <param name="texturePath">Path to the texture.</param>
    /// <param name="width">Width of the sprite.</param>
    /// <param name="height">Height of the sprite.</param>
    /// <returns>The sprite.</returns>
    public static Sprite GetSprite(string texturePath, int width = 0, int height = 0)
    {
        return GetSprite(AssetManager.GetAsset<Texture2D>(texturePath));
    }

    /// <summary>
    /// Gets a sprite from a texture.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <param name="width">Width of the sprite.</param>
    /// <param name="height">Height of the sprite.</param>
    /// <returns></returns>
    public static Sprite GetSprite(Texture2D texture, int width = 0, int height = 0)
    {
        if (width == 0)
        {
            width = texture.width;
        }

        if (height == 0)
        {
            height = texture.height;
        }

        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// Registers an app button.
    /// </summary>
    /// <param name="oab">Whether the button should be registered in the OAB.</param>
    /// <param name="inFlight">Whether the button should be registered in flight.</param>
    /// <param name="name">The text of the button.</param>
    /// <param name="id">The ID of the button.</param>
    /// <param name="icon">The icon of the button.</param>
    /// <param name="toggleCallback">The callback to be called when the button is toggled.</param>
    /// <param name="self">The self parameter for the callback.</param>
    public static void RegisterAppButton(
        bool oab,
        bool inFlight,
        string name,
        string id,
        Sprite icon,
        Closure toggleCallback,
        [CanBeNull] DynValue self = null
    )
    {
        Action<bool> callback;
        if (self != null)
        {
            callback = b => toggleCallback.Call(self, b);
        }
        else
        {
            callback = b => toggleCallback.Call(b);
        }

        if (oab)
        {
            Appbar.RegisterAppButton(name, id, icon, callback);
        }

        if (inFlight)
        {
            Appbar.RegisterOABAppButton(name, id, icon, callback);
        }
    }

    /// <summary>
    /// Registers an app button.
    /// </summary>
    /// <param name="oab">Whether the button should be registered in the OAB.</param>
    /// <param name="inFlight">Whether the button should be registered in flight.</param>
    /// <param name="name">The text of the button.</param>
    /// <param name="id">The ID of the button.</param>
    /// <param name="texturePath">The path to the texture of the button.</param>
    /// <param name="toggleCallback">The callback to be called when the button is toggled.</param>
    /// <param name="self">The self parameter for the callback.</param>
    public static void RegisterAppButton(
        bool oab,
        bool inFlight,
        string name,
        string id,
        string texturePath,
        Closure toggleCallback,
        [CanBeNull] DynValue self = null
    ) => RegisterAppButton(oab, inFlight, name, id, GetSprite(texturePath), toggleCallback, self);

    /// <summary>
    /// Registers an app window.
    /// </summary>
    /// <param name="oab">Whether the window should be registered in the OAB.</param>
    /// <param name="inFlight">Whether the window should be registered in flight.</param>
    /// <param name="name">The text of the button that opens the window.</param>
    /// <param name="id">The ID of the button that opens the window.</param>
    /// <param name="icon">The icon of the button that opens the window.</param>
    /// <param name="window">The window.</param>
    /// <param name="toggleCallback">The callback to be called when the button is toggled.</param>
    /// <param name="self">The self parameter for the callback.</param>
    public static void RegisterAppWindow(
        bool oab,
        bool inFlight,
        string name,
        string id,
        Sprite icon,
        VisualElement window,
        Closure toggleCallback,
        [CanBeNull] DynValue self = null
    )
    {
        Action<bool> callback;
        if (self != null)
        {
            callback = b =>
            {
                window.visible = b;
                toggleCallback.Call(self, b);
            };
        }
        else
        {
            callback = b =>
            {
                window.visible = b;
                toggleCallback.Call(b);
            };
        }

        if (oab)
        {
            Appbar.RegisterAppButton(name, id, icon, callback);
        }

        if (inFlight)
        {
            Appbar.RegisterOABAppButton(name, id, icon, callback);
        }
    }

    /// <summary>
    /// Registers an app window.
    /// </summary>
    /// <param name="oab">Whether the window should be registered in the OAB.</param>
    /// <param name="inFlight">Whether the window should be registered in flight.</param>
    /// <param name="name">The text of the button that opens the window.</param>
    /// <param name="id">The ID of the button that opens the window.</param>
    /// <param name="texturePath">The path to the texture of the button that opens the window.</param>
    /// <param name="window">The window.</param>
    /// <param name="toggleCallback">The callback to be called when the button is toggled.</param>
    /// <param name="self">The self parameter for the callback.</param>
    public static void RegisterAppWindow(
        bool oab,
        bool inFlight,
        string name,
        string id,
        string texturePath,
        VisualElement window,
        Closure toggleCallback,
        [CanBeNull] DynValue self = null
    ) => RegisterAppWindow(oab, inFlight, name, id, GetSprite(texturePath), window, toggleCallback, self);

    /// <summary>
    /// Sets the indicator of an app button.
    /// </summary>
    /// <param name="id">The ID of the button.</param>
    /// <param name="b">Whether the indicator should be set.</param>
    public static void SetAppButtonIndicator(string id, bool b)
    {
        Appbar.SetAppBarButtonIndicator(id, b);
    }
}