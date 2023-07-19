using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BepInEx;
using I2.Loc;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.API.UI;
using SpaceWarpPatcher;
using UitkForKsp2;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceWarp.UI.ModList;

internal class ModListController : MonoBehaviour
{
    private VisualTreeAsset _listEntryTemplate;
    private VisualTreeAsset _dependencyTemplate;

    // Mod list UI element references
    private VisualElement _container;

    private Button _enableAllButton;
    private Button _disableAllButton;
    private Button _revertChangesButton;
    private Label _changesLabel;
    private readonly LocalizedString _multipleChangesLabelText = "SpaceWarp/ModList/multipleChangesDetected";
    private readonly LocalizedString _singleChangeLabelText = "SpaceWarp/ModList/singleChangeDetected";

    private Foldout _enabledModFoldout;
    private VisualElement _enabledModList;
    private Foldout _erroredModFoldout;
    private VisualElement _erroredModList;
    private Foldout _disabledModFoldout;
    private VisualElement _disabledModList;

    private Button _openModsFolderButton;
    private Button _openConfigManagerButton;

    // Details UI element references
    private VisualElement _detailsContainer;
    private Label _detailsNameLabel;
    private Label _detailsIdLabel;
    private Label _detailsAuthorLabel;
    private Label _detailsVersionLabel;
    private Button _detailsSourceLink;
    private StyleFloat _detailsSourceLinkInitialBorderWidth;
    private Label _detailsDescriptionLabel;
    private Label _detailsKspVersionLabel;
    private VisualElement _detailsOutdatedWarning;
    private VisualElement _detailsUnsupportedWarning;
    private VisualElement _detailsDisabledWarning;
    private VisualElement _mismatchedIDWarning;
    private VisualElement _missingInfoWarning;
    private VisualElement _mismatchedVersionWarning;
    private VisualElement _badDirectoryWarning;
    private Foldout _detailsDependenciesFoldout;
    private VisualElement _detailsDependenciesList;

    private static LocalizedString _missingDependency = "SpaceWarp/ModList/MissingDependency";
    private static LocalizedString _erroredDependency = "SpaceWarp/ModList/ErroredDependency";
    private static LocalizedString _disabledDependency = "SpaceWarp/ModList/DisabledDependency";
    private static LocalizedString _unspecifiedDependency = "SpaceWarp/ModList/UnspecifiedDependency";
    private static LocalizedString _unsupportedDependency = "SpaceWarp/ModList/UnsupportedDependency";

    // State
    private bool _isLoaded;
    private bool _isWindowVisible;

    private readonly Dictionary<string, VisualElement> _modItemElements = new();

    private Dictionary<string, bool> _toggles;
    private Dictionary<string, bool> _initialToggles;

    private static readonly IReadOnlyList<string> NoToggleGuids = new List<string>
    {
        SpaceWarpPlugin.ModGuid,
        UitkForKsp2Plugin.ModGuid,
        ConfigurationManager.ConfigurationManager.GUID
    };

    private void Awake()
    {
        _listEntryTemplate = AssetManager.GetAsset<VisualTreeAsset>($"{SpaceWarpPlugin.ModGuid}/modlist/ui/modlist/modlistitem.uxml");
        _dependencyTemplate = AssetManager.GetAsset<VisualTreeAsset>($"{SpaceWarpPlugin.ModGuid}/modlist/ui/modlist/modlistdependency.uxml");
    }

    internal void AddMainMenuItem()
    {
        string term;

        if (SpaceWarpPlugin.Instance.ConfigShowMainMenuWarningForErroredMods.Value && SpaceWarpManager.ErroredPlugins?.Count > 0)
            term = "SpaceWarp/Mods/Errored";
        else if (SpaceWarpPlugin.Instance.ConfigShowMainMenuWarningForOutdatedMods.Value && SpaceWarpManager.ModsOutdated.ContainsValue(true))
            term = "SpaceWarp/Mods/Outdated";
        else
            term = "SpaceWarp/Mods";

        MainMenu.RegisterLocalizedMenuButton(term, ToggleWindow);
    }

