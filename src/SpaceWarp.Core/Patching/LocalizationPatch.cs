using HarmonyLib;
using I2.Loc;
using UnityEngine;

namespace SpaceWarp.Patching;

/// <summary>
/// Patches the localization system to properly find fallback translations.
/// </summary>
[HarmonyPatch]
internal static class LocalizationPatch
{
    [HarmonyPatch(
        typeof(LocalizationManager),
        nameof(LocalizationManager.TryGetTranslation),
        [
            typeof(string), typeof(string), typeof(bool), typeof(int), typeof(bool), typeof(bool),
            typeof(GameObject), typeof(string), typeof(bool), typeof(string)
        ],
        [
            ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
            ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal
        ]
    )]
    [HarmonyPrefix]
    // ReSharper disable InconsistentNaming
    private static bool LocalizationManagerTryGetTranslationPrefix(
        ref bool __result,
        string Term,
        out string Translation,
        bool FixForRTL = true,
        int maxLineLengthForRTL = 0,
        bool ignoreRTLnumbers = true,
        bool applyParameters = false,
        GameObject localParametersRoot = null,
        string overrideLanguage = null,
        bool allowLocalizedParameters = true,
        string overrideSpecialization = null
    )
    {
        // ReSharper restore InconsistentNaming
        __result = TryGetTranslation(
            Term,
            out Translation,
            FixForRTL,
            maxLineLengthForRTL,
            ignoreRTLnumbers,
            applyParameters,
            localParametersRoot,
            overrideLanguage,
            allowLocalizedParameters,
            overrideSpecialization
        );

        return false;
    }

    private static bool TryGetTranslation(
        string term,
        out string translation,
        bool fixForRtl,
        int maxLineLengthForRtl,
        bool ignoreRtlNumbers,
        bool applyParameters,
        GameObject localParametersRoot,
        string overrideLanguage,
        bool allowLocalizedParameters,
        string overrideSpecialization
    )
    {
        translation = null;
        if (string.IsNullOrEmpty(term))
        {
            return false;
        }

        if (LocalizationManager.DebugLocalizationIsOn)
        {
            translation = term;
            return true;
        }

        LocalizationManager.InitializeIfNeeded();

        foreach (var source in LocalizationManager.Sources)
        {
            if (!TryGetTranslationFromSource(
                    source,
                    term,
                    out translation,
                    overrideLanguage,
                    overrideSpecialization
                ))
            {
                continue;
            }

            if (applyParameters)
            {
                LocalizationManager.ApplyLocalizationParams(
                    ref translation,
                    localParametersRoot,
                    allowLocalizedParameters
                );
            }

            if (LocalizationManager.IsRight2Left & fixForRtl)
            {
                translation = LocalizationManager.ApplyRTLfix(translation, maxLineLengthForRtl, ignoreRtlNumbers);
            }

            return true;
        }

        return TryGetFallback(
            term,
            ref translation,
            fixForRtl,
            maxLineLengthForRtl,
            ignoreRtlNumbers,
            applyParameters,
            localParametersRoot,
            allowLocalizedParameters,
            overrideSpecialization
        );
    }

    private static bool TryGetTranslationFromSource(
        LanguageSourceData source,
        string term,
        out string translation,
        string overrideLanguage = null,
        string overrideSpecialization = null,
        bool skipDisabled = false,
        bool allowCategoryMistmatch = false
    )
    {
        var languageIndex = source.GetLanguageIndex(
            overrideLanguage ?? LocalizationManager.CurrentLanguage,
            SkipDisabled: false
        );

        if (languageIndex >= 0 && (!skipDisabled || source.mLanguages[languageIndex].IsEnabled()))
        {
            var termData = source.GetTermData(term, allowCategoryMistmatch);
            if (termData != null)
            {
                translation = termData.GetTranslation(languageIndex, overrideSpecialization, true);
                if (translation == "---")
                {
                    translation = string.Empty;
                    return true;
                }

                if (!string.IsNullOrEmpty(translation))
                {
                    return true;
                }
            }
        }

        translation = null;
        return false;
    }

    private static bool TryGetFallback(
        string term,
        ref string translation,
        bool fixForRtl,
        int maxLineLengthForRtl,
        bool ignoreRtlNumbers,
        bool applyParameters,
        GameObject localParametersRoot,
        bool allowLocalizedParameters,
        string overrideSpecialization
    )
    {
        if (!string.IsNullOrEmpty(translation) && !translation.Equals(term))
        {
            return true;
        }

        foreach (var source in LocalizationManager.Sources)
        {
            switch (source.OnMissingTranslation)
            {
                case LanguageSourceData.MissingTranslationAction.ShowWarning:
                    translation = "<!-Missing Translation [" + term + "]-!>";
                    return true;
                case LanguageSourceData.MissingTranslationAction.Empty:
                    translation = string.Empty;
                    return true;
                case LanguageSourceData.MissingTranslationAction.ShowTerm:
                    translation = term;
                    return true;
            }

            if (!TryGetFallbackFromSource(source, term, out var fallback, overrideSpecialization))
            {
                continue;
            }

            if (applyParameters)
            {
                LocalizationManager.ApplyLocalizationParams(
                    ref fallback,
                    localParametersRoot,
                    allowLocalizedParameters
                );
            }

            if (LocalizationManager.IsRight2Left & fixForRtl)
            {
                fallback = LocalizationManager.ApplyRTLfix(fallback, maxLineLengthForRtl, ignoreRtlNumbers);
            }

            translation = fallback;
            return true;
        }

        return false;
    }

    private static bool TryGetFallbackFromSource(
        LanguageSourceData source,
        string term,
        out string translation,
        string overrideSpecialization
    )
    {
        translation = null;

        for (var index = 0; index < source.mLanguages.Count; ++index)
        {
            var termData = source.GetTermData(term);
            if (termData == null)
            {
                continue;
            }

            translation = termData.GetTranslation(index, overrideSpecialization, true);
            if (!string.IsNullOrEmpty(translation))
            {
                return true;
            }
        }

        return false;
    }
}