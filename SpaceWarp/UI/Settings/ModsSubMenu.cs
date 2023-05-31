using System;
using KSP.UI;

namespace SpaceWarp.UI.Settings;

public class ModsSubMenu : SettingsSubMenu
{
    public void Awake()
    {
        SettingsConfigVariables settingsConfigVariables = gameObject.AddComponent<SettingsConfigVariables>();
        // So now we have to set up every UI button with a revert and all that button
    }

    public override void OnShow()
    {
        base.OnShow();
    }

    public override void Apply()
    {
        base.Apply();
    }

    public override void Revert()
    {
        base.Revert();
    }
}