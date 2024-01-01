using I2.Loc;

namespace SpaceWarp.InternalUtilities;

internal static class LocalizationHelpers
{
    public static void AddSource(LanguageSourceData source)
    {
        if (LocalizationManager.Sources.Contains(source))
        {
            return;
        }

        source.OnMissingTranslation = LanguageSourceData.MissingTranslationAction.Fallback;

        LocalizationManager.Sources.Insert(0, source);
        foreach (var language in source.mLanguages)
        {
            language.SetLoaded(true);
        }

        if (source.mDictionary.Count != 0)
        {
            return;
        }

        source.UpdateDictionary(true);
    }
}