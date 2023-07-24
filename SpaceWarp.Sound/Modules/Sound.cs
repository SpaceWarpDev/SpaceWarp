using System;
using System.Collections.Generic;
using System.IO;
using SpaceWarp.API.Loading;
using SpaceWarp.API.Sound;
using SpaceWarp.Backend.Sound;
using UnityObject = UnityEngine.Object;

namespace SpaceWarp.Modules;

public class Sound : SpaceWarpModule
{
    public override string Name => "SpaceWarp.Sound";
    internal static Sound Instance;
    public override void LoadModule()
    {
        Instance = this;
        Loading.AddAssetLoadingAction("soundbanks", "loading soundbanks", AssetSoundbankLoadingAction, "bnk");
    }

    public override void PreInitializeModule()
    {
    }

    public override void InitializeModule()
    {
    }

    public override void PostInitializeModule()
    {
    }
    internal static List<(string name, UnityObject asset)> AssetSoundbankLoadingAction(string internalPath, string filename)
    {
        var fileData = File.ReadAllBytes(filename);
        //Since theres no UnityObject that relates to soundbanks it passes null, saving only the internalpath
        List<(string name, UnityObject asset)> assets = new() { ($"soundbanks/{internalPath}", null) };

        //Banks are identified under Bank.soundbanks with their internal path
        if (SoundAPI.LoadBank($"soundbanks/{internalPath}", fileData, out var bank))
        {
            return assets;
        }
        throw new Exception(
            $"Failed to load soundbank {internalPath}");
    }
}