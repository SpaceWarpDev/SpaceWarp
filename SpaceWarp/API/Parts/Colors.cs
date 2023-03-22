using System;
using System.Collections.Generic;
using BepInEx.Logging;
using SpaceWarp.Patching;

namespace SpaceWarp.API.Parts
{
    public static class Colors
    {
        /// <summary>
        /// Key is ModGUID.
        /// Value is list of strings containing the partNames.
        /// Only parts in this list will be modified.
        /// </summary>
        public static Dictionary<string, string[]> DeclaredParts => ColorsPatch.DeclaredParts;
        private static ManualLogSource Logger => ColorsPatch.Logger;

        /// <summary>
        /// Adds <paramref name="partNameList"/> to internal parts list under <paramref name="modGUID"/>
        /// allowing them to have the patch applied.
        /// </summary>
        /// <param name="modGUID">guid of the mod that owns the parts.</param>
        /// <param name="partNameList">Collection of partNames. Names that end in XS, S, M, L or XL will be counted as the same part,</param>
        public static void DeclareParts(string modGUID, params string[] partNameList) => ColorsPatch.DeclareParts(modGUID, partNameList);
        /// <summary>
        /// Adds <paramref name="partNameList"/> to internal parts list under <paramref name="modGUID"/>
        /// allowing them to have the patch applied.
        /// </summary>
        /// <param name="modGUID">guid of the mod that owns the parts.</param>
        /// <param name="partNameList">Collection of partNames. Names that end in XS, S, M, L or XL will be counted as the same part.</param>
        public static void DeclareParts(string modGUID, IEnumerable<string> partNameList) => ColorsPatch.DeclareParts(modGUID, partNameList);
    }
}
