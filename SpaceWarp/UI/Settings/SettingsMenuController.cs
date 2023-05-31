using System;
using I2.Loc;
using KSP.Api;
using KSP.Api.CoreTypes;
using KSP.Game;
using KSP.Rendering;
using KSP.UI;
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

    private const string ContentGraphicsPath = $"{ContentPath}/GraphicsSettingsMenu";
    private ModsSubMenu _modsSubMenu;
    private GameObject _headerPrefab;
    private GameObject _dividerPrefab;
    private GameObject _sectionPrefab;
    private void Start()
    {
        var categories = GameObject.Find(CategoriesPath);
        var graphics = GameObject.Find(GraphicsPath);
        var modsButton =Instantiate(graphics, categories.transform);
        var content = GameObject.Find(ContentPath);
        var graphicsSettings = GameObject.Find(ContentGraphicsPath);
        _headerPrefab = Instantiate(graphicsSettings.transform.Find("SettingsMenuHeader").gameObject);
        _headerPrefab.Persist();
        _headerPrefab.SetActive(false);
        _dividerPrefab = Instantiate(graphicsSettings.transform.Find("SettingsMenuDivider").gameObject);
        _dividerPrefab.Persist();
        _dividerPrefab.SetActive(false);
        _sectionPrefab = Instantiate(graphicsSettings.transform.Find("Video").gameObject);
        foreach (Transform child in _sectionPrefab.transform)
        {
            if (child.gameObject.name != "Title")
            {
                Destroy(child.gameObject);
            }
        }
        _sectionPrefab.Persist();
        _sectionPrefab.SetActive(false);
        modsButton.GetComponentInChildren<Localize>().Term = "";
        var text = modsButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        text.text = "Mods";
        var toggle = modsButton.GetComponentInChildren<UIAction_Void_Toggle>();

        var submenu = Instantiate(graphicsSettings, content.transform);
        Destroy(submenu.GetComponent<GraphicsSettingsMenuManager>());
        Destroy(submenu.GetComponent<SettingsConfigVariables>());
        _modsSubMenu = submenu.AddComponent<ModsSubMenu>();
        _modsSubMenu.GenerateTitle = GenerateTitle;
        _modsSubMenu.GenerateDivider = GenerateDivider;
        _modsSubMenu.GenerateSectionHeader = GenerateSection;
        foreach (Transform child in submenu.transform)
        {
            Destroy(child.gameObject);
        }
        // Now we have to set up the functions for mod names and categories
        Game.SettingsMenuManager._dataContext.AddAction("ToggleModsSettings",ToggleModsSettings);
        toggle.actionKey = "ToggleModsSettings";
        toggle.action = new DelegateAction(ToggleModsSettings);
        _modsSubMenu.gameObject.SetActive(false);
    }

    private void ToggleModsSettings()
    {
        Game.SettingsMenuManager.ToggleMenu(_modsSubMenu);
        Game.SettingsMenuManager.UpdateResetButton("Mods");
    }

    private GameObject GenerateTitle(string title)
    {
        var copy = Instantiate(_headerPrefab);
        var text = copy.GetComponent<TMPro.TextMeshProUGUI>();
        copy.GetComponent<Localize>().Term = title;
        text.text = title;
        copy.SetActive(true);
        return copy;
    }

    private GameObject GenerateDivider()
    {
        var copy = Instantiate(_dividerPrefab);
        copy.SetActive(true);
        return copy;
    }

    private GameObject GenerateSection(string section)
    {
        var copy = Instantiate(_sectionPrefab);
        var title = copy.transform.Find("Title");

        var loc = copy.GetComponent<Localize>();
        if (loc != null)
        {
            loc.Term = section;
        }
        var text = title.GetComponent<TMPro.TextMeshProUGUI>();
        text.text = section;
        copy.SetActive(true);
        return copy;
    }
}