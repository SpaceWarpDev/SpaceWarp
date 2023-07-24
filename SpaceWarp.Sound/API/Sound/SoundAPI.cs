using SpaceWarp.Backend.Sound;

namespace SpaceWarp.API.Sound;

internal class SoundAPI
{
    public static bool LoadBank(string internalPath, byte[] bankData, out Soundbank soundbank)
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
        Soundbank.LoadedSoundbanks.Add(bank.InternalPath, bank);
        soundbank = bank;
        return true;
    }

    public static Soundbank GetSoundbankWithID(string internalPath)
    {
        return Soundbank.LoadedSoundbanks[internalPath];
    }
}
