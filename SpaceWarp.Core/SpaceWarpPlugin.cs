global using UnityObject = UnityEngine.Object;
global using System.Linq;
using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using I2.Loc;
using JetBrains.Annotations;
using KSP.IO;
using KSP.ScriptInterop.impl.moonsharp;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using SpaceWarp.API.Loading;
using SpaceWarp.API.Lua;
using SpaceWarp.API.Mods;
using SpaceWarp.InternalUtilities;
using UitkForKsp2;
using SpaceWarp.Modules;
using SpaceWarp.Patching.LoadingActions;

namespace SpaceWarp;

[BepInDependency("com.bepis.bepinex.configurationmanager", "17.1")]
[BepInDependency(UitkForKsp2Plugin.ModGuid, UitkForKsp2Plugin.ModVer)]
[BepInIncompatibility("com.shadow.quantum")]
[BepInPlugin(ModGuid, ModName, ModVer)]
public sealed class SpaceWarpPlugin : BaseSpaceWarpPlugin
{
    public static SpaceWarpPlugin Instance;

    [PublicAPI] public const string ModGuid = "com.github.x606.spacewarp";
    [PublicAPI] public const string ModName = "Space Warp";
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION; // TODO: Don't hard code this, but I don't know much msbuild stuff so @munix wil have to do that,
                                                                          //       and @munix is really lazy to do it right now but he definitely will at some point :P
                                                      
    internal ScriptEnvironment GlobalLuaState;

    internal new static ManualLogSource Logger;

    public SpaceWarpPlugin()
    {
        // Load the type forwarders
        Assembly.LoadFile(
            $"{new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName}\\SpaceWarp.dll");
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
        BepInEx.Bootstrap.Chainloader.ManagerObject.Persist();
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
        // Persist all game objects so I don't need to stomp on config
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
