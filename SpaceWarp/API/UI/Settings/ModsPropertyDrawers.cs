using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using BepInEx.Configuration;
using I2.Loc;
using KSP;
using KSP.Api.CoreTypes;
using KSP.UI;
using KSP.UI.Binding;
using SpaceWarp.Backend.UI.Settings;
using SpaceWarp.InternalUtilities;
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
        if (!AllPropertyDrawers.ContainsKey(entry.SettingType))
        {
            try
            {
                AllPropertyDrawers.Add(entry.SettingType,GenerateGenericDrawerFor(entry.SettingType));
            }
            catch
            {
                //Ignored
            }
        }
        return AllPropertyDrawers.TryGetValue(entry.SettingType, out var func) ? func(entry) : null;
    }



    internal static GameObject DropdownPrefab;
    internal static GameObject RadioPrefab;
    internal static GameObject RadioSettingPrefab;
    internal static GameObject InputFieldPrefab;


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
                ddCopy.name = entry.Definition.Key;
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
                    toggle.name = optionNames[i];
                    settingCopy.SetActive(true);
                }

                var sec = radioCopy.AddComponent<CustomSettingsElementDescriptionController>();
                sec.description = entry.Description.Description;
                sec.isInputSettingElement = false;
                radioCopy.SetActive(true);
                radioCopy.name = entry.Definition.Key;
                return radioCopy;
            }
        };
    }

    
    // Should satisfy most needs
    private static Func<ConfigEntryBase, GameObject> GenerateGenericDrawerFor(Type entrySettingType)
    {
        var valueListMethod = typeof(ModsPropertyDrawers).GetMethod(nameof(CreateFromAcceptableValueList),
            BindingFlags.Static | BindingFlags.NonPublic)
            ?.MakeGenericMethod(entrySettingType);
        var valueRangeMethod = typeof(ModsPropertyDrawers).GetMethod(nameof(CreateFromAcceptableValueRange),
                BindingFlags.Static | BindingFlags.NonPublic)
            ?.MakeGenericMethod(entrySettingType);
        return entry =>
        {
            var t = entry.Description.AcceptableValues?.GetType();
            if (t?.GetGenericTypeDefinition() == typeof(AcceptableValueList<>) &&
                t.GenericTypeArguments[0] == entrySettingType)
            {
                if (valueListMethod != null)
                    return (GameObject)valueListMethod.Invoke(null, new object[] { entry, entry.Description.AcceptableValues });
            }

            if (t?.GetGenericTypeDefinition() == typeof(AcceptableValueRange<>) &&
                t.GenericTypeArguments[0] == entrySettingType)
            {
                if (valueRangeMethod != null)
                {
                    return (GameObject)valueRangeMethod.Invoke(null, new object[] { entry, entry.Description.AcceptableValues });
                }
            }
            
            var inputFieldCopy = UnityObject.Instantiate(InputFieldPrefab);
            var lab = inputFieldCopy.GetChild("Label");
            lab.GetComponent<Localize>().SetTerm(entry.Definition.Key);
            lab.GetComponent<TextMeshProUGUI>().text = entry.Definition.Key;
            var textField = inputFieldCopy.GetComponentInChildren<InputFieldExtended>();
            var textFieldObject = textField.gameObject;
            textFieldObject.name = entry.Definition.Key + ": Text Field";
            textField.characterLimit = 256;
            textField.readOnly = false;
            textField.interactable = true;
            textField.text = entry.BoxedValue.ToString();
            textField.onValueChanged.AddListener(str => { entry.BoxedValue = TypeDescriptor.GetConverter(entrySettingType).ConvertFromString(str); });
            var rectTransform = textFieldObject.transform.parent.gameObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 2.7f);
            rectTransform.anchorMax = new Vector2(0.19f, 2.25f);
            var sec = inputFieldCopy.AddComponent<CustomSettingsElementDescriptionController>();
            sec.description = entry.Description.Description;
            sec.isInputSettingElement = false;
            inputFieldCopy.SetActive(true);
            inputFieldCopy.name = entry.Definition.Key;
            return inputFieldCopy;
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
        radioCopy.name = entry.Definition.Key;
        return radioCopy;
    }

    private static GameObject CreateFromAcceptableValueList<T>(ConfigEntry<T> entry,
        AcceptableValueList<T> acceptableValues) where T : IEquatable<T>
    {
        var ddCopy = UnityObject.Instantiate(DropdownPrefab);
        var lab = ddCopy.GetChild("Label");
        lab.GetComponent<Localize>().SetTerm(entry.Definition.Key);
        lab.GetComponent<TextMeshProUGUI>().text = entry.Definition.Key;
        var dropdown = ddCopy.GetChild("Setting").GetChild("KSP2DropDown");
        var extended = dropdown.GetComponent<DropdownExtended>();
        // Start by clearing the options data
        extended.options.Clear();
        foreach (var option in acceptableValues.AcceptableValues)
        {
            extended.options.Add(new TMP_Dropdown.OptionData(option as string ?? (option is Color color
                ? (color.a < 1 ? ColorUtility.ToHtmlStringRGBA(color) : ColorUtility.ToHtmlStringRGB(color))
                : option.ToString())));
        }

        extended.value = acceptableValues.AcceptableValues.IndexOf(entry.Value);
        extended.onValueChanged.AddListener(idx => { entry.Value = acceptableValues.AcceptableValues[idx]; });
        var sec = ddCopy.AddComponent<CustomSettingsElementDescriptionController>();
        sec.description = entry.Description.Description;
        sec.isInputSettingElement = false;
        ddCopy.SetActive(true);
        ddCopy.name = entry.Definition.Key;
        return ddCopy;
    }

    internal static GameObject SliderPrefab;
    
    // We should just assume that stuff can be converted to a float for this
    private static GameObject CreateFromAcceptableValueRange<T>(ConfigEntry<T> entry,
        AcceptableValueRange<T> acceptableValues) where T : IComparable
    {
        // Now we have to have a "slider" prefab
        var slCopy = UnityObject.Instantiate(SliderPrefab);
        var lab = slCopy.GetChild("Label");
        lab.GetComponent<Localize>().SetTerm(entry.Definition.Key);
        lab.GetComponent<TextMeshProUGUI>().text = entry.Definition.Key;
        var setting = slCopy.GetChild("Setting");
        var slider = setting.GetChild("KSP2SliderLinear").GetComponent<SliderExtended>();
        var amount = setting.GetChild("Amount display");
        var text = amount.GetComponentInChildren<TextMeshProUGUI>();
        text.text = entry.Value.ToString();
        Func<T, float> toFloat = x => Convert.ToSingle(x);
        // if (!typeof(T).IsIntegral())
        // {
        //     var convT = TypeDescriptor.GetConverter(typeof(T)) ??
        //                 throw new ArgumentNullException("TypeDescriptor.GetConverter(typeof(T))");
        //     toT = x => (T)convT.ConvertFrom(x);
        // }
        Func<float, T> toT = Type.GetTypeCode(typeof(T)) switch
        {
            TypeCode.Byte => x => (T)(object)Convert.ToByte(x),
            TypeCode.SByte => x => (T)(object)Convert.ToSByte(x),
            TypeCode.UInt16 => x => (T)(object)Convert.ToUInt16(x),
            TypeCode.UInt32 => x => (T)(object)Convert.ToUInt32(x),
            TypeCode.UInt64 => x => (T)(object)Convert.ToUInt64(x),
            TypeCode.Int16 => x => (T)(object)Convert.ToInt16(x),
            TypeCode.Int32 => x => (T)(object)Convert.ToInt32(x),
            TypeCode.Int64 => x => (T)(object)Convert.ToInt64(x),
            TypeCode.Decimal => x => (T)(object)Convert.ToDecimal(x),
            TypeCode.Double => x => (T)(object)Convert.ToDouble(x),
            TypeCode.Single => x => (T)(object)x,
            _ => x => throw new NotImplementedException(typeof(T).ToString())
        };

        slider.onValueChanged.AddListener(value =>
        {
            // var trueValue = (acceptableValues.MaxValue-acceptableValues.MinValue) * (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(value)
            var trueValue = (toFloat(acceptableValues.MaxValue) - toFloat(acceptableValues.MinValue)) * value +
                            toFloat(acceptableValues.MinValue);

            entry.Value = toT(trueValue) ?? entry.Value;
            if (entry.Value != null) text.text = entry.Value.ToString();
            slider.SetWithoutCallback(toFloat(entry.Value));
        });

        var sec = slCopy.AddComponent<CustomSettingsElementDescriptionController>();
        sec.description = entry.Description.Description;
        sec.isInputSettingElement = false;
        slCopy.SetActive(true);
        slCopy.name = entry.Definition.Key;
        return slCopy;
    }

    private static GameObject CreateStringConfig(ConfigEntryBase entryBase)
    {
        var entry = (ConfigEntry<string>)entryBase;
        if (entry.Description.AcceptableValues is AcceptableValueList<string> str)
        {
            return CreateFromAcceptableValueList(entry, str);
        }

        var inputFieldCopy = UnityObject.Instantiate(InputFieldPrefab);
        var lab = inputFieldCopy.GetChild("Label");
        lab.GetComponent<Localize>().SetTerm(entry.Definition.Key);
        lab.GetComponent<TextMeshProUGUI>().text = entry.Definition.Key;
        var textField = inputFieldCopy.GetComponentInChildren<InputFieldExtended>();
        var textFieldObject = textField.gameObject;
        textFieldObject.name = entry.Definition.Key + ": Text Field";
        textField.characterLimit = 256;
        textField.readOnly = false;
        textField.interactable = true;
        textField.text = entry.Value;
        textField.onValueChanged.AddListener(str => { entry.Value = str; });
        var rectTransform = textFieldObject.transform.parent.gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 2.7f);
        rectTransform.anchorMax = new Vector2(0.19f, 2.25f);
        var sec = inputFieldCopy.AddComponent<CustomSettingsElementDescriptionController>();
        sec.description = entry.Description.Description;
        sec.isInputSettingElement = false;
        inputFieldCopy.SetActive(true);
        inputFieldCopy.name = entry.Definition.Key;
        return inputFieldCopy;
    }

    private static GameObject CreateColorConfig(ConfigEntryBase entryBase)
    {
        var entry = (ConfigEntry<Color>)entryBase;
        if (entry.Description.AcceptableValues is AcceptableValueList<Color> col)
        {
            return CreateFromAcceptableValueList(entry, col);
        }
        var inputFieldCopy = UnityObject.Instantiate(InputFieldPrefab);
        var lab = inputFieldCopy.GetChild("Label");
        lab.GetComponent<Localize>().SetTerm(entry.Definition.Key);
        lab.GetComponent<TextMeshProUGUI>().text = entry.Definition.Key;
        var textField = inputFieldCopy.GetComponentInChildren<InputFieldExtended>();
        var textFieldObject = textField.gameObject;
        textFieldObject.name = entry.Definition.Key + ": Color Field";
        textField.characterLimit = 256;
        textField.readOnly = false;
        textField.interactable = true;
        textField.text = entry.Value.a < 1 ? ColorUtility.ToHtmlStringRGBA(entry.Value) : ColorUtility.ToHtmlStringRGB(entry.Value);
        textField.onValueChanged.AddListener(str => {
            if (ColorUtility.TryParseHtmlString(str, out var color))
            {
                entry.Value = color;
            }
        });
        var rectTransform = textFieldObject.transform.parent.gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 2.7f);
        rectTransform.anchorMax = new Vector2(0.19f, 2.25f);
        var sec = inputFieldCopy.AddComponent<CustomSettingsElementDescriptionController>();
        sec.description = entry.Description.Description;
        sec.isInputSettingElement = false;
        inputFieldCopy.SetActive(true);
        inputFieldCopy.name = entry.Definition.Key;
        return inputFieldCopy;
    }

    internal static void SetupDefaults()
    {
        AllPropertyDrawers[typeof(bool)] = CreateBoolConfig;
        AllPropertyDrawers[typeof(string)] = CreateStringConfig;
        AllPropertyDrawers[typeof(Color)] = CreateColorConfig;
    }
}