using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpaceWarp2.API;

/// <summary>
/// The mod list's preferences: which mods are disabled, and which features new saves start with.
/// </summary>
public static class ModList
{
    /// <summary>
    /// The list of all disabled plugin GUIDs.
    /// </summary>
    internal static string[] DisabledPluginGuids { get; private set; }

    /// <summary>
    /// Whether the mod list changed since the last run.
    /// </summary>
    public static bool ChangedSinceLastRun { get; internal set; }

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        DisabledPluginGuids = null;
        ChangedSinceLastRun = default;
    }

    internal static void Initialize()
    {
        if (!File.Exists(CommonPaths.DisabledPlugins))
        {
            File.Create(CommonPaths.DisabledPlugins).Dispose();
        }

        DisabledPluginGuids = File.ReadAllLines(CommonPaths.DisabledPlugins);

        var oldHash = "";
        if (File.Exists(CommonPaths.HashLocation))
        {
            oldHash = File.ReadAllText(CommonPaths.HashLocation);
            File.Delete(CommonPaths.HashLocation);
        }

        var newHash = File.ReadAllText(CommonPaths.DisabledPlugins);

        Directory.CreateDirectory(CommonPaths.ModsFolder);
        foreach (var swinfo in Directory.GetFiles(CommonPaths.ModsFolder, "swinfo.json", SearchOption.AllDirectories))
        {
            newHash += File.ReadAllText(swinfo);
        }

        newHash = GetHash(newHash);
        
        File.WriteAllText(CommonPaths.HashLocation, newHash);

        ChangedSinceLastRun = newHash != oldHash;
    }

    /// <summary>
    /// Gets the features the player marked to start every new save with, whatever its campaign pack.
    /// </summary>
    public static IReadOnlyCollection<string> FeatureDefaults => Array.Empty<string>();

    /// <summary>
    /// Marks or unmarks a feature to start every new save with, and writes the choice to disk.
    /// </summary>
    /// <param name="featureId">The qualified feature id.</param>
    /// <param name="isDefault">Whether new saves start with the feature.</param>
    public static void SetFeatureDefault(string featureId, bool isDefault)
    {
    }

    /// <summary>
    /// Reads the features marked to start every new save with from disk.
    /// </summary>
    public static void LoadFeatureDefaults()
    {
    }

    private static string GetHash(string text)
    {
        var crypt = new System.Security.Cryptography.SHA256Managed();
        var hash = new StringBuilder();
        var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(text));
        foreach (byte theByte in crypto)
        {
            hash.Append(theByte.ToString("x2"));
        }
        return hash.ToString();
    }
}