using JetBrains.Annotations;
using UnityEngine;

namespace SpaceWarp2.API;

/// <summary>
/// Contains paths to various directories.
/// </summary>
[PublicAPI]
public static class CommonPaths
{
    public static string ModsFolder = "Assets/Mods";
    public static string LuaFolder = "Assets/Lua";
    public static string DisabledPlugins = "disabled_plugins.cfg";
    public static string HashLocation = "mod_list_hash.txt";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        ModsFolder = Application.isEditor ? "Assets/Mods" : "./mods";
        LuaFolder = Application.isEditor ? "Assets/Lua" : "./scripts";
        DisabledPlugins = "disabled_plugins.cfg";
        HashLocation = "mod_list_hash.txt";
    }
}
