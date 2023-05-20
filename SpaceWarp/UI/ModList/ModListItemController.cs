using BepInEx;
using SpaceWarp.API.Mods.JSON;
using UnityEngine.UIElements;

namespace SpaceWarp.UI.ModList;

public class ModListItemController
{
    private readonly Label _nameLabel;

    public ModListItemController(VisualElement visualElement)
    {
        visualElement.userData = this;
        _nameLabel = visualElement.Q<Label>(className: "mod-item-label");
    }

    public string Guid { get; set; }
    public object Info { get; private set; }
    public bool IsOutdated { get; private set; }
    public bool IsUnsupported { get; private set; }
    public bool IsDisabled { get; private set; }

    public void SetInfo(ModInfo info)
    {
        Info = info;
        _nameLabel.text = Trim(info.Name);
    }

    public void SetInfo(PluginInfo info)
    {
        Info = info;
        _nameLabel.text = Trim(info.Metadata.Name);
    }

    public void SetIsOutdated()
    {
        IsOutdated = true;
        _nameLabel.AddToClassList("outdated");
    }

    public void SetIsUnsupported()
    {
        IsUnsupported = true;
        _nameLabel.AddToClassList("unsupported");
    }

    public void SetIsDisabled()
    {
        IsDisabled = true;
        _nameLabel.AddToClassList("disabled");
    }

    private static string Trim(string name)
    {
        if (name.Length > 25)
        {
            name = name[..22] + "...";
        }

        return name;
    }
}