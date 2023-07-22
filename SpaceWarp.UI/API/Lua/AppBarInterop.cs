using System;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using SpaceWarp.API.Assets;
using SpaceWarp.API.UI.Appbar;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable UnusedMember.Global
namespace SpaceWarp.API.Lua;

[SpaceWarpLuaAPI("AppBar")]
// ReSharper disable once UnusedType.Global
public static class AppBarInterop
{
    public static Sprite GetSprite(string texturePath, int width = 0, int height = 0)
    {
        return GetSprite(AssetManager.GetAsset<Texture2D>(texturePath));
    }
    
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
    
    public static void RegisterAppButton(bool oab, bool inFlight, string name, string ID, Sprite icon,
        Closure toggleCallback, [CanBeNull] DynValue self = null)
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
            Appbar.RegisterAppButton(name, ID, icon, callback);
        }

        if (inFlight)
        {
            Appbar.RegisterOABAppButton(name, ID, icon, callback);
        }
    }

    public static void RegisterAppButton(bool oab, bool inFlight, string name, string ID, string texturePath,
        Closure toggleCallback, [CanBeNull] DynValue self = null) =>
        RegisterAppButton(oab, inFlight, name, ID, GetSprite(texturePath), toggleCallback, self);
    
    public static void RegisterAppWindow(bool oab, bool inFlight, string name, string ID, Sprite icon, VisualElement window, Closure toggleCallback, [CanBeNull] DynValue self = null)
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
            Appbar.RegisterAppButton(name, ID, icon, callback);
        }

        if (inFlight)
        {
            Appbar.RegisterOABAppButton(name, ID, icon, callback);
        }
    }

    public static void RegisterAppWindow(bool oab, bool inFlight, string name, string ID, string texturePath, VisualElement window,
        Closure toggleCallback, [CanBeNull] DynValue self = null) =>
        RegisterAppWindow(oab, inFlight, name, ID, GetSprite(texturePath), window, toggleCallback, self);

    public static void SetAppButtonIndicator(string id, bool b)
    {
        Appbar.SetAppBarButtonIndicator(id, b);
    }
}