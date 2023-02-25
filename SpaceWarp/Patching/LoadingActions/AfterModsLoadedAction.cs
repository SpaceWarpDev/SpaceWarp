using System;
using KSP.Game.Flow;
using SpaceWarp.API;

namespace SpaceWarp.Patching.LoadingActions
{
    public class AfterModsLoadedAction : FlowAction
    {
        private SpaceWarpManager _manager;
        public AfterModsLoadedAction(string name, SpaceWarpManager manager) : base(name)
        {
            _manager = manager;
        }

        protected override void DoAction(Action resolve, Action<string> reject)
        {
            try
            {
                _manager.AfterInitializationTasks();
                resolve();
            }
            catch (Exception e)
            {
                reject(e.ToString());
            }
        }
    }
}