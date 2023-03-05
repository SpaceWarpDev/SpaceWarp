namespace SpaceWarp.API.Versions;

public static class VersionUtility
{
    /// <summary>
    /// Checks if one semantic version is above another
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

        string[] semanticVersion = toCheck.Split('.');
        string[] requiredVersion = version.Split('.');

        return !requiredVersion.Where((t, i) => t != "*" && int.Parse(semanticVersion[i]) < int.Parse(t)).Any();
    }

    /// <summary>
    /// Checks if one semantic version is below another
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

        string[] semanticVersion = toCheck.Split('.');
        string[] requiredVersion = version.Split('.');

        return !requiredVersion.Where((t, i) => t != "*" && int.Parse(semanticVersion[i]) > int.Parse(t)).Any();
    }
}