using BepInEx;
using BepInEx.Logging;
using KSP.Game;
using KSP.Messages;
using KSP.VFX;
using SpaceWarp.API.Mods.JSON;

namespace SpaceWarp.API.Mods;

/// <summary>
/// Represents a KSP2 Mod, you should inherit from this and do your manager processing.
/// </summary>
public abstract class BaseSpaceWarpPlugin : BaseUnityPlugin
{
	#region KspBehaviour things
	protected GameInstance Game => GameManager.Instance == null ? null : GameManager.Instance.Game;

	protected MessageCenter Messages => Game.Messages;

	// ReSharper disable Unity.NoNullPropagation
	// fine because its null checked by Game properly
	protected ContextualFxSystem CFXSystem => Game?.GraphicsManager?.ContextualFxSystem;

	protected bool IsGameShuttingDown => Game == null;
	#endregion
	
	public ModInfo SpaceWarpMetadata { get; internal set; }
	internal ManualLogSource ModLogger => base.Logger;
	public string PluginFolderPath { get; internal set; }

	public virtual void OnPreInitialized()
	{
	}

	public virtual void OnInitialized()
	{
	}

	public virtual void OnPostInitialized()
	{
	}
}
