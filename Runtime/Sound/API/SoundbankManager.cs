using System.Collections.Generic;
using JetBrains.Annotations;

namespace SpaceWarp.Sound.API;

/// <summary>
/// Manages Soundbanks.
/// </summary>
[PublicAPI]
public static class SoundbankManager
{
    /// <summary>
    /// Loads a Soundbank from bytes.
    /// </summary>
    /// <param name="modId">The id of the mod that this sound bank is for</param>
    /// <param name="internalPath">The internal path of the Soundbank relative to the mods soundbanks folder (using / as a separator)</param>
    /// <param name="bankData">The bytes of the Soundbank.</param>
    /// <param name="soundbank">The loaded Soundbank.</param>
    /// <returns>Whether the Soundbank was loaded successfully.</returns>
    public static bool LoadSoundbank(string modId, string internalPath, byte[] bankData, out Soundbank soundbank)
    {
        var bank = new Soundbank(bankData);
        var result = bank.Load();

        if (result != AKRESULT.AK_Success)
        {
            Sound.Instance.ModuleLogger.LogError($"Soundbank loading failed with result {result}");
            soundbank = null;
            return false;
        }
        
        bank.ModId = modId;
        bank.InternalPath = internalPath;
        LoadedSoundbanks.Add((bank.ModId, bank.InternalPath), bank);
        soundbank = bank;
        return true;
    }

    /// <summary>
    /// Gets a Soundbank by its internal path.
    /// </summary>
    /// <param name="modId">The mod that the soundbank is stored in</param>
    /// <param name="internalPath">The internal path of the Soundbank relative to the mods soundbanks folder (using / as a separator)</param>
    /// <returns>The Soundbank.</returns>
    public static Soundbank GetSoundbank(string modId, string internalPath)
    {
        return LoadedSoundbanks[(modId, internalPath)];
    }

    /// <summary>
    /// Tries to get a Soundbank by its internal path.
    /// </summary>
    /// <param name="modId">The mod that the soundbank is stored in</param>
    /// <param name="internalPath">The internal path of the Soundbank relative to the mods soundbanks folder (using / as a separator)</param>
    /// <param name="soundbank">The Soundbank.</param>
    /// <returns>Whether the Soundbank was found.</returns>
    public static bool TryGetSoundbank(string modId, string internalPath, out Soundbank soundbank)
    {
        return LoadedSoundbanks.TryGetValue((modId,internalPath), out soundbank);
    }

    /// <summary>
    /// Lookup table that saves all loaded soundbanks. Key is the asset mod id and internal path.
    /// </summary>
    private static readonly Dictionary<(string modId, string internalPath), Soundbank> LoadedSoundbanks = new();
}
