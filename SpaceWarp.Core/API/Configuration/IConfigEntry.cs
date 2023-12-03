using System;
using JetBrains.Annotations;

namespace SpaceWarp.API.Configuration;

[PublicAPI]
public interface IConfigEntry
{
    public object Value { get; set; }
    public Type ValueType { get; }
    public T Get<T>() where T : class;
    public void Set<T>(T value);

    public string Description { get; }

    public IValueConstraint Constraint { get; }
    
    /// <summary>
    /// Called when setting the value on a config file
    /// </summary>
    /// <param name="valueChangedCallback">An action that takes te old value and the new value and calls a callback</param>
    public void RegisterCallback(Action<object, object> valueChangedCallback);
}