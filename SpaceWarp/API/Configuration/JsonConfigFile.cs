using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SpaceWarp.API.Configuration;

public class JsonConfigFile : IConfigFile
{
    [CanBeNull] private JObject _previousConfigObject = null;


    private Dictionary<string, Dictionary<string, JsonConfigEntry>> _currentEntries = new();
    private readonly string _file;


    public JsonConfigFile(string file)
    {
        // Use .cfg as this is going to have comments and that will be an issue
        if (File.Exists(file))
        {
            try
            {
                _previousConfigObject = JObject.Parse(File.ReadAllText(file));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in attempting to load previous config file at '{file}': {e}");
                // ignored
            }
        }

        _file = file;
    }
    
    public void Save()
    {
        if (!_currentEntries.Any(value => value.Value.Count > 0)) return;
        var result = new StringBuilder();
        result.AppendLine("{");
        var hadPreviousSection = false;
        foreach (var section in _currentEntries.Where(section => section.Value.Count > 0))
        {
            hadPreviousSection = DumpSection(hadPreviousSection, result, section);
        }
        result.AppendLine("\n}");
        File.WriteAllText(_file, result.ToString());
    }

    private static bool DumpSection(bool hadPreviousSection, StringBuilder result, KeyValuePair<string, Dictionary<string, JsonConfigEntry>> section)
    {
        if (hadPreviousSection)
        {
            result.AppendLine(",");
        }

        result.AppendLine($"    \"{section.Key.Replace("\"", "\\\"").Replace("\n", "\\\n")}\": {{");
        var hadPreviousKey = false;
        foreach (var entry in section.Value)
        {
            hadPreviousKey = DumpEntry(result, hadPreviousKey, entry);
        }

        result.Append("\n    }");
        return true;
    }

    private static bool DumpEntry(StringBuilder result, bool hadPreviousKey, KeyValuePair<string, JsonConfigEntry> entry)
    {
        if (hadPreviousKey)
        {
            result.AppendLine(",");
        }

        // result.AppendLine($"        // {entry.Value.Description}");
        if (entry.Value.Description != "")
        {
            var descriptionLines = entry.Value.Description.Split('\n').Select(x => x.TrimEnd());
            foreach (var line in descriptionLines)
            {
                result.AppendLine($"        // {line}");
            }
        }

        var serialized = JsonConvert.SerializeObject(entry.Value.Value, Formatting.Indented);
        var serializedLines = serialized.Split('\n').Select(x => x.TrimEnd()).ToArray();
        if (serializedLines.Length > 1)
        {
            result.AppendLine($"        \"{entry.Key.Replace("\"", "\\\"").Replace("\n", "\\\n")}\": ");
            for (var i = 0; i < serializedLines.Length; i++)
            {
                if (i != serializedLines.Length - 1)
                {
                    result.AppendLine($"        {serializedLines[i]}");
                }
                else
                {
                    result.Append($"        {serializedLines[i]}");
                }
            }
        }
        else
        {
            result.Append($"        \"{entry.Key.Replace("\"", "\\\"").Replace("\n", "\\\n")}\": {serializedLines[0]}");
        }

        return true;
    }

    public IConfigEntry this[string section, string key] => _currentEntries[section][key];

    public IConfigEntry Bind<T>(string section, string key, T defaultValue = default, string description = "")
    {
        // So now we have to check if its already bound, and/or if the previous config object has it
        if (!_currentEntries.TryGetValue(section, out var previousSection))
        {
            previousSection = new Dictionary<string, JsonConfigEntry>();
            _currentEntries.Add(section,previousSection);
        }

        if (previousSection.TryGetValue(key, out var result))
        {
            return result;
        }

        if (_previousConfigObject != null && _previousConfigObject.TryGetValue(section, out var sect))
        {
            try
            {
                if (sect is JObject obj && obj.TryGetValue(key, out var value))
                {
                    var previousValue = value.ToObject(typeof(T));
                    previousSection[key] = new JsonConfigEntry(this, typeof(T), description, previousValue);
                }
                else
                {
                    previousSection[key] = new JsonConfigEntry(this, typeof(T), description, defaultValue);
                }
            }
            catch
            {
                previousSection[key] = new JsonConfigEntry(this, typeof(T), description, defaultValue);
                // ignored
            }
        } 
        else
        {
            previousSection[key] = new JsonConfigEntry(this, typeof(T), description, defaultValue);
        }
        
        Save();
        return previousSection[key];
    }

    public IReadOnlyList<string> Sections => _currentEntries.Keys.ToList();

    public IReadOnlyList<string> this[string section] => _currentEntries[section].Keys.ToList();
}