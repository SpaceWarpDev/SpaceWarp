using System;
using JetBrains.Annotations;
using KSP.Modding;
using SpaceWarp.API.Logging;
using SpaceWarp.API.Mods;
using UnityEngine;
using ILogger = SpaceWarp.API.Logging.ILogger;

namespace SpaceWarp.Backend.Modding;

internal class KspModAdapter : MonoBehaviour, ISpaceWarpMod
{
    public KSP2Mod AdaptedMod;

    public void OnPreInitialized()
    {
        AdaptedMod.modCore?.ModStart();
    }

    public void OnInitialized()
    {
    }

    public void OnPostInitialized()
    {
    }

    public void Update()
    {
        AdaptedMod.modCore?.ModUpdate();
    }

    public ILogger SWLogger { get; }
}