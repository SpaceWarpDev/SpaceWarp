using JetBrains.Annotations;
using Newtonsoft.Json;
using SpaceWarp.API.Mods.JSON.Converters;

namespace SpaceWarp.API.Mods.JSON;

/// <summary>
/// Represents the version of the API specification from the swinfo.json file.
/// </summary>
[JsonConverter(typeof(SpecVersionConverter))]
[PublicAPI]
public sealed record SpecVersion
{
    private const int DefaultMajor = 1;
    private const int DefaultMinor = 0;

    /// <summary>
    /// Major version number.
    /// </summary>
    public int Major { get; } = DefaultMajor;

    /// <summary>
    /// Minor version number.
    /// </summary>
    public int Minor { get; } = DefaultMinor;

    // ReSharper disable InconsistentNaming

    /// <summary>
    /// Specification version 1.0 (SpaceWarp &lt; 1.2) - used if "spec" is not specified in the swinfo.json file.
    /// </summary>
    public static SpecVersion Default { get; } = new();

    /// <summary>
    /// Specification version 1.2 (SpaceWarp 1.2.x) - replaces SpaceWarp's proprietary ModID with BepInEx plugin GUID.
    /// </summary>
    public static SpecVersion V1_2 { get; } = new(1, 2);

    /// <summary>
    /// Specification version 1.3 (SpaceWarp 1.3.x) - adds back the ModID field, but enforces that it is the same
    /// as the BepInEx plugin GUID if there is a BepInEx plugin attached to the swinfo, since w/ version 1.3 there
    /// does not have to be a plugin.
    /// Also enforces that all dependencies use BepInEx GUID, and that they are loaded.
    /// </summary>
    public static SpecVersion V1_3 { get; } = new(1, 3);

    /// <summary>
    /// Specification version 2.0 (SpaceWarp 1.5.x and 2.0.x) - removes support for version checking from .csproj files,
    ///
    /// </summary>
    public static SpecVersion V2_0 { get; } = new(2, 0);

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

    /// <summary>
    /// Returns the string representation of the version in the format "major.minor".
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{Major}.{Minor}";

    /// <summary>
    /// Returns true if the first version is less than the second version.
    /// </summary>
    /// <param name="a">First version</param>
    /// <param name="b">Second version</param>
    /// <returns>True if the first version is less than the second version</returns>
    public static bool operator <(SpecVersion a, SpecVersion b) => Compare(a, b) < 0;

    /// <summary>
    /// Returns true if the first version is greater than the second version.
    /// </summary>
    /// <param name="a">First version</param>
    /// <param name="b">Second version</param>
    /// <returns>True if the first version is greater than the second version</returns>
    public static bool operator >(SpecVersion a, SpecVersion b) => Compare(a, b) > 0;

    /// <summary>
    /// Returns true if the first version is less than or equal to the second version.
    /// </summary>
    /// <param name="a">First version</param>
    /// <param name="b">Second version</param>
    /// <returns>True if the first version is less than or equal to the second version</returns>
    public static bool operator <=(SpecVersion a, SpecVersion b) => Compare(a, b) <= 0;

    /// <summary>
    /// Returns true if the first version is greater than or equal to the second version.
    /// </summary>
    /// <param name="a">First version</param>
    /// <param name="b">Second version</param>
    /// <returns>True if the first version is greater than or equal to the second version</returns>
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
[PublicAPI]
public sealed class InvalidSpecVersionException : Exception
{
    /// <summary>
    /// Creates a new InvalidSpecVersionException.
    /// </summary>
    /// <param name="version">Invalid version string</param>
    public InvalidSpecVersionException(string version) :
        base($"Invalid spec version: {version}. The correct format is \"major.minor\".")
    {
    }
}

/// <summary>
/// Thrown if a property is deprecated in the current specification version.
/// </summary>
[PublicAPI]
public sealed class DeprecatedSwinfoPropertyException : Exception
{
    /// <summary>
    /// Creates a new DeprecatedSwinfoPropertyException.
    /// </summary>
    /// <param name="property">Deprecated property name</param>
    /// <param name="deprecationVersion">Specification version in which the property was deprecated</param>
    public DeprecatedSwinfoPropertyException(string property, SpecVersion deprecationVersion) :
        base(
            $"The swinfo.json property \"{property}\" is deprecated in the spec version {deprecationVersion} and will be removed completely in the future."
        )
    {
    }
}