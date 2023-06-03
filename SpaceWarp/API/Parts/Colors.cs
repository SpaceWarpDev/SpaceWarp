using System;
using System.Collections.Generic;
using BepInEx.Logging;
using SpaceWarp.API.Lua;
using SpaceWarp.Patching;

namespace SpaceWarp.API.Parts;

[SpaceWarpLuaAPI("Colors")]
public static class Colors
{
    /// <summary>
    ///     Key is ModGUID.
    ///     Value is list of strings containing the partNames.
    ///     Only parts in this list will be modified.
    /// </summary>
    public static Dictionary<string, string[]> DeclaredParts => ColorsPatch.DeclaredParts;

    /// <summary>
    ///     Adds <paramref name="partNameList" /> to internal parts list under <paramref name="modGuid" />
    ///     allowing them to have the patch applied.
    /// </summary>
    /// <param name="modGuid">guid of the mod that owns the parts.</param>
    /// <param name="partNameList">
    ///     Collection of partNames. Names that end in XS, S, M, L or XL will be counted as the same
    ///     part,
    ///     Example: partNameS, partNameM, partNameL, partNameXL are all treated as partName
    /// </param>
    [Obsolete("This should only be used for testing purposes (for easier reloading of textures). Use the shader \"Parts Replace\" instead")]
    public static void DeclareParts(string modGuid, params string[] partNameList)
    {
        ColorsPatch.DeclareParts(modGuid, partNameList);
    }

    /// <summary>
    ///     Adds <paramref name="partNameList" /> to internal parts list under <paramref name="modGuid" />
    ///     allowing them to have the patch applied.
    /// </summary>
    /// <param name="modGuid">guid of the mod that owns the parts.</param>
    /// <param name="partNameList">
    ///     Collection of partNames. Names that end in XS, S, M, L or XL will be counted as the same
    ///     part.
    ///     Example: partNameS, partNameM, partNameL, partNameXL are all treated as partName
    /// </param>
    [Obsolete("This should only be used for testing purposes (for easier reloading of textures). Use the shader \"Parts Replace\" instead")]
    public static void DeclareParts(string modGuid, IEnumerable<string> partNameList)
    {
        ColorsPatch.DeclareParts(modGuid, partNameList);
    }
    /// <summary>
    /// Retrieves all the texture list from your part. Textures not set will be null.
    /// </summary>
    /// <param name="partName">Name of the part as described in the .json.</param>
    /// <returns></returns>
    public static UnityEngine.Texture[] GetTextures(string partName) => ColorsPatch.GetTextures(partName);
}