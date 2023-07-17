using System;
using System.IO;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class LoadLuaAction : FlowAction
{
    private readonly SpaceWarpPluginDescriptor _plugin;

    public LoadLuaAction(SpaceWarpPluginDescriptor plugin) : base(
        $"Running lua scripts for {plugin.SWInfo.Name}")
    {
        _plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            // Now we load the lua and run it for this mod
            var pluginDirectory = _plugin.Folder;
            foreach (var luaFile in pluginDirectory.GetFiles("*.lua", SearchOption.AllDirectories))
            {
                try
                {
                    // var compiled = GlobalLuaState.Compile(File.ReadAllText(luaFile.FullName), luaFile.FullName);
                    // GlobalLuaState.RunScriptAsset(compiled);
                    SpaceWarpPlugin.Instance.GlobalLuaState.script.DoString(File.ReadAllText(luaFile.FullName));
                }
                catch (Exception e)
                {
                    if (_plugin.Plugin != null)
                        _plugin.Plugin.SWLogger.LogError(e.ToString());
                    else
                        SpaceWarpPlugin.Logger.LogError(_plugin.SWInfo.Name + ": " + e); 
                }
            }
            resolve();
        }
        catch (Exception e)
        {
            if (_plugin.Plugin != null)
                _plugin.Plugin.SWLogger.LogError(e.ToString());
            else
                SpaceWarpPlugin.Logger.LogError(_plugin.SWInfo.Name + ": " + e);   
            reject(null);
        }
    }
}