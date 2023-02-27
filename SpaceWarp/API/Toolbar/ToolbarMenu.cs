using System;
using KSP.Sim.impl;
using SpaceWarp.API.AssetBundles;
using UnityEngine;

namespace SpaceWarp.API.Toolbar
{
    public abstract class ToolbarMenu : KerbalBehavior
    {
        private GUISkin _spaceWarpConsoleSkin = null;
        private bool _drawing = false;
        public virtual GUISkin Skin
        {
            get
            {
                if (_spaceWarpConsoleSkin == null)
                {
                    ResourceManager.TryGetAsset($"space_warp/swconsoleui/spacewarpConsole.guiskin", out _spaceWarpConsoleSkin);
                }

                return _spaceWarpConsoleSkin;
            }
        }

        public string Name;

        public void OnGUI()
        {
            if (!_drawing) return;
            GUI.skin = Skin;
            
        }

        internal void ToggleGUI(bool drawing)
        {
            _drawing = drawing;
        }
        
        public abstract void DrawWindow();
    }
}