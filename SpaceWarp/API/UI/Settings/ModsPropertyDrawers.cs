using System;
using System.Collections.Generic;
using System.ComponentModel;
using BepInEx.Configuration;
using I2.Loc;
using KSP;
using KSP.UI;
using KSP.UI.Binding;
using SpaceWarp.Backend.UI.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceWarp.API.UI.Settings;

public static class ModsPropertyDrawers
{
    private static Dictionary<Type, Func<ConfigEntryBase, GameObject>> AllPropertyDrawers = new();

    public static void AddDrawer<T>(Func<ConfigEntryBase, GameObject> drawerGenerator) =>
        AllPropertyDrawers.Add(typeof(T), drawerGenerator);

    public static GameObject Drawer(ConfigEntryBase entry)
    {
        if (entry.SettingType.IsEnum && !AllPropertyDrawers.ContainsKey(entry.SettingType))
            AllPropertyDrawers.Add(entry.SettingType, GenerateEnumDrawerFor(entry.SettingType));
        return AllPropertyDrawers.TryGetValue(entry.SettingType, out var func) ? func(entry) : null;
    }

    internal static GameObject DropdownPrefab;
    internal static GameObject RadioPrefab;
    internal static GameObject RadioSettingPrefab;

    // We internally just generate a localization key for the description
    private static int nextLocKey = 0;

    private static string GetNextLocalizationKey()
    {
        return $"Menu/Settings/Descriptions/ModOption{nextLocKey++}";
    }

    private static string GenerateLocalizationKeyFor(string description)
    {
        int numLanguages = LocalizationManager.Sources.First().mLanguages.Count;
        var termData = new TermData
        {
            TermType = eTermType.Text,
            Term = GetNextLocalizationKey(),
            Description = "",
            Languages = Enumerable.Repeat(description, numLanguages).ToArray()
        };
        LocalizationManager.Sources.First().mTerms.Add(termData);
        return termData.Term;
    }

    private static Func<ConfigEntryBase, GameObject> GenerateEnumDrawerFor(Type t)
    {
        var optionNames = t.GetEnumNames().ToList();
        var optionValues = t.GetEnumValues().Cast<int>().ToList();
        for (var i = 0; i < optionNames.Count; i++)
        {
            try
            {
                var memberInfos = t.GetMember(optionNames[i]);
                var enumValueInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == t);
                var valueAttributes = enumValueInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                optionNames[i] = ((DescriptionAttribute)valueAttributes[0]).Description;
            }
            catch
            {
                // ignored
            }
        }

