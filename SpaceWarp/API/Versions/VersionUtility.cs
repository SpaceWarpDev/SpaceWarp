using System;

namespace SpaceWarp.API.Versions;

public static class VersionUtility
{
    /// <summary>
    ///     Checks if one semantic version is newer than another
    /// </summary>
    /// <param name="version1">The first version</param>
    /// <param name="version2">The second version</param>
    /// <returns>version1 is newer than version2</returns>
    public static bool IsNewerThan(string version1, string version2)
    {
        return CompareSemanticVersionStrings(version1, version2) > 0;
    }

    /// <summary>
    ///     Checks if one semantic version is older than another
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

    /// <summary>
    ///     Compares 2 semantic versions
    /// </summary>
    /// <param name="version1">The first version</param>
    /// <param name="version2">The second version</param>
    /// <returns>0 if version1 equals version2, -1 if version1 is less than version2, 1 if version1 is greater than version2 </returns>
    public static int CompareSemanticVersionStrings(string version1, string version2)
    {
        var version1Parts = version1.Split('.');
        var version2Parts = version2.Split('.');

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

        return 0;
    }
}