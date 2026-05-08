using BepInEx;
using SpaceWarp.API.Mods.JSON;
using UnityEngine.UIElements;

public class ModListItemController
{
    private VisualElement _element;
    private Label _nameLabel;

    public void SetVisualElement(VisualElement visualElement)
    {
        _element = visualElement;
        _element.userData = this;
        _nameLabel = _element.Q<Label>(className: "mod-item-label");
    }

    public string Guid;
    public object Info;
    public bool IsOutdated;
    public bool IsUnsupported;
    public bool IsDisabled;

    public void SetModInfo(ModInfo info)
    {
        Info = info;
        _nameLabel.text = info.Name;
    }

    public void SetPluginInfo(PluginInfo info)
    {
        Info = info;
        _nameLabel.text = info.Metadata.Name;
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
}