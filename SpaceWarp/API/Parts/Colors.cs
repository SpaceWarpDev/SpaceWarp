using System.Collections.Generic;
using SpaceWarp.Patching;

namespace SpaceWarp.API.Parts;

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
    /// </param>
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
    /// </param>
    public static void DeclareParts(string modGuid, IEnumerable<string> partNameList)
    {
        ColorsPatch.DeclareParts(modGuid, partNameList);
    }
}