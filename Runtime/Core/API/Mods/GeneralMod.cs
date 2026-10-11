using HarmonyLib;
using ReduxLib.Configuration;
using ReduxLib.Logging;
using SpaceWarp2.API.Loading;

namespace SpaceWarp2.API.Mods;

public abstract class GeneralMod : ISpaceWarpMod, ICampaignLifecycle
{
    public virtual void OnPreInitialized()
    {
    }

    public virtual void OnInitialized()
    {
    }

    public virtual void OnPostInitialized()
    {
    }

    public ILogger SWLogger { get; set; }
    public IConfigFile SWConfiguration { get; set; }
    public SpaceWarpPluginDescriptor SWMetadata { get; set; }

    /// <summary>
    /// Gets whether this mod's default feature is on in the active set.
    /// </summary>
    public bool Enabled => SWMetadata?.Enabled ?? true;

    /// <summary>
    /// Called once a campaign this mod is enabled in has loaded.
    /// </summary>
    public virtual void OnCampaignLoaded()
    {
    }

    /// <summary>
    /// Called before that campaign unloads, to undo what <see cref="OnCampaignLoaded" /> set up.
    /// </summary>
    public virtual void OnCampaignUnloading()
    {
    }

    public Harmony CreateHarmonyAndPatchAll()
    {
        var harmony = new Harmony(SWMetadata.Guid);
        harmony.PatchAll(GetType().Assembly);
        return harmony;
    }
}