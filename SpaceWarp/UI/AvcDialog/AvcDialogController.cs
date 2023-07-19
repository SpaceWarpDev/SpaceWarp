using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceWarp.UI.AvcDialog;

public class AvcDialogController : MonoBehaviour
{
    internal SpaceWarpPlugin Plugin;

    private void Awake()
    {
        var document = GetComponent<UIDocument>();
        document.EnableLocalization();

        var container = document.rootVisualElement;
        container[0].CenterByDefault();

        container.Q<Button>("yes-button").RegisterCallback<ClickEvent>(_ =>
        {
            Plugin.ConfigFirstLaunch.Value = false;
            Plugin.ConfigCheckVersions.Value = true;
            Plugin.CheckVersions();
            gameObject.SetActive(false);
        });

        container.Q<Button>("no-button").RegisterCallback<ClickEvent>(_ =>
        {
            Plugin.ConfigFirstLaunch.Value = false;
            Plugin.ConfigCheckVersions.Value = false;
            gameObject.SetActive(false);
        });
    }
}