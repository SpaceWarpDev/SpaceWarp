using System;
using Newtonsoft.Json;
using SpaceWarp.API.Mods.JSON.Converters;

namespace SpaceWarp.API.Mods.JSON;

/// <summary>
/// Represents the version of the API specification from the swinfo.json file.
/// </summary>
[JsonConverter(typeof(SpecVersionConverter))]
public sealed record SpecVersion
{
    private const int DefaultMajor = 1;
    private const int DefaultMinor = 0;

    public int Major { get; } = DefaultMajor;
    public int Minor { get; } = DefaultMinor;

    // ReSharper disable InconsistentNaming

    /// <summary>
    /// Specification version 1.0 (SpaceWarp &lt; 1.2), used if "spec" is not specified in the swinfo.json file.
    /// </summary>
    public static SpecVersion Default { get; } = new();

    /// <summary>
    /// Specification version 1.2 (SpaceWarp 1.2.x), replaces SpaceWarp's proprietary ModID with BepInEx plugin GUID.
    /// </summary>
    public static SpecVersion V1_2 { get; } = new(1, 2);

    /// <summary>
    /// Specification version 1.3 (SpaceWarp 1.3.x), adds back the ModID field, but enforces that it is the same as the BepInEx plugin GUID if there is a BepInEx plugin attached to the swinfo
    /// Which w/ version 1.3 there does not have to be a plugin
    /// Also enforces that all dependencies use BepInEx GUID, and that they are loaded
    /// </summary>
    public static SpecVersion V1_3 { get; } = new(1, 3);

    // ReSharper restore InconsistentNaming

    /// <summary>
    /// Creates a new SpecVersion object with the version "major.minor".
    /// </summary>
    /// <param name="major">Major version number</param>
    /// <param name="minor">Minor version number</param>
    public SpecVersion(int major, int minor)
    {
        Major = major;
        Minor = minor;
    }

    /// <summary>
    /// Creates a new SpecVersion object from a string.
    /// </summary>
    /// <param name="version">Specification version in the format "major.minor"</param>
    /// <exception cref="InvalidSpecVersionException">Thrown if the string format is invalid</exception>
    public SpecVersion(string version = null)
    {
        if (version == null)
        {
            return;
        }

        var split = version.Split('.');
        if (split.Length != 2 || !int.TryParse(split[0], out var major) || !int.TryParse(split[1], out var minor))
        {
            throw new InvalidSpecVersionException(version);
        }

        Major = major;
        Minor = minor;
    }

    public override string ToString() => $"{Major}.{Minor}";

    public static bool operator <(SpecVersion a, SpecVersion b) => Compare(a, b) < 0;
    public static bool operator >(SpecVersion a, SpecVersion b) => Compare(a, b) > 0;
    public static bool operator <=(SpecVersion a, SpecVersion b) => Compare(a, b) <= 0;
    public static bool operator >=(SpecVersion a, SpecVersion b) => Compare(a, b) >= 0;

    private static int Compare(SpecVersion a, SpecVersion b)
    {
        if (a.Major != b.Major)
        {
            return a.Major - b.Major;
        }

        return a.Minor - b.Minor;
    }
}

/// <summary>
/// Thrown if the specification version string is invalid.
/// </summary>
public sealed class InvalidSpecVersionException : Exception
{
    public InvalidSpecVersionException(string version) : base(
        $"Invalid spec version: {version}. The correct format is \"major.minor\".")
    {
    }
}

/// <summary>
/// Thrown if a property is deprecated in the current specification version.
/// </summary>
public sealed class DeprecatedSwinfoPropertyException : Exception
{
    public DeprecatedSwinfoPropertyException(string property, SpecVersion deprecationVersion) : base(
        $"The swinfo.json property \"{property}\" is deprecated in the spec version {deprecationVersion} and will be removed completely in the future."
    )
    {
    }
}