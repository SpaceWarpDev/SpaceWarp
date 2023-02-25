using System;
using KSP.Game.Flow;
using SpaceWarp.API;

namespace SpaceWarp.Patching.LoadingActions
{
    public class ReadingModsAction : FlowAction
    {
        private SpaceWarpManager _manager;
        public ReadingModsAction(string name, SpaceWarpManager manager) : base(name)
        {
            _manager = manager;
        }

        protected override void DoAction(Action resolve, Action<string> reject)
        {
            try
            {
                _manager.ReadMods();
                resolve();
            }
            catch (Exception e)
            {
                reject(e.ToString());
            }
        }
    }
}