        var shouldDropdown = optionNames.Count > 5 || optionNames.Select(x => x.Length).Sum() >= 50;
        return entry =>
        {
            if (shouldDropdown)
            {
                var ddCopy = UnityObject.Instantiate(DropdownPrefab);
                var lab = ddCopy.GetChild("Label");
                lab.GetComponent<Localize>().SetTerm(entry.Definition.Key);
                lab.GetComponent<TextMeshProUGUI>().text = entry.Definition.Key;
                var dropdown = ddCopy.GetChild("Setting").GetChild("KSP2DropDown");
                var extended = dropdown.GetComponent<DropdownExtended>();
                // Start by clearing the options data
                extended.options.Clear();
                foreach (var option in optionNames)
                {
                    extended.options.Add(new TMP_Dropdown.OptionData(option));
                }

                extended.value = optionValues.IndexOf((int)entry.BoxedValue);
                extended.onValueChanged.AddListener(idx => { entry.BoxedValue = optionValues[idx]; });
                var sec = ddCopy.AddComponent<CustomSettingsElementDescriptionController>();
                sec.description = entry.Description.Description;
                sec.isInputSettingElement = false;
                ddCopy.SetActive(true);
                return ddCopy;
            }
            else
            {
                var radioCopy = UnityObject.Instantiate(RadioPrefab);
                var lab = radioCopy.GetChild("Label");
                lab.GetComponent<Localize>().SetTerm(entry.Definition.Key);
                lab.GetComponent<TextMeshProUGUI>().text = entry.Definition.Key;
                var setting = radioCopy.GetChild("Setting");
                var idx = optionValues.IndexOf((int)entry.BoxedValue);
                List<ToggleExtended> allToggles = new();
                for (var i = 0; i < optionNames.Count; i++)
                {
                    var settingCopy = UnityObject.Instantiate(RadioSettingPrefab, setting.transform, true);
                    var option = settingCopy.GetComponentInChildren<TextMeshProUGUI>();
                    option.text = optionNames[i];
                    var loc = settingCopy.GetComponentInChildren<Localize>();
                    loc.Term = optionNames[i];
                    var toggle = settingCopy.GetComponent<ToggleExtended>();
                    toggle.Set(i == idx);
                    var i1 = i;
                    toggle.onValueChanged.AddListener(tgl =>
                    {
                        // This should update automagically
                        var idx2 = optionValues.IndexOf((int)entry.BoxedValue);
                        if (i1 == idx2)
                        {
                            if (!tgl)
                            {
                                toggle.Set(true);
                            }

                            return;
                        }

                        if (!tgl)
                        {
                            return;
                        }

                        entry.BoxedValue = optionValues[i1];
                        for (var j = 0; j < allToggles.Count; j++)
                        {
                            if (j == i1) continue;
                            if (allToggles[j])
                                allToggles[j].Set(false);
                        }
                    });
                    allToggles.Add(toggle);
                    settingCopy.SetActive(true);
                }

                var sec = radioCopy.AddComponent<CustomSettingsElementDescriptionController>();
                sec.description = entry.Description.Description;
                sec.isInputSettingElement = false;
                radioCopy.SetActive(true);
                return radioCopy;
            }
        };
    }

    private static GameObject CreateBoolConfig(ConfigEntryBase baseEntry)
    {
        var entry = (ConfigEntry<bool>)baseEntry;
        var radioCopy = UnityObject.Instantiate(RadioPrefab);
        var lab = radioCopy.GetChild("Label");
        lab.GetComponent<Localize>().SetTerm(entry.Definition.Key);
        lab.GetComponent<TextMeshProUGUI>().text = entry.Definition.Key;
        var setting = radioCopy.GetChild("Setting");
        var idx = entry.Value ? 0 : 1;
        List<ToggleExtended> allToggles = new();
        for (var i = 0; i < 2; i++)
        {
            var settingCopy = UnityObject.Instantiate(RadioSettingPrefab, setting.transform, true);
            var option = settingCopy.GetComponentInChildren<TextMeshProUGUI>();
            option.text = i == 0 ? "Yes" : "No";
            var loc = settingCopy.GetComponentInChildren<Localize>();
            loc.Term = option.text;
            var toggle = settingCopy.GetComponent<ToggleExtended>();
            toggle.Set(i == idx);
            var i1 = i;
            toggle.onValueChanged.AddListener(tgl =>
            {
                // This should update automagically
                var idx2 = entry.Value ? 0 : 1;
                if (i1 == idx2)
                {
                    if (!tgl)
                    {
                        toggle.Set(true);
                    }

                    return;
                }

                if (!tgl)
                {
                    return;
                }
                entry.BoxedValue = i1 == 0;
                for (var j = 0; j < allToggles.Count; j++)
                {
                    if (j == i1) continue;
                    if (allToggles[j])
                        allToggles[j].Set(false);
                }
            });
            allToggles.Add(toggle);
            settingCopy.SetActive(true);
        }

        var sec = radioCopy.AddComponent<CustomSettingsElementDescriptionController>();
        sec.description = entry.Description.Description;
        sec.isInputSettingElement = false;
        radioCopy.SetActive(true);
        return radioCopy;
    }

    internal static void SetupDefaults()
    {
        AllPropertyDrawers[typeof(bool)] = CreateBoolConfig;
    }
}