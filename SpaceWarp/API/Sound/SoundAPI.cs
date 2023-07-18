using SpaceWarp.Backend.Sound;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceWarp.API.Sound;
internal class SoundAPI
{
    public static bool LoadBank(string internalPath, byte[] BankData, out Soundbank soundbank)
    {
        Soundbank bank = new Soundbank(BankData);
        AKRESULT result = bank.Load();

        if (result == AKRESULT.AK_Success)
        {
            bank.InternalPath = internalPath;
            Soundbank.soundbanks.Add(bank.InternalPath, bank);
            soundbank = bank;
            return true;
        }
        else
        {
            SpaceWarpManager.Logger.LogError($"Soundbank loading failed with result {result}");
            soundbank = null;
            return false;
        }
    }

    public static Soundbank GetSoundbankWithID(string internalPath)
    {
        return Soundbank.soundbanks[internalPath];
    }
}
