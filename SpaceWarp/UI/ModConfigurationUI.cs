using System;
using System.ComponentModel;
using System.Reflection;
using KSP.Game;
using SpaceWarp.API;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace SpaceWarp.UI;

public class ModConfigurationUI : KerbalMonoBehaviour
{
    public Type ConfigurationType;
    public object ConfigurationObject;

    [FormerlySerializedAs("ModID")] public string modID;

    private int _windowWidth = 350;
    private int _windowHeight = 700;
    private Rect _windowRect;

    private static GUIStyle _boxStyle;

    private ModConfigurationSection _rootSection;
    private GUIStyle _spaceWarpUISkinToggled;
    private bool hasGUIStyles = false;
        
    private void Awake()
    {
        _windowWidth = (int)(Screen.width * 0.5f);
        _windowHeight = (int)(Screen.height * 0.5f);
    }

    public void Start()
    {
        _rootSection = new ModConfigurationSection();
        foreach (FieldInfo field in ConfigurationType.GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            object attribute;
            string attributeName;
            string section = "";
                
            ConfigSectionAttribute sectionAttribute = field.GetCustomAttribute<ConfigSectionAttribute>();

            if (sectionAttribute != null)
            {
                section = sectionAttribute.Path;
            }
                
            ConfigFieldAttribute fieldAttribute = field.GetCustomAttribute<ConfigFieldAttribute>();
                
            if (fieldAttribute == null)
            {
                // attribute = fieldAttribute;
                // attributeName = fieldAttribute.Name;
                // _rootSection.Insert(section.Split(new []{'/'},StringSplitOptions.RemoveEmptyEntries), (attributeName, field, attribute, field.GetValue(ConfigurationObject).ToString()));
                continue;
            }
            attribute = fieldAttribute;
            attributeName = fieldAttribute.Name;

            var value = field.GetValue(ConfigurationObject);
            _rootSection.Insert(section.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries),
                (attributeName, field, attribute, value != null ? value.ToString() : ""));
        }

        _windowRect = new Rect((Screen.width * 0.15f), (Screen.height * 0.15f), 0, 0);
    }

    private void GetGUIStyles()
    {
        _spaceWarpUISkinToggled = new GUIStyle(SpaceWarpManager.Skin.button);
        var oldNormal = _spaceWarpUISkinToggled.normal;
        var oldHover = _spaceWarpUISkinToggled.hover;
        var oldActive = _spaceWarpUISkinToggled.active;
        var oldFocused = _spaceWarpUISkinToggled.focused;
        _spaceWarpUISkinToggled.normal = _spaceWarpUISkinToggled.onNormal;
        _spaceWarpUISkinToggled.hover = _spaceWarpUISkinToggled.onHover;
        _spaceWarpUISkinToggled.active = _spaceWarpUISkinToggled.onActive;
        _spaceWarpUISkinToggled.focused = _spaceWarpUISkinToggled.onFocused;
        _spaceWarpUISkinToggled.onNormal = oldNormal;
        _spaceWarpUISkinToggled.onHover = oldHover;
        _spaceWarpUISkinToggled.onActive = oldActive;
        _spaceWarpUISkinToggled.onFocused = oldFocused;
        hasGUIStyles = true;
    }
    public void OnGUI()
    {
        GUI.skin = SpaceWarpManager.Skin;
        if (!hasGUIStyles)
        {
                
        }
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        string header = $"{modID} configuration";
            
        GUILayoutOption width = GUILayout.Width((float)(_windowWidth * 0.5));
        GUILayoutOption height = GUILayout.Height((float)(_windowHeight * 0.5));

        _windowRect = GUILayout.Window(controlID, _windowRect, FillWindow, header, width, height);
    }

    private string EditorInputField(string fieldName, FieldInfo info, string current)
    {
        var result = "";
        GUILayout.BeginHorizontal();

        if (info.FieldType != typeof(bool))
        {
            GUILayout.Label(fieldName);

            string rawInputValue = GUILayout.TextField(current);
            result = rawInputValue;
            try
            {
                object convertedInputValue = TypeDescriptor.GetConverter(info.FieldType)
                    .ConvertFromInvariantString(rawInputValue);
                info.SetValue(ConfigurationObject, convertedInputValue);
            }
            catch
            {
                // ignored
            }
        }
        else
        {
            bool toggleValue = GUILayout.Toggle((bool)info.GetValue(ConfigurationObject), fieldName);
            result = toggleValue.ToString();
            info.SetValue(ConfigurationObject, toggleValue);
        }

        GUILayout.EndHorizontal();
        return result;
    }

    private string EditorForField((string name, FieldInfo info, object confAttribute, string currentStringValue) field)
    {
        if (field.confAttribute is ConfigFieldAttribute)
        {
            return EditorInputField(field.name, field.info, field.currentStringValue);
        }
        else
        {
            return "";
        }
    }

    private void SectionPropertyViewer(string sectionName, ModConfigurationSection section, string parent)
    {
            

        if (GUILayout.Button((section.Open ? "V " : "> ") + (parent == "" ? sectionName : parent + "/" + sectionName)))
        {
            section.Open = !section.Open;
        }

        if (!section.Open)
        {
            return;
        }


        for (int i = 0; i < section.Properties.Count; i++)
        {
            var prop = section.Properties[i];
            var str = EditorForField(prop);
            prop.currentStringValue = str;
            section.Properties[i] = prop;
        }

        foreach ((string path, ModConfigurationSection section) sub in section.SubSections)
        {
            SectionPropertyViewer(sub.path, sub.section, parent == "" ? sectionName : parent + "/" + sectionName);
        }
    }

    public ModConfigurationUI(Rect windowRect)
    {
        _windowRect = windowRect;
    }

    private void FillWindow(int windowID)
    {
        _boxStyle = GUI.skin.GetStyle("Box");
        GUILayout.BeginVertical();

        // These are the root properties
        for (int i = 0; i < _rootSection.Properties.Count; i++)
        {
            var prop = _rootSection.Properties[i];
            var str = EditorForField(prop);
            prop.currentStringValue = str;
            _rootSection.Properties[i] = prop;
        }

        foreach ((string path, ModConfigurationSection section) section in _rootSection.SubSections)
        {
            SectionPropertyViewer(section.path, section.section, "");
        }

        if (GUILayout.Button("Save and close"))
        {
            //Run saving code from the configuration manager
            if (ManagerLocator.TryGet(out ConfigurationManager configurationManager))
            {
                configurationManager.UpdateConfiguration(modID);
            }
            Destroy(this);
        }

        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, 10000, 500));
    }
}