using System;
using System.Collections.Generic;

// Disable warnings for missing Equals and GetHashCode implementations
#pragma warning disable CS0660, CS0661

namespace SpaceWarp.API.Versions;

/// <summary>
/// Extended version of semantic versioning (see https://semver.org/) that supports an unlimited amount of
/// version numbers.
/// </summary>
public class SemanticVersion : IComparable<SemanticVersion>
{
    #region Properties

    /// <summary>
    /// The major (first) version number.
    /// </summary>
    public int Major => VersionNumbers[0];

    /// <summary>
    /// The minor (second) version number.
    /// </summary>
    public int Minor => VersionNumbers[1];

    /// <summary>
    /// The patch (third) version number.
    /// </summary>
    public int Patch => VersionNumbers[2];

    /// <summary>
    /// List of all version numbers.
    /// </summary>
    public List<int> VersionNumbers { get; } = new();

    /// <summary>
    /// Prerelease identifiers.
    /// </summary>
    public string Prerelease { get; }

    /// <summary>
    /// Build metadata.
    /// </summary>
    public string Build { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new SemanticVersion object.
    /// </summary>
    /// <param name="major">Major version number</param>
    /// <param name="minor">Minor version number</param>
    /// <param name="patch">Patch version number</param>
    /// <param name="prerelease">Prerelease identifiers</param>
    /// <param name="build">Build metadata</param>
    public SemanticVersion(int major, int minor, int patch, string prerelease, string build)
    {
        VersionNumbers.Add(major);
        VersionNumbers.Add(minor);
        VersionNumbers.Add(patch);
        Prerelease = prerelease ?? string.Empty;
        Build = build ?? string.Empty;
    }

    /// <summary>
    /// Creates a new SemanticVersion object.
    /// </summary>
    /// <param name="major">Major version number</param>
    /// <param name="minor">Minor version number</param>
    /// <param name="patch">Patch version number</param>
    /// <param name="prerelease">Prerelease identifiers</param>
    public SemanticVersion(int major, int minor, int patch, string prerelease)
        : this(major, minor, patch, prerelease, string.Empty)
    {
    }

    /// <summary>
    /// Creates a new SemanticVersion object.
    /// </summary>
    /// <param name="major">Major version number</param>
    /// <param name="minor">Minor version number</param>
    /// <param name="patch">Patch version number</param>
    public SemanticVersion(int major, int minor, int patch)
        : this(major, minor, patch, string.Empty)
    {
    }

    /// <summary>
    /// Creates a new SemanticVersion object from a version string.
    /// </summary>
    /// <param name="version">Version string in the format "major.minor.patch[-prerelease][+build]"</param>
    /// <exception cref="ArgumentException">Thrown if the version string is invalid</exception>
    public SemanticVersion(string version)
    {
        if (string.IsNullOrEmpty(version))
        {
            throw new ArgumentException("Version string must not be null or empty.");
        }

        var originalVersion = version;

        var buildIndex = version.IndexOf('+');
        if (buildIndex != -1)
        {
            Build = version[(buildIndex + 1)..];
            version = version[..buildIndex];
        }

        var prereleaseIndex = version.IndexOf('-');
        if (prereleaseIndex != -1)
        {
            Prerelease = version[(prereleaseIndex + 1)..];
            version = version[..prereleaseIndex];
        }

        var versionParts = version.Split('.');
        if (versionParts.Length < 3)
        {
            throw new ArgumentException(
                $"Version string \"{originalVersion}\" must have at least 3 version numbers (major.minor.patch)."
            );
        }

        foreach (var part in versionParts)
        {
            if (int.TryParse(part, out var versionNumber))
            {
                VersionNumbers.Add(versionNumber);
            }
            else
            {
                throw new ArgumentException(
                    $"Invalid version number: \"{part}\" in version string \"{originalVersion}\"."
                );
            }
        }
    }

    #endregion

    #region Comparisons

    /// <summary>
    /// Compares this version to another version.
    /// </summary>
    /// <param name="other">The other version to compare to.</param>
    /// <returns>
    /// Negative if this version is less than the other version, 0 if they are equal, positive if this version is
    /// larger than the other version.
    /// </returns>
    public int CompareTo(SemanticVersion other)
    {
        /*
         * Precedence MUST be calculated by separating the version into major, minor, patch and pre-release identifiers
         * in that order (Build metadata does not figure into precedence).
         *
         * Precedence is determined by the first difference when comparing each of these identifiers from left to right
         * as follows: Major, minor, and patch versions are always compared numerically.
         *
         * Example: 1.0.0 < 2.0.0 < 2.1.0 < 2.1.1.
         *
         * When major, minor, and patch are equal, a pre-release version has lower precedence than a normal version:
         *
         * Example: 1.0.0-alpha < 1.0.0.
         *
         * Precedence for two pre-release versions with the same major, minor, and patch version MUST be determined by
         * comparing each dot separated identifier from left to right until a difference is found as follows:
         *
         * Identifiers consisting of only digits are compared numerically.
         *
         * Identifiers with letters or hyphens are compared lexically in ASCII sort order.
         *
         * Numeric identifiers always have lower precedence than non-numeric identifiers.
         *
         * A larger set of pre-release fields has a higher precedence than a smaller set, if all of the preceding
         * identifiers are equal.
         *
         * Example: 1.0.0-alpha < 1.0.0-alpha.1 < 1.0.0-alpha.beta < 1.0.0-beta < 1.0.0-beta.2 < 1.0.0-beta.11
         * < 1.0.0-rc.1 < 1.0.0.
         */

        // Compare the individual version numbers
        var maxLength = Math.Max(VersionNumbers.Count, other.VersionNumbers.Count);
        for (var i = 0; i < maxLength; i++)
        {
            var version1Part = i < VersionNumbers.Count ? VersionNumbers[i] : 0;
            var version2Part = i < other.VersionNumbers.Count ? other.VersionNumbers[i] : 0;

            if (version1Part > version2Part)
            {
                return 1;
            }

            if (version1Part < version2Part)
            {
                return -1;
            }
        }

        // If the version numbers are equal, compare the prerelease identifiers if at least one of them is not null,
        // otherwise the versions are considered equal
        if (Prerelease == null && other.Prerelease == null)
        {
            return 0;
        }

        if (Prerelease == null)
        {
            return 1;
        }

        if (other.Prerelease == null)
        {
            return -1;
        }

        // Both prerelease strings are not null, compare them
        var prereleaseParts1 = Prerelease.Split('.');
        var prereleaseParts2 = other.Prerelease.Split('.');
        var maxPrereleaseLength = Math.Max(prereleaseParts1.Length, prereleaseParts2.Length);
        for (var i = 0; i < maxPrereleaseLength; i++)
        {
            var prerelease1Part = i < prereleaseParts1.Length ? prereleaseParts1[i] : string.Empty;
            var prerelease2Part = i < prereleaseParts2.Length ? prereleaseParts2[i] : string.Empty;

            if (prerelease1Part == prerelease2Part)
            {
                continue;
            }

            var prerelease1PartIsNumeric = int.TryParse(prerelease1Part, out var prerelease1PartNumber);
            var prerelease2PartIsNumeric = int.TryParse(prerelease2Part, out var prerelease2PartNumber);

            if (prerelease1PartIsNumeric && prerelease2PartIsNumeric)
            {
                return prerelease1PartNumber - prerelease2PartNumber;
            }

            if (prerelease1PartIsNumeric)
            {
                return -1;
            }

            if (prerelease2PartIsNumeric)
            {
                return 1;
            }

            return string.Compare(prerelease1Part, prerelease2Part, StringComparison.Ordinal);
        }

        return 0;
    }

    /// <summary>
    /// Compares whether one version is less than another version.
    /// </summary>
    /// <param name="a">First version to compare.</param>
    /// <param name="b">Second version to compare.</param>
    /// <returns>True if a is less than b, false otherwise.</returns>
    public static bool operator <(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) < 0;

    /// <summary>
    /// Compares whether one version is greater than another version.
    /// </summary>
    /// <param name="a">First version to compare.</param>
    /// <param name="b">Second version to compare.</param>
    /// <returns>True if a is greater than b, false otherwise.</returns>
    public static bool operator >(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) > 0;

    /// <summary>
    /// Compares whether one version is less than or equal to another version.
    /// </summary>
    /// <param name="a">First version to compare.</param>
    /// <param name="b">Second version to compare.</param>
    /// <returns>True if a is less than or equal to b, false otherwise.</returns>
    public static bool operator <=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) <= 0;

