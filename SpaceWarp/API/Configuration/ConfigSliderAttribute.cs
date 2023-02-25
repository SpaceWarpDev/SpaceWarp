using System;

namespace SpaceWarp.API.Configuration
{
    public class ConfigSliderAttribute : Attribute
    {
        public string Name;
        public Object Minimum;
        public Object Maximum;
        public Object Step;

        public ConfigSliderAttribute(string name, object minimum, object maximum, object step)
        {
            Name = name;
            Minimum = minimum;
            Maximum = maximum;
            Step = step;
        }
    }
}