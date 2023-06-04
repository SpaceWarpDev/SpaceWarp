using BepInEx;
using SpaceWarp.API.Mods.JSON;
using UnityEngine.UIElements;

namespace SpaceWarp.UI.ModList;

internal class ModListItemController
{
    private readonly Label _nameLabel;

    internal ModListItemController(VisualElement visualElement)
    {
        visualElement.userData = this;
        _nameLabel = visualElement.Q<Label>(className: "mod-item-label");
    }

    internal string Guid { get; set; }
    internal object Info { get; private set; }
    internal bool IsOutdated { get; private set; }
    internal bool IsUnsupported { get; private set; }
    internal bool IsDisabled { get; private set; }

    internal void SetInfo(ModInfo info)
    {
        Info = info;
        _nameLabel.text = Trim(info.Name);
    }

    internal void SetInfo(PluginInfo info)
    {
        Info = info;
        _nameLabel.text = Trim(info.Metadata.Name);
    }

    internal void SetIsOutdated()
    {
        IsOutdated = true;
        _nameLabel.AddToClassList("outdated");
    }

    internal void SetIsUnsupported()
    {
        IsUnsupported = true;
        _nameLabel.AddToClassList("unsupported");
    }

    internal void SetIsDisabled()
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