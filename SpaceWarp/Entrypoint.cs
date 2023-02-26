using BepInEx;
using HarmonyLib;
using KSP.Logging;
using SpaceWarp.UI;
using System.Reflection;

namespace SpaceWarp
{
    static class Package
    {
        public const string URL = "com.github.x606.spacewarp";
        public const string NAME = "SpaceWarp";
        public const string VERSION = "0.1.1";
    }

    [BepInPlugin(Package.URL, Package.NAME, Package.VERSION)]
    public class SpaceWarpEntrypoint : BaseUnityPlugin
    {
        private void Awake()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            KspLogManager.AddLogCallback(SpaceWarpConsoleLogListener.LogCallback);

            Logger.LogInfo($"Plugin {Package.NAME} is loaded!");
        }
    }
}