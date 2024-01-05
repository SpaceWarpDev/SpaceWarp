using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using BepInEx;
using SpaceWarp;
using SpaceWarp.API.Mods.JSON;
using UnityEngine;
using UnityEngine.UIElements;


public class ModListView : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset listEntryTemplate;
    [SerializeField] private VisualTreeAsset dependencyTemplate;

    // Mod list UI element references
    private Button _enableAllButton;
    private Button _disableAllButton;
    private Button _revertChangesButton;
    private Label _changesLabel;
    private string _changesLabelText = "Changes detected: {0}. Please restart the game.";

    private Foldout _spaceWarpModFoldout;
    private VisualElement _spaceWarpModList;
    private Foldout _otherModFoldout;
    private VisualElement _otherInfoModList;
    private VisualElement _otherModList;
    private Foldout _disabledModFoldout;
    private VisualElement _disabledInfoModList;
    private VisualElement _disabledModList;

    // Details UI element references
    private VisualElement _detailsContainer;
    private Label _detailsNameLabel;
    private Label _detailsIdLabel;
    private Label _detailsAuthorLabel;
    private Label _detailsVersionLabel;
    private Button _detailsSourceLink;
    private Label _detailsDescriptionLabel;
    private Label _detailsKspVersionLabel;
    private VisualElement _detailsOutdatedWarning;
    private VisualElement _detailsUnsupportedWarning;
    private VisualElement _detailsDisabledWarning;
    private Foldout _detailsDependenciesFoldout;
    private VisualElement _detailsDependenciesList;

    // State
    private readonly List<VisualElement> _modItemElements = new List<VisualElement>();

    private Dictionary<string, bool> _toggles;
    private Dictionary<string, bool> _initialToggles;
    private readonly Dictionary<string, bool> _wasToggledDict = new Dictionary<string, bool>();

    private static readonly IReadOnlyList<string> NoToggleGuids = new List<string>
    {
        "com.github.x606.spacewarp",
        "com.bepis.bepinex.configurationmanager"
    };

    private void OnEnable()
    {
        InitializeElements();
        FillModLists();
        SetupToggles();
    }

    private void InitializeElements()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Register a callback for the back button
        root.Q<Button>("back-button").RegisterCallback<ClickEvent>(evt => transform.gameObject.SetActive(false));

        // Store references to the Mod list UI elements
        _enableAllButton = root.Q<Button>("enable-all-button");
        _disableAllButton = root.Q<Button>("disable-all-button");
        _revertChangesButton = root.Q<Button>("revert-changes-button");
        _changesLabel = root.Q<Label>("changes-label");

        _spaceWarpModFoldout = root.Q<Foldout>("spacewarp-mod-foldout");
        _spaceWarpModList = root.Q<VisualElement>("spacewarp-mod-list");

        _otherModFoldout = root.Q<Foldout>("other-mod-foldout");
        _otherInfoModList = root.Q<VisualElement>("other-info-mod-list");
        _otherModList = root.Q<VisualElement>("other-mod-list");

        _disabledModFoldout = root.Q<Foldout>("disabled-mod-foldout");
        _disabledInfoModList = root.Q<VisualElement>("disabled-info-mod-list");
        _disabledModList = root.Q<VisualElement>("disabled-mod-list");

        // Store references to the selected mod details UI element references
        _detailsContainer = root.Q<VisualElement>("details-container");
        _detailsNameLabel = root.Q<Label>("details-name");
        _detailsIdLabel = root.Q<Label>("details-id");
        _detailsAuthorLabel = root.Q<Label>("details-author");
        _detailsVersionLabel = root.Q<Label>("details-version");
        _detailsSourceLink = root.Q<Button>("details-source");
        _detailsDescriptionLabel = root.Q<Label>("details-description");
        _detailsKspVersionLabel = root.Q<Label>("details-ksp-version");

        _detailsOutdatedWarning = root.Q<VisualElement>("details-outdated-warning");
        _detailsUnsupportedWarning = root.Q<VisualElement>("details-unsupported-warning");
        _detailsDisabledWarning = root.Q<VisualElement>("details-disabled-warning");

        _detailsDependenciesFoldout = root.Q<Foldout>("details-dependencies-foldout");
        _detailsDependenciesList = root.Q<VisualElement>("details-dependencies-list");

        // Show only categories that have any mods in them
        if (SpaceWarpManager.SpaceWarpPlugins.Count + SpaceWarpManager.NonSpaceWarpInfos.Count > 0)
        {
            _spaceWarpModFoldout.style.display = DisplayStyle.Flex;
        }

        if (SpaceWarpManager.NonSpaceWarpPlugins.Count > 0)
        {
            _otherModFoldout.style.display = DisplayStyle.Flex;
        }

        if (SpaceWarpManager.DisabledInfoPlugins.Count + SpaceWarpManager.DisabledNonInfoPlugins.Count > 0)
        {
            _disabledModFoldout.style.display = DisplayStyle.Flex;
        }

        _detailsSourceLink.RegisterCallback<ClickEvent>(_ =>
        {
            var url = _detailsSourceLink.text.Trim();
            if (url.StartsWith("http"))
            {
                Application.OpenURL(url);
            }
        });
    }

    private void FillModLists()
    {
        foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins)
        {
            MakeListItem(_spaceWarpModList, (element, data) =>
            {
                data.Guid = plugin.Info.Metadata.GUID;
                data.SetModInfo(plugin.SpaceWarpMetadata);

                if (SpaceWarpManager.ModsOutdated[plugin.SpaceWarpMetadata.ModID])
                {
                    data.SetIsOutdated();
                }

                if (SpaceWarpManager.ModsUnsupported[plugin.SpaceWarpMetadata.ModID])
                {
                    data.SetIsUnsupported();
                }
            });
        }

        foreach (var (plugin, modInfo) in SpaceWarpManager.NonSpaceWarpInfos)
        {
            MakeListItem(_otherInfoModList, (element, data) =>
            {
                data.Guid = plugin.Info.Metadata.GUID;
                data.SetModInfo(modInfo);

                if (SpaceWarpManager.ModsOutdated[modInfo.ModID])
                {
                    data.SetIsOutdated();
                }

                if (SpaceWarpManager.ModsUnsupported[modInfo.ModID])
                {
                    data.SetIsUnsupported();
                }
            });
        }

        foreach (var bepinPlugin in SpaceWarpManager.NonSpaceWarpPlugins)
        {
            MakeListItem(_otherModList, (element, data) =>
            {
                data.Guid = bepinPlugin.Info.Metadata.GUID;
                data.SetPluginInfo(bepinPlugin.Info);
            });
        }

        foreach (var (pluginInfo, modInfo) in SpaceWarpManager.DisabledInfoPlugins)
        {
            MakeListItem(_disabledInfoModList, (element, data) =>
            {
                data.Guid = pluginInfo.Metadata.GUID;
                data.SetModInfo(modInfo);
                data.SetIsDisabled();

                if (SpaceWarpManager.ModsOutdated[modInfo.ModID])
                {
                    data.SetIsOutdated();
                }

                if (SpaceWarpManager.ModsUnsupported[modInfo.ModID])
                {
                    data.SetIsUnsupported();
                }
            });
        }

        foreach (var pluginInfo in SpaceWarpManager.DisabledNonInfoPlugins)
        {
            MakeListItem(_disabledModList, (element, data) =>
            {
                data.Guid = pluginInfo.Metadata.GUID;
                data.SetPluginInfo(pluginInfo);
                data.SetIsDisabled();
            });
        }
    }

    private void SetupToggles()
    {
        _initialToggles = SpaceWarpManager.PluginGuidEnabledStatus.Where(
            item => !NoToggleGuids.Contains(item.Item1)
        ).ToDictionary(item => item.Item1, item => item.Item2);
        _toggles = new Dictionary<string, bool>(_initialToggles);
        UpdateToggles();

        var noToggleElements = _modItemElements.Where(element =>
            NoToggleGuids.Contains(((ModListItemController)element.userData).Guid)
        );
        foreach (var element in noToggleElements)
        {
            element.Q<Toggle>().RemoveFromHierarchy();
        }

        _enableAllButton.RegisterCallback<ClickEvent>(evt =>
        {
            _toggles = _toggles.Keys.ToDictionary(key => key, _ => true);
            UpdateToggles();
            UpdateChangesLabel();
            UpdateDisabledFile();
        });
        _disableAllButton.RegisterCallback<ClickEvent>(evt =>
        {
            _toggles = _toggles.Keys.ToDictionary(key => key, _ => false);
            UpdateToggles();
            UpdateChangesLabel();
            UpdateDisabledFile();
        });
        _revertChangesButton.RegisterCallback<ClickEvent>(evt =>
        {
            _toggles = new Dictionary<string, bool>(_initialToggles);
            UpdateToggles();
            UpdateChangesLabel();
            UpdateDisabledFile();
        });
    }

    private void MakeListItem(VisualElement container, Action<VisualElement, ModListItemController> bindFunc)
    {
        var element = listEntryTemplate.Instantiate();

        var data = new ModListItemController();
        data.SetVisualElement(element);
        bindFunc(element, data);
        element.RegisterCallback<ClickEvent>(OnModSelected);

        if (!NoToggleGuids.Contains(data.Guid))
        {
            element.Q<Toggle>().RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                _toggles[data.Guid] = evt.newValue;
                UpdateDisabledFile();
                UpdateChangesLabel();
            });
        }

        _modItemElements.Add(element);
        container.Add(element);
    }

    private void OnModSelected(ClickEvent evt)
    {
        if (!(evt.currentTarget is TemplateContainer { userData: ModListItemController data }))
        {
            return;
        }

        foreach (var modItem in _modItemElements)
        {
            if (modItem == evt.currentTarget)
            {
                modItem[0].AddToClassList("selected");
            }
            else
            {
                modItem[0].RemoveFromClassList("selected");
            }
        }

        switch (data.Info)
        {
            // Handle non-selection (Escape to deselect everything)
            case null:
                ClearSelected();
                return;

            case ModInfo modInfo:
                SetSelectedModInfo(data, modInfo);
                return;

            case PluginInfo plugin:
                SetSelectedPluginInfo(data, plugin);
                return;
        }
    }

    private void SetSelectedModInfo(ModListItemController data, ModInfo info)
    {
        SetSelected(
            info.Name,
            data.Guid,
            info.Author,
            info.Version,
            info.Source,
            info.Description,
            info.SupportedKsp2Versions.ToString(),
            info.Dependencies
                ?.Select(dependencyInfo => (dependencyInfo.ID, dependencyInfo.Version.ToString()))
                .ToList(),
            data.IsOutdated,
            data.IsUnsupported,
            data.IsDisabled
        );
    }

    private void SetSelectedPluginInfo(ModListItemController data, PluginInfo info)
    {
        SetSelected(
            info.Metadata.Name,
            data.Guid,
            version: info.Metadata.Version.ToString(),
            isOutdated: data.IsOutdated,
            isUnsupported: data.IsUnsupported,
            isDisabled: data.IsDisabled
        );
    }

    private void SetSelected(
        string modName = "",
        string id = "",
        string author = "",
        string version = "",
        string source = "",
        string description = "",
        string kspVersion = "",
        List<(string, string)> dependencies = null,
        bool isOutdated = false,
        bool isUnsupported = false,
        bool isDisabled = false,
        bool hasSelected = true
    )
    {
        _detailsContainer.visible = hasSelected;

        _detailsNameLabel.text = modName;
        _detailsIdLabel.text = id;
        _detailsAuthorLabel.text = author;
        _detailsVersionLabel.text = version;
        _detailsSourceLink.text = "https://github.com";
        _detailsDescriptionLabel.text = description;
        _detailsKspVersionLabel.text = kspVersion;

        SetDependencies(dependencies);
        SetModWarnings(isOutdated, isUnsupported, isDisabled);
    }

    private void SetDependencies(List<(string, string)> dependencies)
    {
        _detailsDependenciesList.hierarchy.Clear();
        _detailsDependenciesFoldout.visible = dependencies != null && dependencies.Count > 0;

        if (dependencies == null)
        {
            return;
        }

        foreach (var (id, version) in dependencies)
        {
            var dependencyElement = dependencyTemplate.Instantiate();
            dependencyElement.Q<Label>(className: "details-key-label").text = id;
            dependencyElement.Q<Label>(className: "details-value-label").text = version;
            _detailsDependenciesList.Add(dependencyElement);
        }
    }

    private void SetModWarnings(bool isOutdated, bool isUnsupported, bool isDisabled)
    {
        _detailsOutdatedWarning.style.display = isOutdated ? DisplayStyle.Flex : DisplayStyle.None;
        _detailsUnsupportedWarning.style.display = isUnsupported ? DisplayStyle.Flex : DisplayStyle.None;
        _detailsDisabledWarning.style.display = isDisabled ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void ClearSelected()
    {
        SetSelected(hasSelected: false);
    }

    private void UpdateToggles()
    {
        foreach (var element in _modItemElements)
        {
            if (!(element.userData is ModListItemController data) || NoToggleGuids.Contains(data.Guid))
            {
                continue;
            }

            element.Q<Toggle>().SetValueWithoutNotify(_toggles[data.Guid]);
        }
    }

    private void UpdateChangesLabel()
    {
        var numChanges = _toggles.Count(entry =>
            _initialToggles.ContainsKey(entry.Key) && _initialToggles[entry.Key] != entry.Value
        );

        if (numChanges > 0)
        {
            _changesLabel.style.display = DisplayStyle.Flex;
            _changesLabel.text = string.Format(_changesLabelText, numChanges);
        }
        else
        {
            _changesLabel.style.display = DisplayStyle.None;
        }
    }

    private void UpdateDisabledFile()
    {
        // File.WriteAllLines(
        //     ChainloaderPatch.DisabledPluginsFilepath,
        //     _toggles.Where(item => !item.Item2).Select(item => item.Item1)
        // );
    }
}