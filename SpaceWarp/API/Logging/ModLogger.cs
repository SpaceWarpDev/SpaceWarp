using System;
using System.Text;
using UnityEngine;

namespace SpaceWarp.API.Logging
{
    public class ModLogger : BaseModLogger
    {
        public string ModuleName;

        public ModLogger(string moduleName)
        {
            ModuleName = moduleName;
        }

        private void InternalLog(LogLevel level, string message)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"[{ModuleName}] ");
            sb.Append($"[{level}] ");
            sb.Append(message);
            UnityEngine.Debug.Log(sb.ToString());
        }
        
        public override void Log(LogLevel level, string message)
        {
            if ((int)level >= StartupManager.SpaceWarpObject.SpaceWarpConfiguration.LogLevel)
            {
                InternalLog(level,message);
                //
            }
        }
    }
}