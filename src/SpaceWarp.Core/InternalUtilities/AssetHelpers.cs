using I2.Loc;
using KSP.Game;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SpaceWarp.InternalUtilities;

internal static class AssetHelpers
{
    public static void LoadAddressable(string catalog)
    {
        SpaceWarpPlugin.Instance.SWLogger.LogInfo($"Attempting to load {catalog}");
        var operation = Addressables.LoadContentCatalogAsync(catalog);
        operation.WaitForCompletion();
        if (operation.Status == AsyncOperationStatus.Failed)
        {
            SpaceWarpPlugin.Instance.SWLogger.LogError($"Failed to load addressables catalog {catalog}");
        }
        else
        {
            SpaceWarpPlugin.Instance.SWLogger.LogInfo($"Loaded addressables catalog {catalog}");
            var locator = operation.Result;
            SpaceWarpPlugin.Instance.SWLogger.LogInfo($"{catalog} ----- {locator.LocatorId}");
            GameManager.Instance.Assets.RegisterResourceLocator(locator);
        }
    }

    internal static void LoadLocalizationFromFolder(string folder)
    {
        SpaceWarpPlugin.Instance.SWLogger.LogInfo($"Attempting to load localizations from {folder}");
        LanguageSourceData languageSourceData = null;
        if (!Directory.Exists(folder))
        {
            SpaceWarpPlugin.Instance.SWLogger.LogInfo($"{folder} does not exist, not loading localizations.");
            return;
        }

        var info = new DirectoryInfo(folder);
        foreach (var csvFile in info.GetFiles("*.csv", SearchOption.AllDirectories))
        {
            languageSourceData ??= new LanguageSourceData();
            var csvData = File.ReadAllText(csvFile.FullName).Replace("\r\n", "\n");
            languageSourceData.Import_CSV("", csvData, eSpreadsheetUpdateMode.AddNewTerms);
        }

        foreach (var i2CsvFile in info.GetFiles("*.i2csv", SearchOption.AllDirectories))
        {
            languageSourceData ??= new LanguageSourceData();
            var i2CsvData = File.ReadAllText(i2CsvFile.FullName).Replace("\r\n", "\n");
            languageSourceData.Import_I2CSV("", i2CsvData, eSpreadsheetUpdateMode.AddNewTerms);
        }

        if (languageSourceData != null)
        {
            languageSourceData.OnMissingTranslation = LanguageSourceData.MissingTranslationAction.Fallback;
            SpaceWarpPlugin.Instance.SWLogger.LogInfo($"Loaded localizations from {folder}");

            AddSource(languageSourceData);
        }
        else
        {
            SpaceWarpPlugin.Instance.SWLogger.LogInfo($"No localizations found in {folder}");
        }
    }

    private static void AddSource(LanguageSourceData source)
    {
        if (LocalizationManager.Sources.Contains(source))
        {
            return;
        }

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