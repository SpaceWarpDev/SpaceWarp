using KSP.Game;
using SpaceWarp.API.Logging;
using SpaceWarp.API.Mods.JSON;
using UnityEngine;

namespace SpaceWarp.API.Mods;

/// <summary>
/// Represents a KSP2 Mod, you should inherit from this and do your manager processing.
/// </summary>
public abstract class Mod : KerbalMonoBehaviour
{
	public BaseModLogger Logger;
	public ModInfo Info;



	public virtual void Initialize()
	{
	}

	private void OnDestroy()
	{
		ModLocator.Remove(this);
	}

	public virtual void OnInitialized()
	{
		// Empty
	}

	public void Setup(Transform transformParent, ModInfo info)
	{
		transform.SetParent(transformParent);

		ModLogger newModLogger = new ModLogger(info.name);
		Logger = newModLogger;
			
		Info = info;
	}
}