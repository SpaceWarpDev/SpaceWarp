using System;
using System.Collections.Generic;
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
        public Type configurationType;
        public object configurationObject;
        public string modName;
        public string modID;

    
        private int windowWidth = 350;
        private int windowHeight = 700;
        private Rect windowRect;


        private List<(string name, FieldInfo info, object confAttribute)> fieldsToConfigure =
            new List<(string name, FieldInfo info, object confAttribute)>();

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
                var fieldAttribute = field.GetCustomAttribute<ConfigFieldAttribute>();
                if (fieldAttribute != null)
                {
                    attribute = fieldAttribute;
                    attributeName = fieldAttribute.Name;
                    fieldsToConfigure.Add((attributeName, field, attribute));
                    
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


        public void EditorInputField(string fieldName, FieldInfo info, ConfigFieldAttribute fieldAttribute)
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
        
        public void EditorForField((string name, FieldInfo info, object confAttribute) field)
        {
            if (field.confAttribute is ConfigFieldAttribute fieldAttribute)
            {
                EditorInputField(field.name,field.info,fieldAttribute);
            }
        }
        private static GUIStyle boxStyle;

        private void FillWindow(int windowID)
        {
            boxStyle = GUI.skin.GetStyle("Box");
            GUILayout.BeginVertical();
            foreach (var field in fieldsToConfigure)
            {
                EditorForField(field);
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
}