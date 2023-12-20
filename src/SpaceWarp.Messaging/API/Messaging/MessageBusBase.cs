using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SpaceWarp.API.Messaging;

[PublicAPI]
public abstract class MessageBusBase
{
    public string Name { get; internal set; }
    internal abstract IReadOnlyList<Delegate> Handlers { get; }
    internal abstract void RemoveHandlerAt(int index);
}