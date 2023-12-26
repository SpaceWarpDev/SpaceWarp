using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace SpaceWarp.API.Sound;

[PublicAPI]
public class Soundbank
{
    public Soundbank(byte[] bankData)
    {
        BankData = bankData;
        Size = (uint)bankData.Length;
    }

    /// <summary>
    /// Pointer to bank data
    /// </summary>
    public IntPtr? BankDataPtr;

    /// <summary>
    /// BankData supplied by the user
    /// </summary>
    public byte[] BankData;

    /// <summary>
    /// Handle for BankData array
    /// </summary>
    public GCHandle? Memory;

    /// <summary>
    /// Size of the bank in bytes
    /// </summary>
    public uint Size;

    /// <summary>
    /// Spacewarp's asset ID. Also used to get the Soundbank.
    /// </summary>
    public string InternalPath;

    /// <summary>
    /// Identifier for the engine
    /// </summary>
    public uint WwiseID;

    public AKRESULT Load()
    {
        // Pins BankData array in memory
        BankDataPtr ??= (Memory = GCHandle.Alloc(BankData, GCHandleType.Pinned)).Value.AddrOfPinnedObject();

        // Loads the entire array as a bank
        var result = AkSoundEngine.LoadBankMemoryView(
            BankDataPtr!.Value,
            (uint)BankData.Length,
            out WwiseID
        );

        if (result == AKRESULT.AK_Success)
        {
            // BankData is held by the GCHandle or was created from a raw pointer, no need for the array
            BankData = null;
        }

        return result;
    }
}