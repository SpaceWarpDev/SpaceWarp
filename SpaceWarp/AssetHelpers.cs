using System.IO;
using KSP.Game;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SpaceWarp;

internal static class AssetHelpers
{
    public static void LoadAddressable(string catalog)
    {
        SpaceWarpManager.Logger.LogInfo($"Attempting to load {catalog}");
        AsyncOperationHandle<IResourceLocator> operation = Addressables.LoadContentCatalogAsync(catalog, null);
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
        I2.Loc.LanguageSourceData languageSourceData = null;
        if (!Directory.Exists(folder))
        {
            SpaceWarpManager.Logger.LogInfo($"{folder} does not exist, not loading localizations.");
            return;
        }
        DirectoryInfo info = new DirectoryInfo(folder);
        foreach (var csvFile in info.GetFiles("*.csv"))
        {
            languageSourceData ??= new I2.Loc.LanguageSourceData();
            var csvData = File.ReadAllText(csvFile.FullName);
            languageSourceData.Import_CSV("", csvData, I2.Loc.eSpreadsheetUpdateMode.AddNewTerms);
        }

        foreach (var i2csvFile in info.GetFiles("*.i2csv"))
        {
            languageSourceData ??= new I2.Loc.LanguageSourceData();
            var i2csvData = File.ReadAllText(i2csvFile.FullName);
            languageSourceData.Import_I2CSV("", i2csvData, I2.Loc.eSpreadsheetUpdateMode.AddNewTerms);
        }

        if (languageSourceData != null)
        {
            SpaceWarpManager.Logger.LogInfo($"Loaded localizations from {folder}");
            I2.Loc.LocalizationManager.AddSource(languageSourceData);
        }
        else
        {
            SpaceWarpManager.Logger.LogInfo($"No localizations found in {folder}");
        }
    }
}
