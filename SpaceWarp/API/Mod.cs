using SpaceWarp.API.Logging;
using UnityEngine;

namespace SpaceWarp.API
{
	public abstract class Mod : MonoBehaviour
	{
		public BaseModLogger Logger;
		public SpaceWarpManager Manager;

		public virtual void Initialize()
		{
			ModLocator.Add(this);
			DontDestroyOnLoad(gameObject);
		}
	}
}
