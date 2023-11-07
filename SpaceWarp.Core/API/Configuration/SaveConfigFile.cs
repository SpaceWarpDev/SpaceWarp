using System.Collections.Generic;
using JetBrains.Annotations;

namespace SpaceWarp.API.Configuration;

public class SaveConfigFile : IConfigFile
{
    [CanBeNull] private JsonConfigFile _internalConfigFile;
    internal Dictionary<string, Dictionary<string, SaveConfigEntry>> CurrentEntries = new();
    
    public JsonConfigFile CurrentConfigFile
    {
        get => _internalConfigFile;
        set
        {
            if (_internalConfigFile == value) return;
            Save();
            _internalConfigFile = value;
            RebindConfigFile();
        }
    }

    private void RebindConfigFile()
    {
        foreach (var subEntry in CurrentEntries.SelectMany(entry => entry.Value))
        {
            if (_internalConfigFile != null)
                subEntry.Value.Bind(_internalConfigFile);
            else
                subEntry.Value.Reset();
        }
    }
    public void Save()
    {
        if (CurrentConfigFile != null)
        {
            CurrentConfigFile.Save();
        }
    }

    public IConfigEntry this[string section, string key] => CurrentEntries[section][key];

    public IConfigEntry Bind<T>(string section, string key, T defaultValue = default, string description = "")
    {
        return Bind(section, key, defaultValue, description, null);
    }

    public IConfigEntry Bind<T>(string section, string key, T defaultValue, string description, IValueConstraint valueConstraint)
    {
        if (!CurrentEntries.TryGetValue(section, out var previousSection))
        {
            previousSection = new Dictionary<string, SaveConfigEntry>();
            CurrentEntries.Add(section,previousSection);
        }
        if (previousSection.TryGetValue(key, out var result))
        {
            return result;
        }

        var entry = new SaveConfigEntry(section, key, description, typeof(T), defaultValue, valueConstraint);
        if (_internalConfigFile != null) entry.Bind(_internalConfigFile);
        previousSection[key] = entry;
        return entry;
    }

    public IReadOnlyList<string> Sections => CurrentEntries.Keys.ToList();

    public IReadOnlyList<string> this[string section] => CurrentEntries[section].Keys.ToList();

    public void ResetToDefaults()
    {
        CurrentConfigFile = null;
    }
}