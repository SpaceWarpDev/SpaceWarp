using JetBrains.Annotations;

namespace SpaceWarp.API.Messaging;

/// <summary>
/// A base class for MessageBus instances.
/// </summary>
[PublicAPI]
public abstract class MessageBusBase
{
    /// <summary>
    /// The name of the MessageBus.
    /// </summary>
    public string Name { get; internal set; }
    internal abstract IReadOnlyList<Delegate> Handlers { get; }
    internal abstract void RemoveHandlerAt(int index);
}