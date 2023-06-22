using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using KSP.UI;
using SpaceWarp.API.UI.Settings;
using UnityEngine;

namespace SpaceWarp.UI.Settings;

internal class ModsSubMenu : SettingsSubMenu
{
    internal Func<string, GameObject> GenerateTitle;
    internal Func<GameObject> GenerateDivider;
    internal Func<string, GameObject> GenerateSectionHeader;

    public void Awake()
    {
        settingsConfigVariables = gameObject.AddComponent<SettingsConfigVariables>();
        // So now we have to set up every UI button with a revert and all that button
    }

    public void Start()
    {
        // Lets make sure we clear out the list
        foreach (Transform child in transform)
        {
            SpaceWarpPlugin.Logger.LogDebug($"Destroying ---- {child.gameObject.name}");
            Destroy(child.gameObject);
        }
        // Now here is where we go through every single mod
#pragma warning disable CS0618
        foreach (var mod in BepInEx.Bootstrap.Chainloader.Plugins.Where(mod => mod.Config.Count > 0 && mod is not ConfigurationManager.ConfigurationManager))
        {
            // This is where do a "Add Name" function
            GenerateTitle(mod.Info.Metadata.Name).transform.SetParent(transform);
            GenerateDivider().transform.SetParent(transform);
            Dictionary<string, List<ConfigEntryBase>> modConfigCategories = new();
            foreach (var config in mod.Config)
            {
                var section = config.Key.Section;
                var conf = config.Value;
                if (modConfigCategories.TryGetValue(section, out var list))
                {
                    list.Add(conf);
                }
                else
                {
                    modConfigCategories[section] = new List<ConfigEntryBase> { conf };
                }
            }

            foreach (var config in modConfigCategories)
            {
                var header = GenerateSectionHeader(config.Key);
                header.transform.SetParent(transform);
                foreach (var drawer in config.Value.Select(ModsPropertyDrawers.Drawer).Where(drawer => drawer != null))
                {
                    drawer.transform.SetParent(header.transform);
                }
                GenerateDivider().transform.SetParent(transform);
            }
        }
    }

    public override void OnShow()
    {
    }

    public override void Apply()
    {
    }

    public override void Revert()
    {
    }
}