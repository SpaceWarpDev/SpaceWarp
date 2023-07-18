using System;
using System.Collections.Generic;
using System.IO;
using KSP.Game;
using KSP.Game.Flow;
using KSP.Modding;
using Newtonsoft.Json;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.InternalUtilities;
using UnityEngine;

namespace SpaceWarp.Patching.LoadingActions;

internal class RegisterKSPLoaderModsAction : FlowAction
{
    public RegisterKSPLoaderModsAction() : base("Registering mods loaded from internal Mod Loader into Space Warp")
    {
    }

    private static ModInfo KSPToSwinfo(KSP2Mod mod)
    {
        var newInfo = new ModInfo
        {
            Spec = SpecVersion.V1_3,
            ModID = mod.ModName,
            Name = mod.ModName,
            Author = mod.ModAuthor,
            Description = mod.ModDescription,
            Source = "<unknown>",
            Version = mod.ModVersion.ToString(),
            Dependencies = new List<DependencyInfo>(),
            SupportedKsp2Versions = new SupportedVersionsInfo
            {
                Min = "*",
                Max = "*"
            },
            VersionCheck = null,
            VersionCheckType = VersionCheckType.SwInfo
        };
        return newInfo;
    }
    
    internal static SpaceWarpPluginDescriptor KspModToSW(KSP2Mod mod, Dictionary<string,Type> kspLoaderSpaceWarpModTypes)
    {
        var path = mod.ModRootPath;
        var key = new DirectoryInfo(mod.ModRootPath).Name.ToLower();
        ISpaceWarpMod swMod;
        if (kspLoaderSpaceWarpModTypes.TryGetValue(key, out var type))
        {
            var go = new GameObject(key);
            swMod = (ISpaceWarpMod)go.AddComponent(type);
            go.Persist();
        }
        else
        {
            swMod = new KspModAdapter(mod);
        }

        ModInfo info;
        if (File.Exists(Path.Combine(path, "swinfo.json")))
        {
            info = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(path));
        }
        else
        {
            info = KSPToSwinfo(mod);
        }

        return new SpaceWarpPluginDescriptor(swMod, info.ModID, info.Name, info, new DirectoryInfo(path));
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        SpaceWarpManager.InternalModLoaderMods = new List<SpaceWarpPluginDescriptor>();
        var modManager = GameManager.Instance.Game.KSP2ModManager;
        // Get a list of KSP2SpaceWarpMods as well
        Dictionary<string,Type> kspLoaderSpaceWarpModTypes = new();

        GetSpaceWarpModTypes(kspLoaderSpaceWarpModTypes);
        int idx = GameManager.Instance.LoadingFlow._flowIndex + 1;
        try
        {
            foreach (var mod in modManager.CurrentMods)
            {
                var sw = KspModToSW(mod, kspLoaderSpaceWarpModTypes);
                SpaceWarpManager.AllPlugins.Add(sw);
                SpaceWarpManager.InternalModLoaderMods.Add(sw);
                GameManager.Instance.LoadingFlow._flowActions.Insert(idx, new PreInitializeModAction(sw));
                idx += 1;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        resolve();
    }

    private static void GetSpaceWarpModTypes(Dictionary<string, Type> kspLoaderSpaceWarpModTypes)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic || assembly.Location == "") continue;
            var location = new FileInfo(assembly.Location).Directory;
            if (location!.Parent!.Name.ToLower() != "mods" ||
                location.Parent.Parent!.Name.ToLower() != "gamedata") continue;
            var key = location.Name.ToLower();
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(BaseKspLoaderSpaceWarpMod).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    kspLoaderSpaceWarpModTypes[key] = type;
                }
            }
        }
    }
}