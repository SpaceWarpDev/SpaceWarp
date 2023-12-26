using JetBrains.Annotations;

namespace SpaceWarp.API.Sound;

[PublicAPI]
public static class SoundbankManager
{
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

    public static Soundbank GetSoundbank(string internalPath)
    {
        return LoadedSoundbanks[internalPath];
    }

    public static bool TryGetSoundbank(string internalPath, out Soundbank soundbank)
    {
        return LoadedSoundbanks.TryGetValue(internalPath, out soundbank);
    }

    /// <summary>
    /// Lookup table that saves all loaded soundbanks. Key is the asset internal path.
    /// </summary>
    private static readonly Dictionary<string, Soundbank> LoadedSoundbanks = new();
}
