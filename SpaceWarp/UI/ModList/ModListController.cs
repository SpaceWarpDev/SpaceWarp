using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods.JSON;
using SpaceWarpPatcher;
using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceWarp.UI.ModList;

public class ModListController : MonoBehaviour
{
    private VisualTreeAsset _listEntryTemplate;
    private VisualTreeAsset _dependencyTemplate;

    // Mod list UI element references
    private Button _enableAllButton;
    private Button _disableAllButton;
    private Button _revertChangesButton;
    private Label _changesLabel;
    private const string ChangesLabelText = "Changes detected: {0}. Please restart the game.";

    private Foldout _spaceWarpModFoldout;
    private VisualElement _spaceWarpModList;
    private Foldout _otherModFoldout;
    private VisualElement _otherInfoModList;
    private VisualElement _otherModList;
    private Foldout _disabledModFoldout;
    private VisualElement _disabledInfoModList;
    private VisualElement _disabledModList;

    // Details UI element references
    private Label _detailsNameLabel;
    private Label _detailsIdLabel;
    private Label _detailsAuthorLabel;
    private Label _detailsVersionLabel;
    private Label _detailsSourceLabel;
    private Label _detailsDescriptionLabel;
    private Label _detailsKspVersionLabel;
    private VisualElement _detailsContainer;
    private Foldout _detailsDependenciesFoldout;
    private VisualElement _detailsDependenciesList;

    // State
    private readonly List<VisualElement> _modItemElements = new();

    private Dictionary<string, bool> _toggles;
    private Dictionary<string, bool> _initialToggles;

    private static readonly IReadOnlyList<string> NoToggleGuids = new List<string>
    {
        SpaceWarpPlugin.ModGuid,
        ConfigurationManager.ConfigurationManager.GUID
    };

    private bool _isLoaded;

    private void OnEnable()
    {
        if (_isLoaded)
        {
            return;
        }

        InitializeElements();
        FillModLists();
        SetupToggles();
        _isLoaded = true;
    }

    private void InitializeElements()
    {
        _listEntryTemplate = AssetManager.GetAsset<VisualTreeAsset>($"spacewarp/modlist/modlistitem.uxml");
        _dependencyTemplate = AssetManager.GetAsset<VisualTreeAsset>($"spacewarp/modlist/modlistdetailsitem.uxml");

        var root = GetComponent<UIDocument>().rootVisualElement;

        // Register a callback for the back button
        root.Q<Button>("back-button").RegisterCallback<ClickEvent>(_ => root.style.display = DisplayStyle.None);

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
        _detailsNameLabel = root.Q<Label>("details-name");
        _detailsIdLabel = root.Q<Label>("details-id");
        _detailsAuthorLabel = root.Q<Label>("details-author");
        _detailsVersionLabel = root.Q<Label>("details-version");
        _detailsSourceLabel = root.Q<Label>("details-source");
        _detailsDescriptionLabel = root.Q<Label>("details-description");
        _detailsKspVersionLabel = root.Q<Label>("details-ksp-version");
        _detailsContainer = root.Q<VisualElement>("details-container");

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
    }

