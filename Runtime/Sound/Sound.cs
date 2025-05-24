using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using SpaceWarp.API.Loading;
using SpaceWarp.Modules;
using SpaceWarp.Sound.API;

namespace SpaceWarp.Sound;

/// <summary>
/// Module that handles sound.
/// </summary>
[UsedImplicitly]
public class Sound : SpaceWarpModule
{
    /// <inheritdoc />
    public override string Name => "SpaceWarp.Sound";

    internal static Sound Instance;

    private const string SOUNDBANKS_FOLDER = "soundbanks";

    /// <inheritdoc />
    public override void LoadModule()
    {
        Instance = this;
        Loading.AddDescriptorLoadingAction("loading soundbanks", descriptor =>
        {
            var soundsPath = Path.Combine(descriptor.Folder.FullName,"assets",SOUNDBANKS_FOLDER);
            var dirInfo = new DirectoryInfo(soundsPath);
            if (!dirInfo.Exists) return;
            foreach (var file in dirInfo.EnumerateFiles("*.bnk", SearchOption.AllDirectories))
            {
                var fullPath = file.FullName;
                var relativePath = Path.GetRelativePath(soundsPath, fullPath).Replace("\\", "/");
                var data = File.ReadAllBytes(fullPath);
                SoundbankManager.LoadSoundbank(descriptor.SWInfo.ModID, relativePath, data, out _);
            }
        });
    }
}