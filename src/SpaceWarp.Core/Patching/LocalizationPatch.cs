using HarmonyLib;
using I2.Loc;

namespace SpaceWarp.Patching;

/// <summary>
/// Patches the localization system so that localization sources that don't contain the current language
/// are not skipped when looking for a fallback translation.
/// </summary>
[HarmonyPatch]
public class LocalizationPatch
{
    [HarmonyPatch(typeof(LanguageSourceData), nameof(LanguageSourceData.TryGetTranslation))]
    [HarmonyPostfix]
    private static void TryGetTranslationPatch(
        // ReSharper disable once InconsistentNaming
        LanguageSourceData __instance,
        // ReSharper disable once InconsistentNaming
        ref bool __result,
        string term,
        // ReSharper disable once InconsistentNaming
        ref string Translation,
        string overrideLanguage = null,
        string overrideSpecialization = null,
        bool skipDisabled = false,
        bool allowCategoryMistmatch = false)
    {
        if (__result && !string.IsNullOrEmpty(Translation) && !Translation.Equals(term))
        {
            return;
        }

        var termData = __instance.GetTermData(term, allowCategoryMistmatch);
        if (__instance.OnMissingTranslation != LanguageSourceData.MissingTranslationAction.Fallback ||
            termData == null)
        {
            return;
        }

        if (!__instance.TryGetFallbackTranslation(termData, out var translation, -1,
                overrideSpecialization, skipDisabled))
        {
            return;
        }

        Translation = translation;
        __result = true;
    }
}