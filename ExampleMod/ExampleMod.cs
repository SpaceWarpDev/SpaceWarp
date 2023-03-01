using SpaceWarp.API.Mods;
using SpaceWarp.API.AssetBundles;
using SpaceWarp.API;

using Random = UnityEngine.Random;
using KSP.UI.Binding;
using KSP.Sim.impl;
using UnityEngine;


namespace ExampleMod
{
    [MainMod]
    public class ExampleMod : Mod
    {
        public GUISkin _spaceWarpUISkin;
        private bool drawUI;
        private Rect windowRect;
        bool loaded;

        public static ExampleMod Instance { get; private set; }


        public override void OnInitialized()
        {
            base.OnInitialized();
            if (loaded)
            {
                Destroy(this);
            }

            loaded = true;
            Instance = this;
            Logger.Info("Hello World, Im a spacewarp Mod.");

            //Example of using the asset loader, Were going to use the spacewarp UI skin for our GUI. 
            // [FORMAT]: space_warp/[assetbundle_name]/[folder_in_assetbundle]/[file.type]
            ResourceManager.TryGetAsset($"space_warp/swconsoleui/swconsoleUI/spacewarpConsole.guiskin", out _spaceWarpUISkin);

            //Now lets add our mod to the APP bar, so have a button to launch it. 



            SpaceWarpManager.RegisterAppButton(
                "Example Mod",
                "BTN-ExampleMod",
                 SpaceWarpManager.LoadIcon(),
                 Togglebutton);

        }

        void Togglebutton(bool toggle)
        {

            drawUI = toggle;
            GameObject.Find("BTN-ExampleMod")?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(toggle);
        }

        //For UI we can use unitys ImGUI, documentation is easily found online. 
        public void OnGUI()
        {
            //We can use the Spacewarp skin if we want.
            GUI.skin = _spaceWarpUISkin;
            if (drawUI)
            {
                windowRect = GUILayout.Window(
                GUIUtility.GetControlID(FocusType.Passive),
                windowRect,
                FillWindow, //The method we call. 
                "Window Header",
                GUILayout.Height(350),
                GUILayout.Width(350));
            }
        }

        private void FillWindow(int windowID)
        {
            GUILayout.Label("This Mod Built with Space-Warp.");
            GUI.DragWindow(new Rect(0, 0, 10000, 500));
        }

        void LateUpdate()
        {
            //Now lets play with some Game objects, first lets mess with the audio on the main menu.
            if (Instance.Game.GlobalGameState.GetState() == KSP.Game.GameState.MainMenu)
            {
                KSP.Audio.KSPAudioEventManager.SetMasterVolume(Random.Range(1.0f, 100.0f));
            }
            else if (Instance.Game.GlobalGameState.GetState() == KSP.Game.GameState.FlightView)
            {
                //Getting the active vessel ,staging it over and over and printing out all the parts. 
                var _activeVessel = Instance.Game.ViewController.GetActiveSimVessel();
                if (_activeVessel != null)
                {
                    _activeVessel.ActivateNextStage();
                    Logger.Warn("Stagin Active Vessel: " + _activeVessel.Name);
                    VesselBehavior behavior = this.Game.ViewController.GetBehaviorIfLoaded(_activeVessel);
                    foreach (PartBehavior p in behavior.parts)
                    {
                        Logger.Warn(p.name);
                    }

                }
            }



        }


    }
}
