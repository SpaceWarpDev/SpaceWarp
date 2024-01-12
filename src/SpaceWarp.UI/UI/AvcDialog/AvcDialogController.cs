using SpaceWarp.Modules;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceWarp.UI.AvcDialog;

internal class AvcDialogController : MonoBehaviour
{
    internal VersionChecking Module;

    private void Awake()
    {
        var document = GetComponent<UIDocument>();
        document.EnableLocalization();

        var container = document.rootVisualElement;
        container[0].CenterByDefault();

        container.Q<Button>("yes-button").RegisterCallback<ClickEvent>(_ =>
        {
            Module.ConfigFirstLaunch.Value = false;
            Module.ConfigCheckVersions.Value = true;
            Module.CheckVersions();
            gameObject.SetActive(false);
        });

        container.Q<Button>("no-button").RegisterCallback<ClickEvent>(_ =>
        {
            Module.ConfigFirstLaunch.Value = false;
            Module.ConfigCheckVersions.Value = false;
            gameObject.SetActive(false);
        });
    }
}