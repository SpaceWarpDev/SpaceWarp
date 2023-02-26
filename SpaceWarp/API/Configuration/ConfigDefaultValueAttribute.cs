using System;

namespace SpaceWarp.API.Configuration
{
    public class ConfigDefaultValueAttribute : Attribute
    {
        public object DefaultValue;

        public ConfigDefaultValueAttribute(object defaultValue)
        {
            DefaultValue = defaultValue;
        }
    }
}