    private void FillModLists()
    {
        foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins)
        {
            MakeListItem(_spaceWarpModList, data =>
            {
                data.Guid = plugin.Info.Metadata.GUID;
                data.SetInfo(plugin.SpaceWarpMetadata);

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
            MakeListItem(_otherInfoModList, data =>
            {
                data.Guid = plugin.Info.Metadata.GUID;
                data.SetInfo(modInfo);

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
            MakeListItem(_otherModList, data =>
            {
                data.Guid = bepinPlugin.Info.Metadata.GUID;
                data.SetInfo(bepinPlugin.Info);
            });
        }

        foreach (var (pluginInfo, modInfo) in SpaceWarpManager.DisabledInfoPlugins)
        {
            MakeListItem(_disabledInfoModList, data =>
            {
                data.Guid = pluginInfo.Metadata.GUID;
                data.SetInfo(modInfo);
                data.SetIsOutdated();
            });
        }

        foreach (var pluginInfo in SpaceWarpManager.DisabledNonInfoPlugins)
        {
            MakeListItem(_disabledModList, data =>
            {
                data.Guid = pluginInfo.Metadata.GUID;
                data.SetInfo(pluginInfo);
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

        _enableAllButton.RegisterCallback<ClickEvent>(_ =>
        {
            _toggles = _toggles.Keys.ToDictionary(key => key, _ => true);
            UpdateToggles();
            UpdateChangesLabel();
            UpdateDisabledFile();
        });
        _disableAllButton.RegisterCallback<ClickEvent>(_ =>
        {
            _toggles = _toggles.Keys.ToDictionary(key => key, _ => false);
            UpdateToggles();
            UpdateChangesLabel();
            UpdateDisabledFile();
        });
        _revertChangesButton.RegisterCallback<ClickEvent>(_ =>
        {
            _toggles = new Dictionary<string, bool>(_initialToggles);
            UpdateToggles();
            UpdateChangesLabel();
            UpdateDisabledFile();
        });
    }

    private void MakeListItem(VisualElement container, Action<ModListItemController> bindFunc)
    {
        var element = _listEntryTemplate.Instantiate();

        var data = new ModListItemController(element);
        bindFunc(data);
        element.AddManipulator(new Clickable(OnModSelected));

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

    private void OnModSelected(EventBase evt)
    {
        if (evt.currentTarget is not TemplateContainer { userData: ModListItemController data })
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
                SetSelectedModInfo(modInfo);
                return;

            case PluginInfo plugin:
                SetSelectedPluginInfo(plugin);
                return;
        }
    }

    private void SetSelectedModInfo(ModInfo info)
    {
        SetSelected(
            info.Name,
            info.ModID,
            info.Author,
            info.Version,
            info.Source,
            info.Description,
            info.SupportedKsp2Versions.ToString(),
            info.Dependencies
                ?.Select(dependencyInfo => (dependencyInfo.ID, dependencyInfo.Version.ToString()))
                .ToList()
        );
    }

    private void SetSelectedPluginInfo(PluginInfo info)
    {
        SetSelected(info.Metadata.Name, info.Metadata.GUID, version: info.Metadata.Version.ToString());
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
        bool hasSelected = true
    )
    {
        _detailsContainer.visible = hasSelected;

        _detailsNameLabel.text = modName;
        _detailsIdLabel.text = id;
        _detailsAuthorLabel.text = author;
        _detailsVersionLabel.text = version;
        _detailsSourceLabel.text = source;
        _detailsDescriptionLabel.text = description;
        _detailsKspVersionLabel.text = kspVersion;

        SetDependencies(dependencies);
    }

    private void SetDependencies(List<(string, string)> dependencies)
    {
        _detailsDependenciesList.hierarchy.Clear();
        _detailsDependenciesFoldout.visible = dependencies is { Count: > 0 };

        if (dependencies == null)
        {
            return;
        }

        foreach (var (id, version) in dependencies)
        {
            var dependencyElement = _dependencyTemplate.Instantiate();
            dependencyElement.Q<Label>(className: "details-key-label").text = id;
            dependencyElement.Q<Label>(className: "details-value-label").text = version;
            _detailsDependenciesList.Add(dependencyElement);
        }
    }

    private void ClearSelected()
    {
        SetSelected(hasSelected: false);
    }

    private void UpdateToggles()
    {
        foreach (var element in _modItemElements)
        {
            if (element.userData is not ModListItemController data || NoToggleGuids.Contains(data.Guid))
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
            _changesLabel.text = string.Format(ChangesLabelText, numChanges);
        }
        else
        {
            _changesLabel.style.display = DisplayStyle.None;
        }
    }

    private void UpdateDisabledFile()
    {
        File.WriteAllLines(
            ChainloaderPatch.DisabledPluginsFilepath,
            _toggles.Where(item => !item.Value).Select(item => item.Key)
        );
    }
}