global using UnityObject = UnityEngine.Object;
global using System.Linq;
using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using I2.Loc;
using KSP.IO;
using KSP.ScriptInterop.impl.moonsharp;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using UitkForKsp2.API;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Loading;
using SpaceWarp.API.Lua;
using SpaceWarp.API.Mods;
using SpaceWarp.Backend.Modding;
using UitkForKsp2;
using UnityEngine.UIElements;
using SpaceWarp.Modules;
using SpaceWarp.Patching.LoadingActions;
using UnityEngine;

namespace SpaceWarp;

[BepInDependency(ConfigurationManager.ConfigurationManager.GUID, ConfigurationManager.ConfigurationManager.Version)]
[BepInDependency(UitkForKsp2Plugin.ModGuid, UitkForKsp2Plugin.ModVer)]
[BepInIncompatibility("com.shadow.quantum")]
[BepInPlugin(ModGuid, ModName, ModVer)]
public sealed class SpaceWarpPlugin : BaseSpaceWarpPlugin
{

    public static SpaceWarpPlugin Instance;


    public const string ModGuid = "com.github.x606.spacewarp";
    public const string ModName = "Space Warp";
    public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    internal ScriptEnvironment GlobalLuaState;
    
    internal new static ManualLogSource Logger;

    public SpaceWarpPlugin()
    {
        Logger = base.Logger;
        Instance = this;
    }

    private static void OnLanguageSourceAssetLoaded(LanguageSourceAsset asset)
    {
        if (!asset || LocalizationManager.Sources.Contains(asset.mSource))
        {
            return;
        }

        asset.mSource.owner = asset;
        LocalizationManager.AddSource(asset.mSource);
    }
    public void Awake()
    {
        IOProvider.Init();
        
        Harmony.CreateAndPatchAll(typeof(SpaceWarpPlugin).Assembly, ModGuid);
        ModuleManager.LoadAllModules();
        
        
        Loading.AddAssetLoadingAction("bundles", "loading asset bundles", FunctionalLoadingActions.AssetBundleLoadingAction, "bundle");
        Loading.AddAssetLoadingAction("images", "loading images", FunctionalLoadingActions.ImageLoadingAction);
        Loading.AddAddressablesLoadingAction<LanguageSourceAsset>("localization","language_source",OnLanguageSourceAssetLoaded);
    }

    private void SetupLuaState()
    {
        // I have been warned and I do not care
        UserData.RegistrationPolicy = InteropRegistrationPolicy.Automatic;

        GlobalLuaState = (ScriptEnvironment)Game.ScriptEnvironment;
        GlobalLuaState.script.Globals["SWLog"] = Logger;

        // Now we loop over every assembly and import static lua classes for methods
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            UserData.RegisterAssembly(assembly);
            foreach (var type in assembly.GetTypes())
            {
                // Now we can easily create API Objects from a [SpaceWarpLuaAPI] attribute
                foreach (var attr in type.GetCustomAttributes())
                {
                    if (attr is not SpaceWarpLuaAPIAttribute luaAPIAttribute) continue;
                    // As this seems to be used here
                    GlobalLuaState.script.Globals[luaAPIAttribute.LuaName] = UserData.CreateStatic(type);
                    break;
                }
            }
        }
    }

    public override void OnPreInitialized()
    {
        ModuleManager.PreInitializeAllModules();
    }


    public override void OnInitialized()
    {
        ModuleManager.InitializeAllModules();
        SetupLuaState();
    }

    public override void OnPostInitialized()
    {
        ModuleManager.PostInitializeAllModules();
    }
}