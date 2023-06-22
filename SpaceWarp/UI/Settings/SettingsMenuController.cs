using System;
using I2.Loc;
using KSP.Api;
using KSP.Api.CoreTypes;
using KSP.Game;
using KSP.Rendering;
using KSP.UI;
using KSP.UI.Binding;
using KSP.UI.Binding.Core;
using SpaceWarp.API.UI.Settings;
using SpaceWarp.InternalUtilities;
using SpaceWarp.Patching;
using UnityEngine;
using UnityEngine.UI;
namespace SpaceWarp.UI.Settings;

internal class SettingsMenuController : KerbalMonoBehaviour
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

    private const string DropdownPrefabPath =
        $"{ContentPath}/GeneralSettingsMenu/Language";

    private const string RadioButtonPrefabPath =
        $"{ContentPath}/GraphicsSettingsMenu/Video/GameScreenMode";

    private const string SliderPrefabPath = $"{ContentPath}/GameplaySettingsMenu/Simulation/MaxPatchesToDisplay";

    private const string InputFieldPrefabPath = "GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Main Canvas/MainMenu(Clone)/CampaignMenu/CreateCampaignMenu/Menu/CampaignOptions/CampaignName/CampaignNameInputField";
    

    private ModsSubMenu _modsSubMenu;
    private GameObject _headerPrefab;
    private GameObject _dividerPrefab;
    private GameObject _sectionPrefab;
    private bool _alreadySetup = false; 
    private void Start()
    {
        MainMenuPatcher.MainMenuLoaded += Setup;
    }

    private void Setup()
    {
        if (_alreadySetup) return;
        _alreadySetup = true;
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
        var ddPrefab = Instantiate(GameObject.Find(DropdownPrefabPath));
        Destroy(ddPrefab.GetComponent<UIValue_ReadBool_SetActive>());
        ddPrefab.Persist();
        var dropdown = ddPrefab.GetChild("Setting").GetChild("KSP2DropDown");
        // This will be redone once I add a specific 
        Destroy(dropdown.GetComponent<UIValue_ReadDropdownOption_Text>());
        Destroy(dropdown.GetComponent<SettingsElementDescriptionController>());
        
        ddPrefab.SetActive(false);
        ModsPropertyDrawers.DropdownPrefab = ddPrefab;

        var radioPrefab = Instantiate(GameObject.Find(RadioButtonPrefabPath));
        radioPrefab.Persist();
        var radioSetting = radioPrefab.GetChild("Setting");
        Destroy(radioSetting.GetComponent<UIValue_WriteEnum_ToggleFlipFlop>());
        var radioSettingPrefab = Instantiate(radioSetting.GetChild("Fullscreen"));
        radioSettingPrefab.Persist();
        foreach (Transform child in radioSetting.transform)
        {
            Destroy(child.gameObject);
        }

        {
            var alpha = radioSettingPrefab.GetComponent<UIValue_WriteBool_Toggle>();
            if (alpha != null) Destroy(alpha);
            var beta = radioSettingPrefab.GetComponent<UIAction_Void_Toggle>();
            if (beta != null) Destroy(beta);
            
        }
        radioPrefab.SetActive(false);
        radioSettingPrefab.SetActive(false);
        ModsPropertyDrawers.RadioPrefab = radioPrefab;
        ModsPropertyDrawers.RadioSettingPrefab = radioSettingPrefab;

        var inputFieldContainer = Instantiate(ddPrefab);
        inputFieldContainer.Persist();
        var setting = inputFieldContainer.GetChild("Setting");
        foreach (Transform child in setting.transform)
        {
            Destroy(child.gameObject);
        }


        var inputFieldPrefab = Instantiate(GameObject.Find(InputFieldPrefabPath), setting.transform);
        Destroy(inputFieldPrefab.GetChild("Icons"));
        var extended = inputFieldPrefab.GetComponentInChildren<InputFieldExtended>();
        var textArea = extended.gameObject;
        Destroy(textArea.GetComponent<InputFieldIconController>());
        Destroy(textArea.GetComponent<UIValue_WriteString_Field>());
        Destroy(textArea.GetComponent<UIAction_Void_InputFieldExtended>());
        inputFieldContainer.SetActive(false);
        ModsPropertyDrawers.InputFieldPrefab = inputFieldContainer;

        var sliderPrefab = Instantiate(GameObject.Find(SliderPrefabPath));
        var sliderSetting = sliderPrefab.GetChild("Setting");
        var slider = sliderSetting.GetChild("KSP2SliderLinear");
        var amount = sliderSetting.GetChild("Amount display");
        Destroy(slider.GetComponent<UIValue_WriteNumber_Slider>());
        Destroy(amount.GetComponentInChildren<UIValue_ReadString_Text>());
        sliderPrefab.SetActive(false);
        ModsPropertyDrawers.SliderPrefab = sliderPrefab;
        
        
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
        ModsPropertyDrawers.SetupDefaults();
        SettingsManagerPatcher.AllExtrasSettingsMenus.Add(_modsSubMenu);
    }

    private void ToggleModsSettings()
    {
        Game.SettingsMenuManager.ToggleMenu(_modsSubMenu);
        Game.SettingsMenuManager._currentResetButtonLocKey = "Mods";
        Game.SettingsMenuManager._resetButtonText.SetValue("Reset Mods");
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
        var text = copy.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        var localize = copy.GetComponentInChildren<Localize>();
        if (localize != null)
        {
            localize.Term = section;
        }
        text.text = section;
        copy.SetActive(true);
        return copy;
    }
}