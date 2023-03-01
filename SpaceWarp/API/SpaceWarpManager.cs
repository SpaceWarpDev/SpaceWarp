using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Newtonsoft.Json;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Logging;
using SpaceWarp.API.AssetBundles;
using SpaceWarp.API.Managers;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.API.Toolbar;
using SpaceWarp.API.Versions;
using SpaceWarp.Compilation;
using SpaceWarp.Patching;
using SpaceWarp.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SpaceWarp.API;

/// <summary>
/// Handles all the SpaceWarp initialization and mod processing.
/// </summary>
public class SpaceWarpManager : Manager 
{
    private BaseModLogger _modLogger;

        private const string MODS_FOLDER_NAME = "Mods";
        #if DOORSTOP_BUILD
        public static string SPACE_WARP_PATH = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName + "/";
        #else
        public static string SPACE_WARP_PATH = Path.Combine(Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.Parent.FullName,"SpaceWarp");
        #endif
        public static string MODS_FULL_PATH = Path.Combine(SPACE_WARP_PATH,MODS_FOLDER_NAME);

    public SpaceWarpGlobalConfiguration SpaceWarpConfiguration;

    private readonly List<Mod> _allModScripts = new List<Mod>();
    internal readonly List<(string, ModInfo)> _modLoadOrder = new List<(string, ModInfo)>();
    public readonly List<(string,ModInfo)> LoadedMods = new List<(string,ModInfo)>();
    private static readonly List<(string, ModInfo)> AllEnabledModInfo = new List<(string, ModInfo)>();
        
    public readonly List<(string,ModInfo)> IgnoredMods = new List<(string,ModInfo)>();

    public ModListUI ModListUI { get; private set; }
    protected override void Start()
    {
            
        Debug.Log($"Space Warp Path {SPACE_WARP_PATH}");
        Debug.Log($"Mods Path {MODS_FULL_PATH}");
            
        base.Start();

        Initialize();
    }

    /// <summary>
    /// Initializes the SpaceWarp manager.
    /// </summary>
    private void Initialize()
    {
        ToolbarBackend.AppBarInFlightSubscriber.AddListener(LoadAllButtons);
        InitializeConfigManager();
            
        InitializeModLogger();

        LoadingScreenPatcher.AddModLoadingScreens();
    }

    ///<summary>
    ///Initializes the configuration manager
    ///</summary>
    public void InitializeConfigManager()
    {
        GameObject confManagerObject = new GameObject("Configuration Manager");
        Persist(confManagerObject);

        confManagerObject.AddComponent<ConfigurationManager>();
        confManagerObject.SetActive(true);
    }
        
    /// <summary>
    /// Initializes the SpaceWarp mod logger.
    /// </summary>
    private void InitializeModLogger()
    {
        _modLogger = new ModLogger("Space Warp");
        _modLogger.Info("Warping Spacetime");
    }

