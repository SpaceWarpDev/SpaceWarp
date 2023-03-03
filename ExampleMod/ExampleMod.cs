using BepInEx;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Assets;
using KSP.UI.Binding;
using KSP.Sim.impl;
using SpaceWarp;
using SpaceWarp.API.UI;
using SpaceWarp.API.UI.Appbar;
using UnityEngine;

namespace ExampleMod;

[BepInPlugin("com.SpaceWarpAuthorName.ExampleMod", "ExampleMod", "3.0.0")]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class ExampleMod : BaseSpaceWarpPlugin
{

    private bool drawUI;
    private Rect windowRect;

    private static ExampleMod Instance { get; set; }

    /// <summary>
    /// A method that is called when the mod is initialized.
    /// It loads the mod's GUI skin, registers the mod's button on the SpaceWarp application bar, and sets the singleton instance of the mod.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();
        Instance = this;

        // Example of using the logger, Were going to log a message to the console, ALT + C to open the console.
        Logger.LogInfo("Hello World, Im a spacewarp Mod.");


        // Register the mod's button in KSP 2s app.bar
        // This requires an `icon.png` file to exist under [plugin_folder]/assets/images
        Appbar.RegisterAppButton(
            "Example Mod",
            "BTN-ExampleMod",
            // Example of using the asset loader, were going to load the apps icon
            // Path format [mod_id]/images/filename
            // for bundles its [mod_id]/[bundle_name]/[path to file in bundle with out assets/bundle]/filename.extension
            // There is also a try get asset function, that returns a bool on whether or not it could grab the asset
            AssetManager.GetAsset<Texture2D>($"{SpaceWarpMetadata.ModID}/images/icon.png"),
            ToggleButton
        );
    }

    /// <summary>
    /// A method that is called when the mod's button on the SpaceWarp application bar is clicked.
    /// It toggles the <see cref="drawUI"/> flag and updates the button's state.
    /// </summary>
    /// <param name="toggle">The new state of the button.</param>
    private void ToggleButton(bool toggle)
    {
        drawUI = toggle;
        GameObject.Find("BTN-ExampleMod")?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(toggle);
    }

    /// <summary>
    /// A method that is called to draw the mod's GUI.
    /// It sets the GUI skin to the SpaceWarp GUI skin and draws the mod's window if the
    /// <see cref="drawUI"/> flag is true.
    /// </summary>
    public void OnGUI()
    {
        // Set the GUI skin to the SpaceWarp GUI skin.
        GUI.skin = Skins.ConsoleSkin;

        if (drawUI)
        {
            windowRect = GUILayout.Window(
                GUIUtility.GetControlID(FocusType.Passive),
                windowRect,
                FillWindow, // The method we call. 
                "Window Header",
                GUILayout.Height(350),
                GUILayout.Width(350)
            );
        }
    }

    /// <summary>
    /// A method that is called to draw the contents of the mod's GUI window.
    /// </summary>
    /// <param name="windowID">The ID of the GUI window.</param>
    private static void FillWindow(int windowID)
    {
        GUILayout.Label("Example Mod - Built with Space-Warp");
        GUI.DragWindow(new Rect(0, 0, 10000, 500));
    }

    /// <summary>
    /// Runs every frame and performs various tasks based on the game state.
    /// </summary>
    private void LateUpdate()
    {
        // Now lets play with some Game objects
        if (Instance.Game.GlobalGameState.GetState() == KSP.Game.GameState.MainMenu)
        {
            KSP.Audio.KSPAudioEventManager.SetMasterVolume(Mathf.Sin(Time.time) * 100);
        }
        else if (Instance.Game.GlobalGameState.GetState() == KSP.Game.GameState.FlightView)
        {
            // Getting the active vessel, staging it over and over and printing out all the parts. 
            VesselComponent _activeVessel = Instance.Game.ViewController.GetActiveSimVessel();
            
            if (_activeVessel != null)
            {
                _activeVessel.ActivateNextStage();
                Logger.LogWarning("Stagin Active Vessel: " + _activeVessel.Name);
                VesselBehavior behavior = Game.ViewController.GetBehaviorIfLoaded(_activeVessel);
                foreach (PartBehavior pb in behavior.parts)
                {
                    Logger.LogWarning(pb.name);
                }
            }
        }
    }
}