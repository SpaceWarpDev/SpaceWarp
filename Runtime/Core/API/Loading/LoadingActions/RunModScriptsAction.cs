using System;
using System.Collections.Generic;
using ReduxLib.GameInterfaces;
using SpaceWarp2.API.Lifecycle;
using SpaceWarp2.API.Mods;
using UnityEngine;

namespace SpaceWarp2.Patching.LoadingActions;

/// <summary>
/// Runs a mod's Lua scripts as part of its load sequence.
/// </summary>
/// <remarks>
/// Runs the declared script files plus the text assets of the addressable script label when one is present.
/// Forks the mod's environment through the <see cref="ModScriptRuntime" />.
/// </remarks>
internal sealed class RunModScriptsAction : BaseFlowAction
{
    private readonly SpaceWarpPluginDescriptor _plugin;

    public RunModScriptsAction(SpaceWarpPluginDescriptor plugin)
        : base($"Running mod scripts for {plugin.Name}", $"Running scripts for {plugin.Name}")
    {
        _plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            ModScriptRuntime.RunMod(_plugin);

            if (
                string.IsNullOrEmpty(_plugin.AddressableScriptLabel)
                || !IAssetProvider.Instance.DoesLabelExist(
                    _plugin.AddressableScriptLabel
                )
            )
            {
                resolve();
                return;
            }

            var sources = new List<(string, string)>();
            IAssetProvider.Instance.LoadByLabel<TextAsset>(
                _plugin.AddressableScriptLabel,
                asset => sources.Add(($"{_plugin.Guid}/{asset.name}", asset.text)),
                _ =>
                {
                    ModScriptRuntime.RunModSources(_plugin, sources);
                    resolve();
                });
        }
        catch (Exception e)
        {
            _plugin.Logger.LogError(e.ToString());
            reject(e.ToString());
        }
    }
}
