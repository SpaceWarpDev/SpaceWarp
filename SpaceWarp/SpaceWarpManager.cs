using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using UnityEngine;
using SpaceWarp.API.AssetBundles;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.API.Toolbar;
using SpaceWarp.Patching;
using SpaceWarp.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SpaceWarp;

/// <summary>
/// Handles all the SpaceWarp initialization and mod processing.
/// </summary>
internal static class SpaceWarpManager
{
    private static ManualLogSource _logger;
    
    internal static BaseSpaceWarpPlugin[] SpaceWarpPlugins;
    
    internal static void GetSpaceWarpPlugins()
    {
        SpaceWarpPlugins = Chainloader.PluginInfos.Values.Select(p => p.Instance).OfType<BaseSpaceWarpPlugin>().ToArray();
    }

    public static ModListUI ModListUI { get; private set; }

    public static void Initialize()
    {
        ToolbarBackend.AppBarInFlightSubscriber.AddListener(LoadAllButtons);

        _logger = BepInEx.Logging.Logger.CreateLogSource("SpaceWarp Manager");

        LoadingScreenPatcher.AddModLoadingScreens();
    }

    internal void InitializeAssets()
    {
        _logger.LogWarning("Initializing mod assets");

        foreach ((string modName, ModInfo info) in _modLoadOrder)
        {
            LoadSingleModAssets(modName, info);
        }

    }


    internal void LoadSpaceWarpAssets()
    {
        string bundlesPath = Path.Combine(SPACE_WARP_PATH, GlobalModDefines.ASSET_BUNDLES_FOLDER);
        
        if (Directory.Exists(bundlesPath))
        {
            foreach (string file in Directory.GetFiles(bundlesPath))
            {
                _logger.LogInfo($"Found space warp asset file {file}");
                string assetBundleName = Path.GetFileNameWithoutExtension(file);
                if (Path.GetExtension(file) != ".bundle") continue;
                    

                AssetBundle assetBundle = AssetBundle.LoadFromFile(file);

                if (assetBundle == null)
                {
                    _logger.LogError($"Failed to load AssetBundle space_warp/{assetBundleName}");
                    continue;
                }
                AssetManager.RegisterAssetBundle("space_warp", assetBundleName, assetBundle);
                _logger.LogInfo($"Loaded AssetBundle space_warp/{assetBundleName}");
            }
        }
    }
        
    /// <summary>
    /// Loads a single mods assets
    /// </summary>
    /// <param name="modName">the name/id of the mod</param>
    /// <param name="info">the mod info structure that describes the mod</param>
    internal void LoadSingleModAssets(string modName, ModInfo info)
    {
        string modFolder = Path.Combine(MODS_FULL_PATH, modName);

        // Now we load all asset bundles under the asset/bundles folder of the mod
        string bundlesPath = modFolder + GlobalModDefines.ASSET_BUNDLES_FOLDER;
        if (Directory.Exists(bundlesPath))
        {
            foreach (string file in Directory.GetFiles(bundlesPath))
            {
                string assetBundleName = Path.GetFileNameWithoutExtension(file);
                if (Path.GetExtension(file) != ".bundle") continue;
                    

                AssetBundle assetBundle = AssetBundle.LoadFromFile(file);

                if (assetBundle == null)
                {
                    _logger.LogError($"Failed to load AssetBundle {info.mod_id}/{assetBundleName}");
                    continue;
                }
                AssetManager.RegisterAssetBundle(info.mod_id, assetBundleName, assetBundle);
                _logger.LogInfo($"Loaded AssetBundle {info.mod_id}/{assetBundleName}");
            }
        }
        else
        {
            _logger.LogInfo($"Did not load assets for {modName} as no assets folder existed!");
        }

        // TODO: load part specific json stuff
    }

    /// <summary>
    /// Initializes the UI for the mod list and configuration menu
    /// </summary>
    private void InitModUI()
    {
        GameObject modUIObject = new GameObject("Space Warp Mod UI");
        Persist(modUIObject);

        modUIObject.transform.SetParent(transform.parent);
        ModListUI = modUIObject.AddComponent<ModListUI>();

        modUIObject.SetActive(true);

        GameObject consoleUIObject = new GameObject("Space Warp Console");
        Persist(consoleUIObject);
        consoleUIObject.transform.SetParent(transform.parent);
        SpaceWarpConsole con = consoleUIObject.AddComponent<SpaceWarpConsole>();
        consoleUIObject.SetActive(true);
    }

    private static List<(string text, Sprite icon, string ID, Action<bool> action)> _buttonsToBeLoaded =
        new List<(string text, Sprite icon, string ID, Action<bool> action)>();

    public T RegisterGameToolbarMenu<T>(string text, string title, string id, Sprite icon) where T : ToolbarMenu
    {
        GameObject toolBarUIObject = new GameObject($"Toolbar: {id}");
        Persist(toolBarUIObject);
        ToolbarMenu menu = toolBarUIObject.AddComponent<T>();
        menu.Title = title;
        toolBarUIObject.transform.SetParent(transform.parent);
        toolBarUIObject.SetActive(true);
        _buttonsToBeLoaded.Add((text,icon,id,menu.ToggleGUI)); 
        return menu as T;
    }

