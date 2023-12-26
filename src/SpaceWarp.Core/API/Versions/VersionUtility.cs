using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace SpaceWarp.API.Versions;

[PublicAPI]
public static class VersionUtility
{
    /// <summary>
    /// Checks if one semantic version is newer than another
    /// </summary>
    /// <param name="version1">The first version</param>
    /// <param name="version2">The second version</param>
    /// <returns>version1 is newer than version2</returns>
    public static bool IsNewerThan(string version1, string version2)
    {
        return CompareSemanticVersionStrings(version1, version2) > 0;
    }

    /// <summary>
    /// Checks if one semantic version is older than another
    /// </summary>
    /// <param name="version1">The first version</param>
    /// <param name="version2">The second version</param>
    /// <returns>version1 is older than version2</returns>
    public static bool IsOlderThan(string version1, string version2)
    {
        return CompareSemanticVersionStrings(version1, version2) < 0;
    }

    public static bool IsSupported(string version, string min, string max)
    {
        return !IsOlderThan(version, min) && !IsNewerThan(version, max);
    }

    private static Regex _toClear = new("[^0-9.*]");
    private static string PreprocessSemanticVersion(string semver) => _toClear.Replace(semver, "");


    /// <summary>
    /// Compares 2 semantic versions
    /// </summary>
    /// <param name="version1">The first version</param>
    /// <param name="version2">The second version</param>
    /// <returns>
    /// 0 if version1 equals version2, -1 if version1 is less than version2, 1 if version1 is greater than version2
    /// </returns>
    public static int CompareSemanticVersionStrings(string version1, string version2)
    {
        var semanticVersion1 = PreprocessSemanticVersion(version1);
        var semanticVersion2 = PreprocessSemanticVersion(version2);
        Console.WriteLine($"version 1: {version1} -> {semanticVersion1}");
        Console.WriteLine($"version 2: {version2} -> {semanticVersion2}");
        var version1Parts = semanticVersion1.Split('.');
        var version2Parts = semanticVersion2.Split('.');

        var maxLength = Math.Max(version1Parts.Length, version2Parts.Length);

        for (var i = 0; i < maxLength; i++)
        {
            var version1PartString = i < version1Parts.Length ? version1Parts[i] : "0";
            var version2PartString = i < version2Parts.Length ? version2Parts[i] : "0";

            if (version1PartString == "*" || version2PartString == "*")
            {
                break;
            }

            var version1Part = int.Parse(version1PartString);
            var version2Part = int.Parse(version2PartString);

            if (version1Part > version2Part)
            {
                return 1;
            }

            if (version1Part < version2Part)
            {
                return -1;
            }
        }

        return ComparePrereleaseSemanticVersions(version1, version2);
    }

    private static Regex _prereleaseVersion = new(@"(\D+)(\d+)");

    private static int ComparePrereleaseSemanticVersions(string version1, string version2)
    {
        var dash1 = version1.IndexOf("-", StringComparison.Ordinal);
        var dash2 = version2.IndexOf("-", StringComparison.Ordinal);
        if (dash1 == -1 && dash2 == -1)
        {
            return 0;
        }

        if (dash1 == -1)
        {
            return 1;
        }

        if (dash2 == -1)
        {
            return -1;
        }

        var alphaVersion1 = version1[(dash1 + 1)..];
        var alphaVersion2 = version2[(dash2 + 1)..];
        // So now we get the numerics from the end of here
        int alphaVersionNumber1 = 0;
        int alphaVersionNumber2 = 0;
        var alphaVersionName1 = alphaVersion1;
        var alphaVersionName2 = alphaVersion2;
        if (_prereleaseVersion.IsMatch(alphaVersion1))
        {
            var match = _prereleaseVersion.Match(alphaVersion1);
            var name = match.Groups[1];
            var number = match.Groups[2];
            alphaVersionNumber1 = int.Parse(number.Value);
            alphaVersionName1 = name.Value;
        }
        
        if (_prereleaseVersion.IsMatch(alphaVersion2))
        {
            var match = _prereleaseVersion.Match(alphaVersion2);
            var name = match.Groups[1];
            var number = match.Groups[2];
            alphaVersionNumber2 = int.Parse(number.Value);
            alphaVersionName2 = name.Value;
        }

        var comparison = string.CompareOrdinal(alphaVersionName1, alphaVersionName2);
        if (comparison == 0)
        {
            if (alphaVersionNumber1 > alphaVersionNumber2)
            {
                return 1;
            } else if (alphaVersionNumber1 == alphaVersionNumber2)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
        return comparison;
    }
}