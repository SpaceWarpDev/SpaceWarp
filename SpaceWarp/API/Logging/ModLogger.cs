using System.Text;

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

        private void InternalLog(LogLevel level, string message)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"[{_moduleName}] ");
            sb.Append($"[{level}] ");
            sb.Append(message);

            UnityEngine.Debug.Log(sb.ToString());
        }

        protected override void Log(LogLevel level, string message)
        {
            if ((int)level >= SpaceWarpGlobalConfiguration.Instance.LogLevel)
            {
                InternalLog(level,message);
            }
        }
    }
}