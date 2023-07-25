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
}