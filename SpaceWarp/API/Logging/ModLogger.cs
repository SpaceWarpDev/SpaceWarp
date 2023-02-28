using System;
using System.Text;
using System.Threading.Tasks;

namespace SpaceWarp.API.Logging
{
    /// <summary>
    /// Unique logger for each mod, each mod has its own logger to accomodate different behaviours.
    /// </summary>
    public class ModLogger : BaseModLogger
    {
        private readonly string _moduleName;

        /// <summary>
        /// Creates a ModLogger for a module
        /// </summary>
        /// <param name="moduleName"></param>
        public ModLogger(string moduleName)
        {
            _moduleName = moduleName;
        }
        
        private string BuildLogMessage(LogLevel level, string message, object[] args)
        {
            StringBuilder sb = new StringBuilder();
            string formattedMessage = string.Format(message, args);

            sb.Append($"[{DateTime.Now:HH:mm:ss.fff}] ");
            sb.Append($"[{_moduleName}] ");
            sb.Append($"[{level}] ");
            sb.Append(formattedMessage);

            return sb.ToString();
        }

        protected override void Log(LogLevel level, string message, params object[] args)
        {
            if ((int)level >= SpaceWarpGlobalConfiguration.Instance.LogLevel)
            {
                string logMessage = BuildLogMessage(level, message, args);
                UnityEngine.Debug.Log(logMessage);
            }
        }
    }
}