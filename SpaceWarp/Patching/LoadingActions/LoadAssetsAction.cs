using System;
using KSP.Game.Flow;
using SpaceWarp.API;
using SpaceWarp.API.Managers;

namespace SpaceWarp.Patching.LoadingActions
{
	public class LoadAssetsAction : FlowAction
	{
		public LoadAssetsAction(string name) : base(name)
		{
			//
		}

		protected override void DoAction(Action resolve, Action<string> reject)
		{
			ManagerLocator.TryGet(out SpaceWarpManager spaceWarpManager);

			try
			{
				spaceWarpManager.InitializeAssets();
				resolve();
			}
			catch (Exception e)
			{
				reject(e.ToString());
			}
		}
	}
}