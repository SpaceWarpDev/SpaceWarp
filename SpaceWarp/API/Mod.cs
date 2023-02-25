using KSP.Game;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Logging;
using UnityEngine;

namespace SpaceWarp.API
{
	public abstract class Mod : KerbalMonoBehaviour
	{
		public BaseModLogger Logger;
		public SpaceWarpManager Manager;
		public ModInfo Info;

		public virtual void Initialize()
		{
			ModLocator.Add(this);
			DontDestroyOnLoad(gameObject);
		}

		public virtual void AfterInitialization()
		{
			// Empty
		}
	}
}
