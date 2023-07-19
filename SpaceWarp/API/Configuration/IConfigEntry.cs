using System;

namespace SpaceWarp.API.Configuration;

public interface IConfigEntry
{
    public object Value { get; set; }
    public Type ValueType { get; }
    public T Get<T>() where T : class;
    public void Set<T>(T value);

    public string Description { get; }
}