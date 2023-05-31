using System;
using KSP.Game;
using KSP.Rendering;
using UnityEngine;
using UnityEngine.UI;
namespace SpaceWarp.UI.Settings;

public class SettingsMenuController : KerbalMonoBehaviour
{
    // TODO: We need to expose this for more than just the mods menu
    // The magic words for our invocation
    private const string SettingsMenuPath =
        "GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Popup Canvas/SettingsMenu(Clone)/Frame/Body";
    private const string CategoriesPath =
        $"{SettingsMenuPath}/Categories";

    private const string GraphicsPath = $"{CategoriesPath}/Graphics";
    private const string ContentPath =
        $"{SettingsMenuPath}/Submenus Scroll/Viewport/Content";

    private void Start()
    {
        var categories = GameObject.Find(CategoriesPath);
        var graphics = GameObject.Find(GraphicsPath);
        var modsButton =Instantiate(graphics, categories.transform);
        var content = GameObject.Find(ContentPath);
        var text = modsButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        text.text = "Mods";
        var toggle = content.GetComponentInChildren<UIAction_Void_Toggle>();
    }
}