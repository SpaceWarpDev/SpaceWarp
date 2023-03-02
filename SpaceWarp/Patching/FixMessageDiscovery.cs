using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using KSP.Game;
using KSP.Messages;
using KSP.Logging;

namespace SpaceWarp.Patching;

[HarmonyPatch]
internal static class FixMessageDiscovery
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MessageDiscovery), nameof(MessageDiscovery.Initialize))]
    private static bool InitializeReplacement()
    {
        if (MessageDiscovery._initialized)
        {
            return false;
        }
        Type[] types = typeof(GameManager).Assembly.GetTypes();
        foreach (Type type in types)
        {
            try
            {
                if (type.GetCustomAttribute(typeof(DiscoverableMessage), inherit: false) is DiscoverableMessage discoverableMessage)
                {
                    if (MessageDiscovery._discoveryNameTypeCache.ContainsKey(discoverableMessage.discoveryName))
                    {
                        GlobalLog.Error("More than one Discoverable Message is using the discoveryName of " + discoverableMessage.discoveryName + ". This is probably unintended!");
                    }
                    MessageDiscovery._discoveryNameTypeCache[discoverableMessage.discoveryName] = type;
                }
            }
            catch (Exception ex)
            {
                GlobalLog.Error("MessageDiscovery: Initialize: " + ex.Message);
            }
        }
        MessageDiscovery.DiscoveryNamesSorted = new string[MessageDiscovery._discoveryNameTypeCache.Count];
        int num = 0;
        foreach (KeyValuePair<string, Type> item in MessageDiscovery._discoveryNameTypeCache)
        {
            MessageDiscovery.DiscoveryNamesSorted[num] = item.Key;
            num++;
        }
        Array.Sort(MessageDiscovery.DiscoveryNamesSorted);
        MessageDiscovery._initialized = true;

        return false;
    }
}