    public static void RegisterAppButton(string text, string id, Sprite icon, Action<bool> func) =>
        _buttonsToBeLoaded.Add((text ,icon, id, func)); 

    /// <summary>
    /// Allows an object to persist through KSP 2s destruction
    /// </summary>
    /// <param name="toPersist">Object that should be persisted</param>
    public static void Persist(UnityObject toPersist)
    {
        UnityObject.DontDestroyOnLoad(toPersist);
        toPersist.hideFlags |= HideFlags.HideAndDontSave;
    }

    private static GUISkin _skin = null;

    public static GUISkin Skin
    {
        get
        {
            if (_skin)
                AssetManager.TryGetAsset("space_warp/swconsoleui/spacewarpConsole.guiskin", out _skin);
            return _skin;
        }
    }
        
    private void LoadAllButtons()
    {
        foreach (var button in _buttonsToBeLoaded)
        {
            ToolbarBackend.AddButton(button.text, button.icon, button.ID, button.action);
        }
    }

    /// <summary>
    /// Loads a png called 'icon.png' as a sprite from the same folder as the calling dll.
    /// In our case this should be SpaceWarp\Mods\[mod]\bin\icon.png
    /// </summary>
    /// <param name="size">The size of the png. The appbar expects 24x24.</param>
    public static Sprite LoadIcon(int size = 24)
    {
        string folderPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);

        return LoadIcon(Path.Combine(folderPath, "icon.png"), size);
    }

    /// <summary>
    /// Loads a png at the given path as a sprite.
    /// </summary>
    /// <param name="path">Path to the png.</param>
    /// <param name="size">The size of the png. The appbar expects 24x24.</param>
    public static Sprite LoadIcon(string path, int size = 24)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Point;

        if (File.Exists(path))
        {
            byte[] fileContent = File.ReadAllBytes(path);
            tex.LoadImage(fileContent);
        }

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    public void InitializeAddressablesFolder()
    {
        LoadSpaceWarpAddressables();
    }

    public IEnumerator LoadAddressable(string catalog)
    {
        _logger.LogInfo($"Attempting to load {catalog}");
        AsyncOperationHandle<IResourceLocator> operation = Addressables.LoadContentCatalogAsync(catalog, null);
        yield return operation;
        if (operation.Status == AsyncOperationStatus.Failed)
        {
            _logger.LogError($"Failed to load addressables catalog {catalog}");
        }
        else
        {
            _logger.LogInfo($"Loaded addressables catalog {catalog}");
            var locator = operation.Result;
            _logger.LogInfo($"{catalog} ----- {locator.LocatorId}");
            Game.Assets.RegisterResourceLocator(locator);
        }
    }

    public void LoadSpaceWarpAddressables()
    {
        string addressablesPath = Path.Combine(SPACE_WARP_PATH,"addressables");
        string catalogPath = Path.Combine(addressablesPath,"catalog.json");
        if (File.Exists(catalogPath))
        {
            StartCoroutine(LoadAddressable(catalogPath));
        }
    }
    
    public void LoadSingleModAddressables(string modID, ModInfo info)
    {
        string modFolder = Path.Combine(MODS_FULL_PATH, modID);
        string addressablesPath = Path.Combine(modFolder,"addressables");
        _logger.LogInfo($"Loading addressables for {modID}");
        string catalogPath = Path.Combine(addressablesPath, "catalog.json");
        if (File.Exists(catalogPath))
        {
            _logger.LogInfo($"Found addressables for {modID}");
            StartCoroutine(LoadAddressable(catalogPath));
        }
        else
        {
            _logger.LogInfo($"Did not find addressables for {modID}");
        }
    }

    private void LoadLocalizationFromFolder(string folder)
    {
        _logger.LogInfo($"Attempting to load localizations from {folder}");
        I2.Loc.LanguageSourceData languageSourceData = null;
        if (!Directory.Exists(folder))
        {
            _logger.LogInfo($"{folder} does not exist, not loading localizations.");
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
            _logger.LogInfo($"Loaded localizations from {folder}");
            I2.Loc.LocalizationManager.AddSource(languageSourceData);
        }
        else
        {
            _logger.LogInfo($"No localizations found in {folder}");
        }
    }
    public void LoadSpaceWarpLocalizations()
    {
        if (I2.Loc.LocalizationManager.Sources.Count == 0)
        {
            I2.Loc.LocalizationManager.UpdateSources();
        }

        string localizationsPath = Path.Combine(SPACE_WARP_PATH, "localizations");
        LoadLocalizationFromFolder(localizationsPath);
    }

    public void LoadSingleModLocalization(string modID, ModInfo info)
    {
        string modFolder = Path.Combine(MODS_FULL_PATH, modID);
        string localizationsPath = Path.Combine(modFolder, "localizations");
        LoadLocalizationFromFolder(localizationsPath);
    }
}