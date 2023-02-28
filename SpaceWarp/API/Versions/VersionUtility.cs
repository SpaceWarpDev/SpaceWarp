namespace SpaceWarp.API.Versions;

public static class VersionUtility
{
    public static bool IsVersionAbove(string version, string toCheck)
    {
        if (version == "")
        {
            return true;
        }

        string[] semanticVersion = toCheck.Split('.');
        string[] requiredVersion = version.Split('.');
                
        for (int i = 0; i < requiredVersion.Length; i++)
        {
            if (requiredVersion[i] == "*")
            {
                continue;
            }

            if (int.Parse(semanticVersion[i]) < int.Parse(requiredVersion[i]))
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsVersionBellow(string version, string toCheck)
    {
        if (version == "")
        {
            return true;
        }

        string[] semanticVersion = toCheck.Split('.');
        string[] requiredVersion = version.Split('.');
            
        for (int i = 0; i < requiredVersion.Length; i++)
        {
            if (requiredVersion[i] == "*")
            {
                continue;
            }

            if (int.Parse(semanticVersion[i]) > int.Parse(requiredVersion[i]))
            {
                return false;
            }
        }

        return true;
    }
}