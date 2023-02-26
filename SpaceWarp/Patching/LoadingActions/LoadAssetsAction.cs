using System;
using KSP.Game.Flow;
using SpaceWarp.API;

namespace SpaceWarp.Patching.LoadingActions
{
	public class LoadAssetsAction : FlowAction
	{
		private SpaceWarpManager _manager;
		public LoadAssetsAction(string name, SpaceWarpManager manager) : base(name)
		{
			_manager = manager;
		}

		protected override void DoAction(Action resolve, Action<string> reject)
		{
			try
			{
				_manager.InitializeAssets();
				resolve();
			}
			catch (Exception e)
			{
				reject(e.ToString());
			}
		}
	}
}