using AK.Wwise;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SpaceWarp.Backend.Sound;
internal class Soundbank
{
    internal Soundbank(byte[] bankData)
    {
        BankData = bankData;
        Size = (uint)bankData.Length;
    }

    /// <summary>
    /// Pointer to bank data
    /// </summary>
    internal IntPtr? BankDataPtr;

    /// <summary>
    /// BankData supplied by the user
    /// </summary>
    internal byte[] BankData;

    /// <summary>
    /// Handle for BankData array
    /// </summary>
    internal GCHandle? Memory;

    /// <summary>
    /// Size of the bank in bytes
    /// </summary>
    internal uint Size;

    /// <summary>
    /// Spacewarp's asset ID. Also used to get the Soundbank.
    /// </summary>
    internal string InternalPath;

    /// <summary>
    /// Identifier for the engine
    /// </summary>
    internal uint WwiseID;

    internal AKRESULT Load()
    {
        // Pins BankData array in memory
        BankDataPtr ??= (Memory = GCHandle.Alloc(BankData, GCHandleType.Pinned)).Value.AddrOfPinnedObject();

        // Loads the entire array as a bank
        var result = AkSoundEngine.LoadBankMemoryView(BankDataPtr!.Value, (uint)BankData.Length, out WwiseID);

        if (result == AKRESULT.AK_Success)
        {
            // BankData is held by the GCHandle or was created from a raw pointer, no need for the array
            BankData = null;
        }

        return result;
    }

    /// <summary>
    /// Lookup table that saves all loaded soundbanks. Key is the asset internal path.
    /// </summary>
    internal static Dictionary<string, Soundbank> soundbanks;
}