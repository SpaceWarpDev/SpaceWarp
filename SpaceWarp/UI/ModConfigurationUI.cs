using System;
using System.Collections.Generic;
using System.Reflection;
using KSP.Game;
using SpaceWarp.API.Configuration;
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
        public ConfigurationManager manager;


        private List<(string name, FieldInfo info, object confAttribute)> fieldsToConfigure =
            new List<(string name, FieldInfo info, object confAttribute)>();

        void Awake()
        {
            windowWidth = (int)(Screen.width * 0.5f);
            windowHeight = (int)(Screen.height * 0.5f);
        }

        public void Start()
        {
            foreach (var field in configurationType.GetFields())
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
                else
                {
                    var sliderAttribute = field.GetCustomAttribute<ConfigSliderAttribute>();
                    if (sliderAttribute != null)
                    {
                        attribute = sliderAttribute;
                        attributeName = sliderAttribute.Name;
                        fieldsToConfigure.Add((attributeName, field, attribute));
                    }
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
                $"{modName} configuration",
                GUILayout.Width((float)(windowWidth * 0.5)),
                GUILayout.Height((float)(windowHeight * 0.5))
            );
        }
        
        
        public void EditorForField((string name, FieldInfo info, object confAttribute) field)
        {
            
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
                Destroy(this);
            }
            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 500));
        }
    }
}