    /// <summary>
    /// Read all the mods in the mods path
    /// </summary>
    internal void ReadMods()
    {
        _modLogger.Info("Reading mods");

        string[] modDirectories;
        try
        {
            modDirectories = Directory.GetDirectories(MODS_FULL_PATH);
        }
        catch(Exception exception)
        {
            _modLogger.Critical($"Unable to open mod path: {MODS_FULL_PATH}\nException:{exception}");
            return;
        }

        if (modDirectories.Length == 0)
        {
            _modLogger.Warn("No mods were found! No panic though.");
        }

        foreach (string modFolderuntrimmedU in modDirectories)
        {
            string modFolder = modFolderuntrimmedU.TrimEnd('/', '\\');

            string modName = Path.GetFileName(modFolder);
            if (!File.Exists(Path.Combine(modFolder,"modinfo.json")))
            {
                _modLogger.Warn($"Found mod {modName} without modinfo.json");
                continue;
            }

            if (File.Exists(Path.Combine(modFolder,".ignore")))
            {
                _modLogger.Info($"Skipping mod {modName} due to .ignore file");
                ModInfo ignore_info = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(modFolder + "\\modinfo.json"));
                string ignore_fileName = Path.GetFileName(modFolder);
                IgnoredMods.Add((ignore_fileName,ignore_info));
                continue;
            }
            _modLogger.Info($"Found mod: {modName}, adding to enable mods");

            ModInfo info = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(modFolder + "\\modinfo.json"));
            string fileName = Path.GetFileName(modFolder);
            AllEnabledModInfo.Add((fileName,info));
        }

        ResolveLoadOrder();
    }

    /// <summary>
    /// Checks if all dependencies are resolved.
    /// </summary>
    /// <param name="mod"></param>
    /// <returns></returns>
    private bool AreDependenciesResolved(ModInfo mod)
    {
        foreach (DependencyInfo dependency in mod.dependencies)
        {
            _modLogger.Info($"{mod.name} dependency - {dependency.id} {dependency.version.min}-{dependency.version.max}");

            string dependencyID = dependency.id;
            SupportedVersionsInfo dependencyVersion = dependency.version;

            bool found = false;
            foreach ((string, ModInfo) loadedMod in _modLoadOrder)
            {
                if (loadedMod.Item2.mod_id != dependencyID)
                {
                    continue;
                }

                string depLoadedVersion = loadedMod.Item2.version;

                if (!VersionUtility.IsVersionAbove(depLoadedVersion, dependencyVersion.min)) 
                    return false;
                if (!VersionUtility.IsVersionBellow(depLoadedVersion, dependencyVersion.max)) 
                    return false;
                        
                found = true;
            }

            if (!found) return false;
        }

        return true;
    }

    /// <summary>
    /// Resolves mod order
    /// </summary>
    private void ResolveLoadOrder()
    {
        //TODO: Make this way more optimized!
        _modLogger.Info("Resolving Load Order");
        bool changed = true;
        while (changed)
        {
            changed = false;
            List<int> toRemove = new List<int>();
            for (int i = 0; i < AllEnabledModInfo.Count; i++)
            {
                _modLogger.Info("Attempting to resolve dependencies for " + AllEnabledModInfo[i].Item1);
                if (AreDependenciesResolved(AllEnabledModInfo[i].Item2))
                {
                    _modLoadOrder.Add(AllEnabledModInfo[i]);
                    toRemove.Add(i);
                    changed = true;
                }
            }

            for (int i = toRemove.Count - 1; i >= 0; i--)
            {
                AllEnabledModInfo.RemoveAt(toRemove[i]);
            }
        }

        if (AllEnabledModInfo.Count > 0)
        {
            foreach ((string modName, ModInfo info) in AllEnabledModInfo)
            {
                _modLogger.Warn($"Skipping loading of {modName} as not all dependencies could be met");
            }
        }
        LoadingScreenPatcher.AddAllModLoadingSteps();
    }

    internal void InitializeAssets()
    {
        _modLogger.Info("Initializing mod assets");

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
                _modLogger.Info($"Found space warp asset file {file}");
                string assetBundleName = Path.GetFileNameWithoutExtension(file);
                if (Path.GetExtension(file) != ".bundle") continue;
                    

                AssetBundle assetBundle = AssetBundle.LoadFromFile(file);

                if (assetBundle == null)
                {
                    _modLogger.Error($"Failed to load AssetBundle space_warp/{assetBundleName}");
                    continue;
                }
                ResourceManager.RegisterAssetBundle("space_warp", assetBundleName, assetBundle);
                _modLogger.Info($"Loaded AssetBundle space_warp/{assetBundleName}");
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
                    _modLogger.Error($"Failed to load AssetBundle {info.mod_id}/{assetBundleName}");
                    continue;
                }
                ResourceManager.RegisterAssetBundle(info.mod_id, assetBundleName, assetBundle);
                _modLogger.Info($"Loaded AssetBundle {info.mod_id}/{assetBundleName}");
            }
        }
        else
        {
            _modLogger.Info($"Did not load assets for {modName} as no assets folder existed!");
        }

        // TODO: load part specific json stuff
    }

    /// <summary>
    /// Runs the mod initialization procedures.
    /// </summary>
    internal void InitializeMods()
    {
        _modLogger.Info("Initializing mods");
            

        foreach ((string modName, ModInfo info) in _modLoadOrder)
        {
            InitializeSingleMod(modName, info);
        }
    }

    /// <summary>
    /// Initializes a single mod
    /// </summary>
    /// <param name="modName">the name/id of the mod</param>
    /// <param name="info">the mod info structure that describes the mod</param>
    internal void InitializeSingleMod(string modName, ModInfo info)
    {
        string modFolder = Path.Combine(MODS_FULL_PATH, modName);

        _modLogger.Info($"Found mod: {modName}, attempting to load mod");

        // Now we load all assemblies under the code folder of the mod
        string codePath = Path.Combine(modFolder, GlobalModDefines.BINARIES_FOLDER);

        if (Directory.Exists(codePath))
        {
            if (!TryLoadMod(codePath, modName, info, out Type mainModType))
            {
                // error logging is done inside TryLoadMod
                return;
            }

            InitializeModObject(modName, info, mainModType);
        }
        else
        {
            _modLogger.Error($"Directory not found: {codePath}");
        }
    }

    /// <summary>
    /// Tries to load a mod at a path
    /// </summary>
    /// <param name="codePath">The full path to this mod binaries.</param>
    /// <param name="modName">The mod name</param>
    /// <param name="mainModType">The Mod type found</param>
    /// <returns>If the mod was successfully found.</returns>
    private bool TryLoadMod(string codePath, string modName, ModInfo modInfo, out Type mainModType)
    {
        string[] files;
        try
        {
            files = Directory.GetFiles(codePath);
        }
        catch
        {
            _modLogger.Error($"Could not load mod: {modName}, unable to read directory");
            mainModType = null;
            return false;
        }

        List<Assembly> modAssemblies = new List<Assembly>();
        foreach (string file in files)
        {
            // we only want to load dll files, ignore everything else
            if (!file.EndsWith(".dll"))
            {
                _modLogger.Warn($"Non-dll file found in \"{codePath}\" \"{file}\", Ignoring");
                continue;
            }

            Assembly asm;
            try
            {
                asm = Assembly.LoadFrom(file);
            }
            catch(Exception exeption)
            {
                _modLogger.Error($"Could not load mod: {modName}, Failed to load assembly {file}\nException: {exeption}");
                mainModType = null;

                return false;
            }

            modAssemblies.Add(asm);
        }
            
            
        string modFolder = Path.Combine(MODS_FULL_PATH,modName);
        string srcPath = Path.Combine(modFolder, "src");
        if (Directory.Exists(srcPath) && Directory.GetFiles(srcPath, "*",SearchOption.AllDirectories).Length > 0)
        {
            var result = ModCompiler.CompileMod(modInfo.mod_id, srcPath);
            if (result != null)
            {
                modAssemblies.Add(result);
            }
        }

        mainModType = null;
        foreach (Assembly asm in modAssemblies)
        {
            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch
            {
                _modLogger.Error($"Could not load mod: {modName}, Unable to get types out of assembly {asm.FullName}");

                mainModType = null;
                return false;
            }


            mainModType = types.FirstOrDefault(type => type.GetCustomAttribute<MainModAttribute>() != null);
            if (mainModType != null) break;
        }

        if (mainModType == null)
        {
            _modLogger.Error($"Could not load mod: {modName}, no type with [MainMod] exists");
            return false;
        }

            
        // We want to load the configuration for the mod as well
        Type configurationModType = null;
        foreach (Assembly asm in modAssemblies)
        {
            configurationModType = asm.GetTypes()
                .FirstOrDefault(type => type.GetCustomAttribute <ModConfigAttribute>() != null);
            if (configurationModType != null) break;
        }

        if (configurationModType != null)
        {
            InitializeModConfig(configurationModType, modName);
        }

        if (!typeof(Mod).IsAssignableFrom(mainModType))
        {
            _modLogger.Error($"Could not load mod: {modName}, the found class ({mainModType.FullName}) with [MainMod] doesn't inherit from {nameof(Mod)}");

            mainModType = null;
            return false;
        }

        // Harmony patch everything in the current mod!
        Harmony harmony = new Harmony($"com.mod.{modInfo.author}.{modInfo.mod_id}");
        foreach (Assembly asm in modAssemblies)
        {
            harmony.PatchAll(asm);
        }

        return true;

    }

    /// <summary>
    /// Tries to find a specific mods config file, if none is found, one is created
    /// </summary>
    private void InitializeModConfig(Type config_type, string mod_id)
    {
        object modConfiguration = null;
        var config_path = Path.Combine(MODS_FULL_PATH, mod_id, "config","config.json");
        if (!File.Exists(config_path))
        {
            modConfiguration = Activator.CreateInstance(config_type);
            foreach (var fieldInfo in config_type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var def = fieldInfo.GetCustomAttribute<ConfigDefaultValueAttribute>();
                if (def != null)
                {
                    fieldInfo.SetValue(modConfiguration, def.DefaultValue);
                }
            }
        }
        else
        {
            try
            {
                string json = File.ReadAllText(config_path);
                modConfiguration = JsonConvert.DeserializeObject(json,config_type);
                    
            }
            catch (Exception exception)
            {
                _modLogger.Error($"Loading mod config failed\nException: {exception}");

                File.Delete(config_path);
                InitializeModConfig(config_type, mod_id);
                return;
            }
        }

        try
        {
            File.WriteAllLines(config_path, new[] { JsonConvert.SerializeObject(modConfiguration) });
        }
        catch (Exception exception)
        {
            _modLogger.Error($"Saving mod config failed\nException: {exception}");
        }

        if (ManagerLocator.TryGet(out ConfigurationManager configurationManager))
        {
            configurationManager.Add(mod_id,(config_type,modConfiguration,config_path));
        }
    }

    /// <summary>
    /// Initializes a mod object.
    /// </summary>
    /// <param name="modName">The mod name to initialize.</param>
    /// <param name="mainModType">The mod type to initialize.</param>
    /// <param name="newModLogger">The new mod logger to spawn</param>
    private void InitializeModObject(string modName, ModInfo info, Type mainModType)
    {
        GameObject modObject = new GameObject($"Mod: {modName}");
        Mod modComponent = (Mod)modObject.AddComponent(mainModType);
            
        _allModScripts.Add(modComponent);
            
        modComponent.Setup(transform.parent, info);
        modObject.SetActive(true);

        _modLogger.Info($"Loaded: {modName}");

        // we probably dont want to completely stop loading mods if 1 mod throws an exception on Initialize
        try
        {
            ModLocator.Add(modComponent);
            Persist(modObject);
            modComponent.Initialize();
        }
        catch(Exception exception)
        {
            _modLogger.Critical($"Exception in {modName} Initialize(): {exception}");
        }

        LoadedMods.Add((modName, info));
    }

    /// <summary>
    /// Calls the OnInitialized method on each initialized mod.
    /// </summary>
    internal void InvokePostInitializeModsAfterAllModsLoaded()
    {
        foreach (Mod mod in _allModScripts)
        {
            try
            {
                mod.OnInitialized();
            }
            catch(Exception exception)
            {
                _modLogger.Critical($"Exception in {mod.name} AfterInitialization(): {exception}");
            }
        }
        InitModUI();
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
    public static void Persist(GameObject toPersist)
    {
        DontDestroyOnLoad(toPersist);
        toPersist.tag = "Game Manager";
    }

    private static GUISkin _skin = null;

    public static GUISkin Skin
    {
        get
        {
            if (_skin == null)
                ResourceManager.TryGetAsset("space_warp/swconsoleui/swconsoleUI/spacewarpConsole.guiskin", out
                    (_skin));
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
            ImageConversion.LoadImage(tex, fileContent);
        }

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    public void InitializeAddressablesFolder()
    {
        LoadSpaceWarpAddressables();
    }

    public IEnumerator LoadAddressable(string catalog)
    {
        _modLogger.Info($"Attempting to load {catalog}");
        AsyncOperationHandle<IResourceLocator> operation = Addressables.LoadContentCatalogAsync(catalog, null);
        yield return operation;
        if (operation.Status == AsyncOperationStatus.Failed)
        {
            _modLogger.Error($"Failed to load addressables catalog {catalog}");
        }
        else
        {
            _modLogger.Info($"Loaded addressables catalog {catalog}");
            var locator = operation.Result;
            _modLogger.Info($"{catalog} ----- {locator.LocatorId}");
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
        _modLogger.Info($"Loading addressables for {modID}");
        string catalogPath = Path.Combine(addressablesPath, "catalog.json");
        if (File.Exists(catalogPath))
        {
            _modLogger.Info($"Found addressables for {modID}");
            StartCoroutine(LoadAddressable(catalogPath));
        }
        else
        {
            _modLogger.Info($"Did not find addressables for {modID}");
        }
    }

    private void LoadLocalizationFromFolder(string folder)
    {
        _modLogger.Info($"Attempting to load localizations from {folder}");
        I2.Loc.LanguageSourceData languageSourceData = null;
        if (!Directory.Exists(folder))
        {
            _modLogger.Info($"{folder} does not exist, not loading localizations.");
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
            _modLogger.Info($"Loaded localizations from {folder}");
            I2.Loc.LocalizationManager.AddSource(languageSourceData);
        }
        else
        {
            _modLogger.Info($"No localizations found in {folder}");
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