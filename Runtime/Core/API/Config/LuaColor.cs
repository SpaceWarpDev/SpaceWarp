using MoonSharp.Interpreter;
using UnityEngine;

namespace SpaceWarp2.API.Config;

/// <summary>
/// The Lua-side representation of a color config value.
/// </summary>
/// <remarks>
/// Exposes the channels as both named fields (<c>c.r</c>) and a 1-based index (<c>c[1]</c>), backed by the
/// same storage so the two views can never diverge. Stored as a <c>UnityEngine.Color</c> on the config side,
/// so the settings menu renders it as a color picker.
/// </remarks>
[MoonSharpUserData]
public sealed class LuaColor
{
    /// <summary>
    /// The red channel.
    /// </summary>
    public double r;

    /// <summary>
    /// The green channel.
    /// </summary>
    public double g;

    /// <summary>
    /// The blue channel.
    /// </summary>
    public double b;

    /// <summary>
    /// The alpha channel.
    /// </summary>
    public double a;

    static LuaColor()
    {
        UserData.RegisterType<LuaColor>();
    }

    public LuaColor(double r, double g, double b, double a)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    /// <summary>
    /// 1-based channel access matching Lua convention: [1] is r, [2] is g, [3] is b, [4] is a.
    /// </summary>
    /// <param name="index">The 1-based channel index.</param>
    public double this[int index]
    {
        get => index switch
        {
            1 => r,
            2 => g,
            3 => b,
            4 => a,
            _ => 0
        };
        set
        {
            switch (index)
            {
                case 1:
                    r = value;
                    break;
                case 2:
                    g = value;
                    break;
                case 3:
                    b = value;
                    break;
                case 4:
                    a = value;
                    break;
            }
        }
    }

    public Color ToColor() => new((float)r, (float)g, (float)b, (float)a);

    public static LuaColor FromColor(Color c) => new(c.r, c.g, c.b, c.a);
}
