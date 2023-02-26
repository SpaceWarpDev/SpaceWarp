using System;

namespace SpaceWarp.API.Configuration
{
    public class ConfigSectionAttribute : Attribute
    {
        public string Path;

        public ConfigSectionAttribute(string path)
        {
            Path = path;
        }
    }
}