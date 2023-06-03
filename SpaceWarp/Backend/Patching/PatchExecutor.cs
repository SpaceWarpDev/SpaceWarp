using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using BepInEx.Logging;
using KSP.Sim.Definitions;
using Moq;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Patching;

namespace SpaceWarp.Backend.Patching;

internal static class PatchExecutor
{
    internal static void ExecutePatch(ManualLogSource source, PartCore partCore, PatchBase patcher, MethodInfo patchMethod)
    {
        // So now we need to reflect the arguments and get all the data
        var parameters = patchMethod.GetParameters();
        List<object> arguments = new();
        foreach (var param in parameters)
        {
           var ty = param.ParameterType;
           if (ty.IsSubclassOf(typeof(ConfigEntryBase)))
           {
               // Try and get the config entry attribute
               var attr = param.GetCustomAttributes().OfType<ModConfigAttribute>().FirstOrDefault();
               if (attr != null)
               {
                   var descriptor = PluginList.TryGetDescriptor(attr.Guid);
                   if (descriptor != null && descriptor.Plugin != null)
                   {
                       try
                       {
                           var entry = descriptor.Plugin.Config[attr.Section, attr.Key];
                           if (param.ParameterType.IsInstanceOfType(entry))
                           {
                                arguments.Add(entry);
                           }
                       }
                       catch
                       {
                           source.LogError($"Attempting to execute patch method: {patchMethod.GetFullName()}, but did not find a config entry for the Config Entry argument: {param.Name}");
                       }
                   }
                   else
                   {
                       source.LogError($"Attempting to execute patch method: {patchMethod.GetFullName()}, but found a ConfigEntry argument w/ the associated mod not loaded/not being a C# mod]: {param.Name}");
                       return;
                   }
               }
               else
               {
                   // Throw an error here
                   source.LogError($"Attempting to execute patch method: {patchMethod.GetFullName()}, but found a ConfigEntry argument w/o an associated [ModConfig]: {param.Name}");
                   return;
               }
           }

           if (ty.IsSubclassOf(typeof(ModuleData)))
           {
               var attr = param.GetCustomAttributes().OfType<AddModuleAttribute>().FirstOrDefault();
               if (attr != null)
               {
                   // So now we create the part behaviour module
                   var module = mod
               }
               else
               {
                   
               }
           }
        }
    }
}