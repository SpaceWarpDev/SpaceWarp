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
        if (!Directory.Exists(folder))
        {
            SpaceWarpPlugin.Instance.SWLogger.LogInfo($"{folder} does not exist, not loading localizations.");
            return;
        }

        int loadedCount = 0;

        var info = new DirectoryInfo(folder);
        foreach (var csvFile in info.GetFiles("*.csv", SearchOption.AllDirectories))
        {
            var csvSource = new LanguageSourceData();
            var csvData = File.ReadAllText(csvFile.FullName).Replace("\r\n", "\n");
            csvSource.Import_CSV("", csvData, eSpreadsheetUpdateMode.AddNewTerms);
            loadedCount++;
            AddSource(csvSource);
        }

        foreach (var i2CsvFile in info.GetFiles("*.i2csv", SearchOption.AllDirectories))
        {
            var i2CsvSource = new LanguageSourceData();
            var i2CsvData = File.ReadAllText(i2CsvFile.FullName).Replace("\r\n", "\n");
            i2CsvSource.Import_I2CSV("", i2CsvData, eSpreadsheetUpdateMode.AddNewTerms);
            loadedCount++;
            AddSource(i2CsvSource);
        }

        SpaceWarpPlugin.Instance.SWLogger.LogInfo(
            loadedCount > 0 ? $"Loaded localizations from {folder}" : $"No localizations found in {folder}"
        );
    }

    private static void AddSource(LanguageSourceData source)
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