using System;
using System.Collections.Generic;

namespace SpaceWarp.API.Configuration
{
    public class ConfigurationManager
    {
        public static readonly Dictionary<string, (Type configType, object configObject)> ModConfigurations = new Dictionary<string, (Type configType, object configObject)>();
        


    }
}