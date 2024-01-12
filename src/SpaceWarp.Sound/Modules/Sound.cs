using JetBrains.Annotations;
using SpaceWarp.API.Loading;
using SpaceWarp.API.Sound;
using UnityObject = UnityEngine.Object;

namespace SpaceWarp.Modules;

/// <summary>
/// Module that handles sound.
/// </summary>
[UsedImplicitly]
public class Sound : SpaceWarpModule
{
    /// <inheritdoc />
    public override string Name => "SpaceWarp.Sound";

    internal static Sound Instance;

    private const string SoundbanksFolder = "soundbanks";

    /// <inheritdoc />
    public override void LoadModule()
    {
        Instance = this;
        Loading.AddAssetLoadingAction(SoundbanksFolder, "loading soundbanks", AssetSoundbankLoadingAction, "bnk");
    }

    private static List<(string name, UnityObject asset)> AssetSoundbankLoadingAction(
        string internalPath,
        string filename
    )
    {
        var fileData = File.ReadAllBytes(filename);
        var fullPath = Path.Combine(SoundbanksFolder, internalPath);

        // Banks are identified by their internal path
        if (SoundbankManager.LoadSoundbank(fullPath, fileData, out _))
        {
            // Since there's no UnityObject related to soundbanks, we pass null, saving only the internalPath
            return new List<(string name, UnityObject asset)> { (fullPath, null) };
        }

        throw new Exception($"Failed to load soundbank {internalPath}");
    }
}