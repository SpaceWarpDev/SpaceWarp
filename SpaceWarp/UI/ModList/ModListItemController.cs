using System.Collections.Generic;
using BepInEx;
using SpaceWarp.API.Mods;
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
    internal SpaceWarpPluginDescriptor Info { get; private set; }
    internal bool IsOutdated { get; private set; }
    internal bool IsUnsupported { get; private set; }
    internal bool IsDisabled { get; private set; }
    
    internal bool IsErrored { get; private set; }

    internal bool HasBadID { get; private set; }

    internal bool HasMismatchedVersion { get; private set; }

    internal bool BadDirectory { get; private set; }
    internal bool MissingSWInfo { get; private set; }

    internal List<string> ErroredDependencies { get; private set; } = new();
    internal List<string> MissingDependencies { get; private set; } = new();

    internal List<string> DisabledDependencies { get; private set; } = new();
    internal List<string> UnsupportedDependencies { get; private set; } = new();

    internal List<string> UnspecifiedDependencies { get; private set; } = new();


    internal void SetInfo(SpaceWarpPluginDescriptor info)
    {
        Info = info;
        _nameLabel.text = Trim(info.Name);
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

    internal void SetIsErrored()
    {
        IsErrored = true;
        _nameLabel.AddToClassList("errored");
    }

    internal void SetBadID()
    {
        SetIsErrored();
        HasBadID = true;
    }

    internal void SetMismatchedVersion()
    {
        SetIsErrored();
        HasMismatchedVersion = true;
    }

    internal void SetMissingSWInfo()
    {
        SetIsErrored();
        MissingSWInfo = true;
    }

    internal void SetBadDirectory()
    {
        SetIsErrored();
        BadDirectory = true;
    }
    
    internal void SetIsDependencyErrored(string erroredDependency)
    {
        ErroredDependencies.Add(erroredDependency);
        SetIsErrored();
    }

    internal void SetIsDependencyMissing(string missingDependency)
    {
        MissingDependencies.Add(missingDependency);
        SetIsErrored();
    }

    internal void SetIsDependencyDisabled(string disabledDependency)
    {
        DisabledDependencies.Add(disabledDependency);
        SetIsErrored();
    }
    
    
    internal void SetIsDependencyUnsupported(string unsupportedDependency)
    {
        UnsupportedDependencies.Add(unsupportedDependency);
        SetIsErrored();
    }

    internal void SetIsDependencyUnspecified(string unspecifiedDependency)
    {
        UnspecifiedDependencies.Add(unspecifiedDependency);
        SetIsErrored();
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