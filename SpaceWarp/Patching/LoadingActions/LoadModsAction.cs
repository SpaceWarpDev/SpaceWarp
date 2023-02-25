using System;
using KSP.Game.Flow;
using SpaceWarp.API;

namespace SpaceWarp.Patching.LoadingActions
{
    public class LoadModsAction : FlowAction
    {
        private SpaceWarpManager _manager;
        public LoadModsAction(string name, SpaceWarpManager manager) : base(name)
        {
            _manager = manager;
        }

        protected override void DoAction(Action resolve, Action<string> reject)
        {
            try
            {
                _manager.InitializeMods();
                resolve();
            }
            catch (Exception e)
            {
                reject(e.ToString());
            }
        }
    }
}