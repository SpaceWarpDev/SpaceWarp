using HarmonyLib;
using System.Reflection;
using UnityEngine.SceneManagement;
namespace SpaceWarp
{
    public class Entrypoint
    {
        private static bool _patched;
     
        private const string HARMONY_PACKAGE_URL = "com.github.celisium.spacewarp-doorstop";

        public static void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (_patched)
            {
                return;
            }
            
            Harmony harmony = new Harmony(HARMONY_PACKAGE_URL);

            MethodInfo original = typeof(KSP.Game.GameManager).GetMethod(nameof(KSP.Game.GameManager.StartGame));
            MethodInfo postfix = typeof(StartupManager).GetMethod(nameof(StartupManager.OnGameStarted));

            harmony.Patch(original, postfix: new HarmonyMethod(postfix));

            _patched = true;
        }
    }
}