using System.IO;
using I2.Loc;
using KSP.Game;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SpaceWarp.InternalUtilities;

internal static class AssetHelpers
{
    public static void LoadAddressable(string catalog)
    {
        SpaceWarpManager.Logger.LogInfo($"Attempting to load {catalog}");
        var operation = Addressables.LoadContentCatalogAsync(catalog);
        operation.WaitForCompletion();
        if (operation.Status == AsyncOperationStatus.Failed)
        {
            SpaceWarpManager.Logger.LogError($"Failed to load addressables catalog {catalog}");
        }
        else
        {
            SpaceWarpManager.Logger.LogInfo($"Loaded addressables catalog {catalog}");
            var locator = operation.Result;
            SpaceWarpManager.Logger.LogInfo($"{catalog} ----- {locator.LocatorId}");
            GameManager.Instance.Game.Assets.RegisterResourceLocator(locator);
        }
    }

    internal static void LoadLocalizationFromFolder(string folder)
    {
        SpaceWarpManager.Logger.LogInfo($"Attempting to load localizations from {folder}");
        LanguageSourceData languageSourceData = null;
        if (!Directory.Exists(folder))
        {
            SpaceWarpManager.Logger.LogInfo($"{folder} does not exist, not loading localizations.");
            return;
        }

        var info = new DirectoryInfo(folder);
        foreach (var csvFile in info.GetFiles("*.csv"))
        {
            languageSourceData ??= new LanguageSourceData();
            var csvData = File.ReadAllText(csvFile.FullName);
            languageSourceData.Import_CSV("", csvData, eSpreadsheetUpdateMode.AddNewTerms);
        }

        foreach (var i2CsvFile in info.GetFiles("*.i2csv"))
        {
            languageSourceData ??= new LanguageSourceData();
            var i2CsvData = File.ReadAllText(i2CsvFile.FullName);
            languageSourceData.Import_I2CSV("", i2CsvData, eSpreadsheetUpdateMode.AddNewTerms);
        }
        

        if (languageSourceData != null)
        {
            languageSourceData.OnMissingTranslation = LanguageSourceData.MissingTranslationAction.Fallback;
            SpaceWarpManager.Logger.LogInfo($"Loaded localizations from {folder}");
            LocalizationManager.AddSource(languageSourceData);
        }
        else
        {
            SpaceWarpManager.Logger.LogInfo($"No localizations found in {folder}");
        }
    }
}
