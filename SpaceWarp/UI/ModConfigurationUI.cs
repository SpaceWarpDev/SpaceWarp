using System;
using System.ComponentModel;
using System.Reflection;
using KSP.Game;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Managers;
using UnityEngine;

namespace SpaceWarp.UI
{

    public class ModConfigurationUI : KerbalMonoBehaviour
    {
        public Type ConfigurationType;
        public object ConfigurationObject;

        public string ModName;
        public string ModID;

        private int _windowWidth = 350;
        private int _windowHeight = 700;
        private Rect _windowRect;

        private static GUIStyle _boxStyle;

        private readonly ModConfigurationSection _rootSection = new ModConfigurationSection();

        private void Awake()
        {
            _windowWidth = (int)(Screen.width * 0.5f);
            _windowHeight = (int)(Screen.height * 0.5f);
        }

        public void Start()
        {
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
                
                if (fieldAttribute != null)
                {
                    attribute = fieldAttribute;
                    attributeName = fieldAttribute.Name;
                    _rootSection.Insert(section.Split(new []{'/'},StringSplitOptions.RemoveEmptyEntries), (attributeName, field, attribute));
                }

                attribute = fieldAttribute;
                attributeName = fieldAttribute?.Name;

                _rootSection.Insert(section.Split(new []{'/'},StringSplitOptions.RemoveEmptyEntries), (attributeName, field, attribute));
            }

            _windowRect = new Rect((Screen.width * 0.15f), (Screen.height * 0.15f), 0, 0);
        }

        public void OnGUI()
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            string header = $"{ModID} configuration";
           
            GUILayoutOption width = GUILayout.Width((float)(_windowWidth * 0.5));
            GUILayoutOption height = GUILayout.Height((float)(_windowHeight * 0.5));

            _windowRect = GUILayout.Window(controlID, _windowRect, FillWindow, header, width, height);
        }

        private void EditorInputField(string fieldName, FieldInfo info)
        {
            GUILayout.BeginHorizontal();

            if (info.FieldType != typeof(bool))
            {
                GUILayout.Label(fieldName);

                string rawInputValue = GUILayout.TextField(info.GetValue(ConfigurationObject).ToString());
                object convertedInputValue = TypeDescriptor.GetConverter(info.FieldType).ConvertFromInvariantString(rawInputValue);

                info.SetValue(ConfigurationObject, convertedInputValue);
            }
            else
            {
                bool toggleValue = GUILayout.Toggle((bool)info.GetValue(ConfigurationObject), fieldName);

                info.SetValue(ConfigurationObject, toggleValue);
            }

            GUILayout.EndHorizontal();
        }

        private void EditorForField((string name, FieldInfo info, object confAttribute) field)
        {
            if (field.confAttribute is ConfigFieldAttribute)
            {
                EditorInputField(field.name, field.info);
            }
        }

        private void SectionPropertyViewer(string sectionName, ModConfigurationSection section, string parent)
        {
            if (GUILayout.Button(parent == "" ? sectionName : parent + "/" + sectionName))
            {
                section.Open = !section.Open;
            }

            if (!section.Open)
            {
                return;
            }

            foreach ((string name, FieldInfo info, object confAttribute) property in section.Properties)
            {
                EditorForField(property);
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
            foreach ((string name, FieldInfo info, object confAttribute) field in _rootSection.Properties)
            {
                EditorForField(field);
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
                    configurationManager.UpdateConfiguration(ModID);
                }
                Destroy(this);
            }

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 500));
        }
    }
}