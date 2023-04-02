using System;

namespace SpaceWarp.API.Versions;

public static class VersionUtility
{
    /// <summary>
    ///     Checks if one semantic version is above another
    /// </summary>
    /// <param name="version">The version to check against</param>
    /// <param name="toCheck">The version that is being checked</param>
    /// <returns>toCheck >= version</returns>
    public static bool IsVersionAbove(string version, string toCheck)
    {
        if (version == "")
        {
            return true;
        }

        var semanticVersion = toCheck.Split('.');
        var requiredVersion = version.Split('.');

        return !requiredVersion.Where((t, i) => t != "*" && int.Parse(semanticVersion[i]) < int.Parse(t)).Any();
    }

    /// <summary>
    ///     Checks if one semantic version is below another
    /// </summary>
    /// <param name="version">The version to check against</param>
    /// <param name="toCheck">The version that is being checked</param>
    /// <returns>toCheck is less than or equal to version</returns>
    public static bool IsVersionBelow(string version, string toCheck)
    {
        if (version == "")
        {
            return true;
        }

        var semanticVersion = toCheck.Split('.');
        var requiredVersion = version.Split('.');

        return !requiredVersion.Where((t, i) => t != "*" && int.Parse(semanticVersion[i]) > int.Parse(t)).Any();
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