    /// <summary>
    /// Compares whether one version is greater than or equal to another version.
    /// </summary>
    /// <param name="a">First version to compare.</param>
    /// <param name="b">Second version to compare.</param>
    /// <returns>True if a is greater than or equal to b, false otherwise.</returns>
    public static bool operator >=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) >= 0;

    /// <summary>
    /// Compares whether one version is equal to another version.
    /// </summary>
    /// <param name="a">First version to compare.</param>
    /// <param name="b">Second version to compare.</param>
    /// <returns>True if a is equal to b, false otherwise.</returns>
    public static bool operator ==(SemanticVersion a, SemanticVersion b) =>
        (a is null && b is null) || (a is not null && b is not null && a.CompareTo(b) == 0);

    /// <summary>
    /// Compares whether one version is not equal to another version.
    /// </summary>
    /// <param name="a">First version to compare.</param>
    /// <param name="b">Second version to compare.</param>
    /// <returns>True if a is not equal to b, false otherwise.</returns>
    public static bool operator !=(SemanticVersion a, SemanticVersion b) => !(a == b);

    #endregion

    #region String conversion

    /// <summary>
    /// Returns the version as a string.
    /// </summary>
    /// <returns>Version string.</returns>
    public override string ToString()
    {
        var versionString = $"{Major}.{Minor}.{Patch}";
        if (!string.IsNullOrEmpty(Prerelease))
        {
            versionString += $"-{Prerelease}";
        }

        if (!string.IsNullOrEmpty(Build))
        {
            versionString += $"+{Build}";
        }

        return versionString;
    }

    /// <summary>
    /// Implicitly converts a SemanticVersion object to a string.
    /// </summary>
    /// <param name="version">SemanticVersion object to convert.</param>
    /// <returns>Version string.</returns>
    public static implicit operator string(SemanticVersion version) => version.ToString();

    /// <summary>
    /// Explicitly converts a string to a SemanticVersion object.
    /// </summary>
    /// <param name="version">Version string to convert.</param>
    /// <returns>SemanticVersion object.</returns>
    public static explicit operator SemanticVersion(string version) => new(version);

    #endregion
}