using HarmonyLib;
using I2.Loc;

namespace SpaceWarp.Patching;

/// <summary>
/// Patches the localization system so that localization sources that don't contain the current language
/// are not skipped when looking for a fallback translation.
/// </summary>
[HarmonyPatch]
public static class LocalizationPatch
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

        if (!__instance.TryGetFallbackTranslation(
                termData,
                out var translation,
                -1,
                overrideSpecialization,
                skipDisabled)
            )
        {
            return;
        }

        Translation = translation;
        __result = true;
    }

    [HarmonyPatch(typeof(LanguageSourceData), nameof(LanguageSourceData.TryGetFallbackTranslation))]
    [HarmonyPrefix]
    private static bool TryGetFallbackTranslation(
        // ReSharper disable once InconsistentNaming
        LanguageSourceData __instance,
        // ReSharper disable once InconsistentNaming
        ref bool __result,
        TermData termData,
        // ReSharper disable once InconsistentNaming
        out string Translation,
        int langIndex,
        string overrideSpecialization = null,
        bool skipDisabled = false)
    {
        var str = langIndex != -1 && __instance.mLanguages.Count > langIndex
            ? __instance.mLanguages[langIndex].Code
            : null;
        if (!string.IsNullOrEmpty(str))
        {
            if (str.Contains('-'))
            {
                str = str[..str.IndexOf('-')];
            }

            for (var index = 0; index < __instance.mLanguages.Count; ++index)
            {
                if (index == langIndex ||
                    !__instance.mLanguages[index].Code.StartsWith(str, StringComparison.Ordinal) ||
                    (skipDisabled && !__instance.mLanguages[index].IsEnabled()))
                {
                    continue;
                }

                Translation = termData.GetTranslation(index, overrideSpecialization, true);
                if (string.IsNullOrEmpty(Translation))
                {
                    continue;
                }

                __result = true;
                return false;
            }
        }

        for (var index = 0; index < __instance.mLanguages.Count; ++index)
        {
            if (index == langIndex ||
                (skipDisabled && !__instance.mLanguages[index].IsEnabled()) ||
                (str != null && __instance.mLanguages[index].Code.StartsWith(str, StringComparison.Ordinal)))
            {
                continue;
            }

            Translation = termData.GetTranslation(index, overrideSpecialization, true);
            if (string.IsNullOrEmpty(Translation))
            {
                continue;
            }

            __result = true;
            return false;
        }

        Translation = null;
        __result = false;
        return false;
    }
}