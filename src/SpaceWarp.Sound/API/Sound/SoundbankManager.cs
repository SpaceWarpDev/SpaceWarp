using JetBrains.Annotations;

namespace SpaceWarp.API.Sound;

/// <summary>
/// Manages Soundbanks.
/// </summary>
[PublicAPI]
public static class SoundbankManager
{
    /// <summary>
    /// Loads a Soundbank from bytes.
    /// </summary>
    /// <param name="internalPath">The internal path of the Soundbank.</param>
    /// <param name="bankData">The bytes of the Soundbank.</param>
    /// <param name="soundbank">The loaded Soundbank.</param>
    /// <returns>Whether the Soundbank was loaded successfully.</returns>
    public static bool LoadSoundbank(string internalPath, byte[] bankData, out Soundbank soundbank)
    {
        var bank = new Soundbank(bankData);
        var result = bank.Load();

        if (result != AKRESULT.AK_Success)
        {
            Modules.Sound.Instance.ModuleLogger.LogError($"Soundbank loading failed with result {result}");
            soundbank = null;
            return false;
        }

        bank.InternalPath = internalPath;
        LoadedSoundbanks.Add(bank.InternalPath, bank);
        soundbank = bank;
        return true;
    }

    /// <summary>
    /// Gets a Soundbank by its internal path.
    /// </summary>
    /// <param name="internalPath">The internal path of the Soundbank.</param>
    /// <returns>The Soundbank.</returns>
    public static Soundbank GetSoundbank(string internalPath)
    {
        return LoadedSoundbanks[internalPath];
    }

    /// <summary>
    /// Tries to get a Soundbank by its internal path.
    /// </summary>
    /// <param name="internalPath">The internal path of the Soundbank.</param>
    /// <param name="soundbank">The Soundbank.</param>
    /// <returns>Whether the Soundbank was found.</returns>
    public static bool TryGetSoundbank(string internalPath, out Soundbank soundbank)
    {
        return LoadedSoundbanks.TryGetValue(internalPath, out soundbank);
    }

    /// <summary>
    /// Lookup table that saves all loaded soundbanks. Key is the asset internal path.
    /// </summary>
    private static readonly Dictionary<string, Soundbank> LoadedSoundbanks = new();
}