    private void OnEnable()
    {
        if (_isLoaded)
        {
            return;
        }

        SetupDocument();
        InitializeElements();
        FillModLists();
        SetupToggles();
        SetupButtons();
        _isLoaded = true;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.M))
        {
            ToggleWindow();
        }

        if (_isWindowVisible && Input.GetKey(KeyCode.Escape))
        {
            HideWindow();
        }
    }

    private void SetupDocument()
    {
        var document = GetComponent<UIDocument>();
        if (document.TryGetComponent<DocumentLocalization>(out var localization))
        {
            localization.Localize();
        }
        else
        {
            document.EnableLocalization();
        }

        _container = document.rootVisualElement;
        _container[0].CenterByDefault();
        HideWindow();
    }

    private void InitializeElements()
    {
        // Register a callback for the back button
        _container.Q<Button>("back-button").RegisterCallback<ClickEvent>(_ => HideWindow());

        // Store references to the Mod list UI elements
        _enableAllButton = _container.Q<Button>("enable-all-button");
        _disableAllButton = _container.Q<Button>("disable-all-button");
        _revertChangesButton = _container.Q<Button>("revert-changes-button");
        _changesLabel = _container.Q<Label>("changes-label");

        _enabledModFoldout = _container.Q<Foldout>("spacewarp-mod-foldout");
        _enabledModList = _container.Q<VisualElement>("spacewarp-mod-list");


        _disabledModFoldout = _container.Q<Foldout>("disabled-mod-foldout");
        _disabledModList = _container.Q<VisualElement>("disabled-mod-list");

        _erroredModFoldout = _container.Q<Foldout>("errored-mod-foldout");
        _erroredModList = _container.Q<VisualElement>("errored-mod-list");

        _openModsFolderButton = _container.Q<Button>("open-mods-folder-button");
        _openConfigManagerButton = _container.Q<Button>("open-config-manager-button");

        // Store references to the selected mod details UI element references
        _detailsContainer = _container.Q<VisualElement>("details-container");
        _detailsNameLabel = _container.Q<Label>("details-name");
        _detailsIdLabel = _container.Q<Label>("details-id");
        _detailsAuthorLabel = _container.Q<Label>("details-author");
        _detailsVersionLabel = _container.Q<Label>("details-version");
        _detailsSourceLink = _container.Q<Button>("details-source");
        _detailsSourceLinkInitialBorderWidth = _detailsSourceLink.style.borderBottomWidth;
        _detailsDescriptionLabel = _container.Q<Label>("details-description");
        _detailsKspVersionLabel = _container.Q<Label>("details-ksp-version");

        _detailsOutdatedWarning = _container.Q<VisualElement>("details-outdated-warning");
        _detailsUnsupportedWarning = _container.Q<VisualElement>("details-unsupported-warning");
        _detailsDisabledWarning = _container.Q<VisualElement>("details-disabled-warning");
        _mismatchedIDWarning = _container.Q<VisualElement>("details-mismatched-id-error");
        _missingInfoWarning = _container.Q<VisualElement>("details-missing-info-error");
        _mismatchedVersionWarning = _container.Q<VisualElement>("details-mismatched-error");
        _badDirectoryWarning = _container.Q<VisualElement>("details-bad-directory-error");

        _detailsDependenciesFoldout = _container.Q<Foldout>("details-dependencies-foldout");
        _detailsDependenciesList = _container.Q<VisualElement>("details-dependencies-list");

        // Show only categories that have any mods in them
        if (SpaceWarpManager.AllPlugins.Count > 0)
        {
            _enabledModFoldout.style.display = DisplayStyle.Flex;
        }

        if (SpaceWarpManager.DisabledPlugins.Count > 0)
        {
            _disabledModFoldout.style.display = DisplayStyle.Flex;
        }

        if (SpaceWarpManager.ErroredPlugins.Count > 0)
        {
            _erroredModFoldout.style.display = DisplayStyle.Flex;
        }
    }

    private void FillModLists()
    {
        foreach (var plugin in SpaceWarpManager.AllPlugins)
        {
            MakeListItem(_enabledModList, data =>
            {
                data.Guid = plugin.Guid;
                data.SetInfo(plugin);

                if (SpaceWarpManager.ModsUnsupported[data.Guid])
                {
                    data.SetIsUnsupported();
                }
            });
        }

        foreach (var pluginInfo in SpaceWarpManager.DisabledPlugins)
        {
            MakeListItem(_disabledModList, data =>
            {
                data.Guid = pluginInfo.Guid;
                data.SetInfo(pluginInfo);
                data.SetIsDisabled();
            });
        }

        foreach (var erroredPlugin in SpaceWarpManager.ErroredPlugins)
        {
            MakeListItem(_erroredModList, data => {
                data.Guid = erroredPlugin.Plugin.Guid;
                erroredPlugin.Apply(data);
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

        var noToggleElements = _modItemElements.Where(pair => NoToggleGuids.Contains(pair.Key));
        foreach (var pair in noToggleElements)
        {
            pair.Value.Q<Toggle>().RemoveFromHierarchy();
        }
    }

    private void SetupButtons()
    {
        _enableAllButton.RegisterCallback<ClickEvent>(_ =>
        {
            _toggles = _toggles.Keys.ToDictionary(key => key, _ => true);
            UpdateToggles();
            UpdateChangesLabel();
            UpdateDisabledFile();
        });

        _disableAllButton.RegisterCallback<ClickEvent>(_ =>
        {
            _toggles = _toggles.Select(kv => (key: kv.Key, value: NoToggleGuids.Contains(kv.Key)))
                .ToDictionary(x => x.key, x => x.value);
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

        _detailsSourceLink.RegisterCallback<ClickEvent>(_ =>
        {
            var url = _detailsSourceLink.text?.Trim();
            if (!string.IsNullOrEmpty(url) && url.StartsWith("http"))
            {
                Application.OpenURL(url);
            }
        });

        _openModsFolderButton.RegisterCallback<ClickEvent>(_ =>
        {
            var explorer = new Process();
            explorer.StartInfo = new ProcessStartInfo("explorer.exe")
            {
                Arguments = $"\"{Paths.PluginPath}\""
            };
            explorer.Start();
        });

        var configManager = SpaceWarpManager.ConfigurationManager;
        _openConfigManagerButton.RegisterCallback<ClickEvent>(_ =>
        {
            configManager.DisplayingWindow = !configManager.DisplayingWindow;
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

        _modItemElements[data.Guid] = element;
        container.Add(element);
    }

    private void OnModSelected(EventBase evt)
    {
        if (evt.currentTarget is not TemplateContainer { userData: ModListItemController data })
        {
            return;
        }

        foreach (var modItem in _modItemElements.Values)
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

        if (data.Info != null)
        {
            SetSelectedModInfo(data,data.Info);
        }
        else
        {
            ClearSelected();
        }
    }

    private void SetSelectedModInfo(ModListItemController data, SpaceWarpPluginDescriptor descriptor)
    {

        //TODO: Add the ability to show errors at some point (Munix)
        SetSelected(
            descriptor.Name,
            data.HasBadID ? descriptor.SWInfo.ModID : data.Guid, // So that I can show a mismatched ID if it is mismatched
            descriptor.SWInfo.Author,
            descriptor.SWInfo.Version,
            descriptor.SWInfo.Source,
            descriptor.SWInfo.Description,
            descriptor.SWInfo.SupportedKsp2Versions.ToString(),
            descriptor.SWInfo.Dependencies
                ?.Select(dependencyInfo => (dependencyInfo.ID, dependencyInfo.Version.ToString()))
                .ToList(),
            data.IsOutdated,
            data.IsUnsupported,
            data.IsDisabled,
            data.HasBadID,
            data.MissingSWInfo,
            data.BadDirectory,
            data.HasMismatchedVersion,
            data.MissingDependencies,
            data.ErroredDependencies,
            data.UnsupportedDependencies,
            data.UnspecifiedDependencies,
            data.DisabledDependencies
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
        bool isMismatchedID = false,
        bool isMissingSWInfo = false,
        bool isBadDirectory = false,
        bool isMismatchedVersion = false,
        List<string> missingDependencies = null,
        List<string> erroredDependencies = null,
        List<string> unsupportedDependencies = null,
        List<string> unspecifiedDependencies = null,
        List<string> disabledDependencies = null,
        bool hasSelected = true
    )
    {
        _detailsContainer.visible = hasSelected;

        _detailsNameLabel.text = modName;
        _detailsIdLabel.text = id;
        _detailsAuthorLabel.text = author;
        _detailsVersionLabel.text = version;
        _detailsSourceLink.text = source;
        _detailsSourceLink.style.borderBottomWidth = string.IsNullOrEmpty(source) || !source.StartsWith("http")
            ? 0
            : _detailsSourceLinkInitialBorderWidth;
        _detailsDescriptionLabel.text = description;
        _detailsKspVersionLabel.text = kspVersion;

        SetDependencies(dependencies,
            missingDependencies,
            erroredDependencies,
            unsupportedDependencies,
            unspecifiedDependencies,
            disabledDependencies);
        SetModWarnings(isOutdated,
            isUnsupported,
            isDisabled,
            isMismatchedID,
            isMissingSWInfo,
            isBadDirectory,
            isMismatchedVersion);
    }

    private void SetDependencies(
        List<(string, string)> dependencies,
        List<string> missingDependencies,
        List<string> erroredDependencies,
        List<string> unsupportedDependencies,
        List<string> unspecifiedDependencies,
        List<string> disabledDependencies)
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
            var warning = dependencyElement.Q<Label>("WarningInformation");
            warning.style.display = DisplayStyle.None;
            if (missingDependencies.Contains(id))
            {
                warning.style.display = DisplayStyle.Flex;
                warning.text = $"({_missingDependency})";
            }

            if (erroredDependencies.Contains(id))
            {
                warning.style.display = DisplayStyle.Flex;
                warning.text = $"({_erroredDependency})";
            }

            if (unsupportedDependencies.Contains(id))
            {
                warning.style.display = DisplayStyle.Flex;
                warning.text = $"({_unsupportedDependency})";
            }

            if (unspecifiedDependencies.Contains(id))
            {
                warning.style.display = DisplayStyle.Flex;
                warning.text = $"({_unspecifiedDependency})";
            }

            if (disabledDependencies.Contains(id))
            {
                warning.style.display = DisplayStyle.Flex;
                warning.text = $"({_disabledDependency})";
            }


            _detailsDependenciesList.Add(dependencyElement);
        }
    }

    private void SetModWarnings(bool isOutdated,
        bool isUnsupported,
        bool isDisabled,
        bool isMismatchedID,
        bool isMissingSWInfo,
        bool isBadDirectory,
        bool isMismatchedVersion)
    {
        _detailsOutdatedWarning.style.display = isOutdated ? DisplayStyle.Flex : DisplayStyle.None;
        _detailsUnsupportedWarning.style.display = isUnsupported ? DisplayStyle.Flex : DisplayStyle.None;
        _detailsDisabledWarning.style.display = isDisabled ? DisplayStyle.Flex : DisplayStyle.None;
        _mismatchedIDWarning.style.display = isMismatchedID ? DisplayStyle.Flex : DisplayStyle.None;
        _missingInfoWarning.style.display = isMissingSWInfo ? DisplayStyle.Flex : DisplayStyle.None;
        _badDirectoryWarning.style.display = isBadDirectory ? DisplayStyle.Flex : DisplayStyle.None;
        _mismatchedVersionWarning.style.display = isMismatchedVersion ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void ClearSelected()
    {
        SetSelected(hasSelected: false);
    }

    private void UpdateToggles()
    {
        foreach (var element in _modItemElements.Values)
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

        if (numChanges == 1)
        {
            _changesLabel.style.display = DisplayStyle.Flex;
            _changesLabel.text = _singleChangeLabelText;
        }
        else if (numChanges > 1)
        {
            _changesLabel.style.display = DisplayStyle.Flex;
            _changesLabel.text = string.Format(_multipleChangesLabelText, numChanges);
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

    internal void UpdateOutdated(string guid, bool isOutdated)
    {
        if (isOutdated)
        {
            (_modItemElements[guid]?.userData as ModListItemController)?.SetIsOutdated();
        }
    }

    private void ToggleWindow()
    {
        _container.style.display = _isWindowVisible ? DisplayStyle.None : DisplayStyle.Flex;
        _isWindowVisible = !_isWindowVisible;
    }

    private void HideWindow()
    {
        _container.style.display = DisplayStyle.None;
        _isWindowVisible = false;
    }
}