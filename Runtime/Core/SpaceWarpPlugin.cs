using System.Collections.Generic;
using System.Reflection;
using ReduxLib.GameInterfaces;
using SpaceWarp.API;
using SpaceWarp.API.Backend.Modding;
using SpaceWarp.API.Loading;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.InternalUtilities;
using SpaceWarp.Modules;
using SpaceWarp.Patching.LoadingActions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using ILogger = ReduxLib.Logging.ILogger;

namespace SpaceWarp;

public sealed class SpaceWarpPlugin : GeneralMod
{
    /// <summary>
    /// SpaceWarp plugin instance.
    /// </summary>
    public static SpaceWarpPlugin Instance;

    internal static ILogger Logger;

    private static Assembly _pathsAssembly;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void LoadSpaceWarp()
    {
        ModList.Initialize();
        _pathsAssembly = SpaceWarpPathsGenerator.GenerateSpaceWarpPathsAssembly();
        ReduxLib.ReduxLib.OnReduxLibInitialized += CreateMonoBehaviours;
        
        Loading.AddAddressablesLoadingAction<TextAsset>("Loading addressables localizations (csv)", "loc_csv", OnCsvLoaded);
        Loading.AddAddressablesLoadingAction<TextAsset>("Loading addressables localizations (csv)", "loc_i2csv", OnI2CsvLoaded);
    }

    private static void OnCsvLoaded(TextAsset csv)
    {
        var text = csv.text.Replace("\r\n", "\n");
        ILocalizer.Instance.AddCsvSource(text);
    }

    private static void OnI2CsvLoaded(TextAsset i2Csv)
    {
        
        var text = i2Csv.text.Replace("\r\n", "\n");
        ILocalizer.Instance.AddI2CsvSource(text);
    }

    private static void CreateMonoBehaviours()
    {
        Logger = ReduxLib.ReduxLib.GetLogger("Space Warp");
        ModuleManager.LoadAllModules();
        PluginRegister.RegisterAllMods();
        PluginList.ResolveDependenciesAndLoadOrder();
        PluginList.LoadAllMods();
        // Now let's add in all our actions
        SetupBeforeGameLoadActions();
        SetupAfterGameLoadActions();
#if UNITY_EDITOR
        Addressables.InternalIdTransformFunc = AddressablesFixes.RedirectInternalIdsToGameDirectoryFixed;
#endif
    }

    private static void SetupBeforeGameLoadActions()
    {
        foreach (var plugin in PluginList.AllEnabledAndActivePlugins)
        {
            BeforeGameLoadActions.Add(new PreInitializeModAction(plugin));
        }
    }
    private static void SetupAfterGameLoadActions()
    {

        foreach (var plugin in PluginList.AllEnabledAndActivePlugins)
        {
            // Space Warps addressables are already loaded by redux at this point
            if (plugin.Guid != "spacewarp")
                AfterGameLoadActions.Add(new LoadAddressablesAction(plugin));
            
            AfterGameLoadActions.Add(new LoadLocalizationAction(plugin));
            
            foreach (var action in Loading.DescriptorLoadingActionGenerators)
            {
                AfterGameLoadActions.Add(action(plugin));
            }
        }
        
        foreach (var actionGenerator in Loading.GeneralLoadingActions)
        {
            AfterGameLoadActions.Add(actionGenerator());
        }
        
        foreach (var plugin in PluginList.AllEnabledAndActivePlugins)
        {
            AfterGameLoadActions.Add(new InitializeModAction(plugin));
        }

        foreach (var plugin in PluginList.AllEnabledAndActivePlugins)
        {
            AfterGameLoadActions.Add(new PostInitializeModAction(plugin));
        }
    }

    public SpaceWarpPlugin()
    {
        Instance = this;
    }



    internal static ModInfo SpaceWarpModInfo = new()
    {
        Spec = SpecVersion.V2_1,
        ModID = "com.github.x606.spacewarp",
        Name = "Space Warp",
        Author = "Space Warp Dev + Rendezvous Entertainment",
        Description = "A C# modding API for KSP2 Redux Modding",
        Source = "https://github.com/SpaceWarpDev/SpaceWarp",
        Version = "2.0.0",
        Dependencies = new List<DependencyInfo>(),
        SupportedKsp2Versions = new SupportedVersionsInfo
        {
            Min = "0.2.2.0.32914",
            Max = "*"
        },
        // We are actually going to have to make a swinfo.json at some point, but this is kinda built into redux
        VersionCheck = null,
        Conflicts = new List<DependencyInfo>(),
        Patchers = new List<string>(),
        MainAssembly = null
    };

    public override void OnPreInitialized()
    {
        ModuleManager.PreInitializeAllModules();
    }

    public override void OnInitialized()
    {
        ModuleManager.InitializeAllModules();
    }

    public override void OnPostInitialized()
    {
        ModuleManager.PostInitializeAllModules();
    }

    #region Space Warp Loading Actions

    public static List<IFlowAction> BeforeGameLoadActions = new();
    public static List<IFlowAction> AfterGameLoadActions = new();

    #endregion
}