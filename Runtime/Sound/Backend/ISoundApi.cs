using System;

namespace SpaceWarp2.Sound.Backend;

public interface ISoundApi
{
    // AK_Success
    public const int Success = 1; 
 
    public static ISoundApi Instance { get; }

    public int LoadWWiseBankMemoryView(IntPtr ptr, uint length, out uint wwiseId, out string errorMessage);
}