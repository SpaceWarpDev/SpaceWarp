using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using MonoMod.Cil;

namespace ChainloaderPatcher;

public static class ChainloaderPatch
{
    public static List<PluginInfo> DisabledPlugins = new();
    public static string DisabledPluginsFilepath = Path.Combine(Paths.PluginPath, "SpaceWarp", "disabled_plugins");

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(Chainloader), nameof(Chainloader.Start))]
    private static void PatchInitializationsIL(ILContext ilContext, ILLabel endLabel)
    {
        ILCursor ilCursor = new(ilContext);

        ilCursor.GotoNext(MoveType.Before, instruction => instruction.MatchStloc(3));
        ilCursor.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<List<PluginInfo>, List<PluginInfo>>>(
            static pluginInfos =>
            {
                var logger = Logger.CreateLogSource("ChainloadPatcher");
                var disabled = File.ReadAllLines(DisabledPluginsFilepath);
                logger.LogInfo($"Have {pluginInfos.Count} pluginInfos");
                logger.LogInfo($"Have {disabled.Length} disabled");
                foreach (var plugin in pluginInfos)
                {
                    if (disabled.Contains(plugin.Metadata.GUID))
                    {
                        DisabledPlugins.Add(plugin);
                        pluginInfos.Remove(plugin);
                        logger.LogInfo($"Disabling plugin {plugin.Metadata.GUID}");
                    }
                }

                return pluginInfos;
            });
    }
}