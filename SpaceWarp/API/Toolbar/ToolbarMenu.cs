using KSP.Game;
using KSP.Sim.impl;
using SpaceWarp.API.AssetBundles;
using UnityEngine;

namespace SpaceWarp.API.Toolbar
{
    public abstract class ToolbarMenu : KerbalBehavior
    {
        private GUISkin _spaceWarpConsoleSkin = null;
        private bool _drawing = false;
        
        public abstract float Width
        {
            get;
        }

        public abstract float Height
        {
            get;
        }
        public virtual GUISkin Skin
        {
            get
            {
                if (_spaceWarpConsoleSkin == null)
                {
                    ResourceManager.TryGetAsset($"space_warp/swconsoleui/swconsoleUI/spacewarpConsole.guiskin", out _spaceWarpConsoleSkin);
                }

                return _spaceWarpConsoleSkin;
            }
        }

        public string Name;
        private Rect _windowRect;

        public void Awake()
        {
            
            _windowRect = new Rect(Width,Height, 0, 0);
        }

        public void OnGUI()
        {
            if (!_drawing
                || GameManager.Instance.Game.GlobalGameState.GetState() != GameState.FlightView) return;
            GUI.skin = Skin;
            
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            GUILayoutOption width = GUILayout.Width(Width);
            GUILayoutOption height = GUILayout.Height(Height);
            
            _windowRect = GUILayout.Window(controlID, _windowRect, DoDrawing, Name, width, height);
        }

        internal void ToggleGUI(bool drawing)
        {
            _drawing = drawing;
        }

        public void ToggleGUI()
        {
            _drawing = !_drawing;
        }

        private void DoDrawing(int windowID)
        {
            DrawWindow(windowID);
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
        
        public abstract void DrawWindow(int windowID);
    }
}