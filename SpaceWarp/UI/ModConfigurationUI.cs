using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using KSP.Game;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Managers;
using UnityEngine;

namespace SpaceWarp.UI
{
    
    public class ModConfigurationUI : KerbalMonoBehaviour
    {
        public Type configurationType;
        public object configurationObject;
        public string modName;
        public string modID;

    
        private int windowWidth = 350;
        private int windowHeight = 700;
        private Rect windowRect;
        
        //Have the file structure
        
        // private List<(string name, FieldInfo info, object confAttribute)> fieldsToConfigure =
        //     new List<(string name, FieldInfo info, object confAttribute)>();

        private ModConfigurationSection _rootSection = new ModConfigurationSection();
        
        
        void Awake()
        {
            windowWidth = (int)(Screen.width * 0.5f);
            windowHeight = (int)(Screen.height * 0.5f);
        }

        public void Start()
        {
            foreach (var field in configurationType.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                object attribute = null;
                string attributeName = "";
                string section = "";
                var sectionAttribute = field.GetCustomAttribute<ConfigSectionAttribute>();
                if (sectionAttribute != null)
                {
                    section = sectionAttribute.Path;
                }
                
                var fieldAttribute = field.GetCustomAttribute<ConfigFieldAttribute>();
                if (fieldAttribute != null)
                {
                    attribute = fieldAttribute;
                    attributeName = fieldAttribute.Name;
                    _rootSection.Insert(section.Split(new []{'/'},StringSplitOptions.RemoveEmptyEntries), (attributeName, field, attribute));
                }
            }
            windowRect = new Rect((Screen.width * 0.15f), (Screen.height * 0.15f),
                0, 0);
        }

        public void OnGUI()
        {windowRect = GUILayout.Window(
                GUIUtility.GetControlID(FocusType.Passive),
                windowRect,
                FillWindow,
                $"{modID} configuration",
                GUILayout.Width((float)(windowWidth * 0.5)),
                GUILayout.Height((float)(windowHeight * 0.5))
            );
        }


        private void EditorInputField(string fieldName, FieldInfo info, ConfigFieldAttribute fieldAttribute)
        {
            GUILayout.BeginHorizontal();
            if (info.FieldType != typeof(bool))
            {
                GUILayout.Label(fieldName);
                var val = GUILayout.TextField(info.GetValue(configurationObject).ToString());
                info.SetValue(configurationObject,
                    TypeDescriptor.GetConverter(info.FieldType).ConvertFromInvariantString(val));
            }
            else
            {
                info.SetValue(configurationObject,GUILayout.Toggle((bool)info.GetValue(configurationObject),fieldName));
            }

            GUILayout.EndHorizontal();
        }
        
        private void EditorForField((string name, FieldInfo info, object confAttribute) field)
        {
            if (field.confAttribute is ConfigFieldAttribute fieldAttribute)
            {
                EditorInputField(field.name,field.info,fieldAttribute);
            }
        }

        private void SectionPropertyViewer(string sectionName, ModConfigurationSection section, string parent)
        {
            if (GUILayout.Button(parent == "" ? sectionName : parent + "/" + sectionName))
            {
                section.Open = !section.Open;
            }

            if (!section.Open) return;
            // Debug.Log($"[ModConfigurationSection] {sectionName} - {section}: {section.Properties.Count}");
            foreach (var property in section.Properties)
            {
                // Debug.Log($"[ModConfigurationUI] {property.name}, {property.info}, {property.confAttribute}");
                EditorForField(property);
            }

            foreach (var sub in section.SubSections)
            {
                SectionPropertyViewer(sub.path, sub.section, parent == "" ? sectionName : parent + "/" + sectionName);
            }
        }
        
        private static GUIStyle boxStyle;

        private void FillWindow(int windowID)
        {
            boxStyle = GUI.skin.GetStyle("Box");
            GUILayout.BeginVertical();
            // These are the root properties
            foreach (var field in _rootSection.Properties)
            {
                // Debug.Log($"[ModConfigurationUI] {field.name}, {field.info}, {field.confAttribute}");
                EditorForField(field);
            }

            foreach (var section in _rootSection.SubSections)
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

    class ModConfigurationSection
    {
        public bool Open = false;
        
        public List<(string name, FieldInfo info, object confAttribute)> Properties =
            new List<(string name, FieldInfo info, object confAttribute)>();
        public List<(string path, ModConfigurationSection section)> SubSections =
            new List<(string path, ModConfigurationSection section)>();

        private ModConfigurationSection TouchSubSection(string subsection)
        {
            var sub1 = SubSections.FirstOrDefault(sub => sub.path == subsection);
            if (sub1 != default) return sub1.section;
            var sub2 = new ModConfigurationSection();
            // Debug.Log($"[ModConfigurationSection] creating {subsection} - {sub2.GetHashCode()}");
            SubSections.Add((subsection,sub2));
            return sub2;
        }
        public void Insert(string[] path, (string name, FieldInfo info, object confAttribute) property)
        {
            var sb = new StringBuilder();
            foreach (var t in path)
            {
                sb.Append(t + "/");
            }
            // Debug.Log($"[ModConfigurationSection] {path.Length}: {sb}");
            if (path.Length > 0)
            {
                var subPath = new List<string>();
                for (var i = 1; i < path.Length; i++)
                {
                    subPath.Add(path[i]);
                }

                var recieved_sub = TouchSubSection(path[0]);
                
                // Debug.Log($"[ModConfigurationSection] received {path[0]} - {recieved_sub.GetHashCode()}");
                recieved_sub.Insert(subPath.ToArray(),property);
            }
            else
            {
                // Debug.Log($"[ModConfigurationSection] {property.name}, {property.info}, {property.confAttribute}");
                Properties.Add(property);
            }
        }
    }
}