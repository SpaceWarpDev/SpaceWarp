using JetBrains.Annotations;

namespace SpaceWarp.API.Messaging;

/// <summary>
/// A static class that manages the creation and retrieval of MessageBus instances.
/// </summary>
[PublicAPI]
public static class MessageBusManager
{
    private static Dictionary<string, MessageBusBase> _messagesBusesByName = new();

    /// <summary>
    /// Creates a new MessageBus instance with the given name.
    /// </summary>
    /// <param name="name">The name of the MessageBus to create.</param>
    /// <typeparam name="T">The type of the MessageBus to create.</typeparam>
    /// <returns>The created MessageBus instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the given name is null or empty.</exception>
    /// <exception cref="Exception">Thrown when a MessageBus with the given name already exists.</exception>
    public static T Add<T>(string name) where T : MessageBusBase, new()
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Null or empty MessageBus name");
        }

        if (_messagesBusesByName.ContainsKey(name))
        {
            throw new Exception($"MessageBus '{name}' exists already");
        }

        var messageBus = new T
        {
            Name = name
        };
        _messagesBusesByName.Add(name, messageBus);

        Modules.Messaging.Instance.ModuleLogger.LogDebug($"MessageBus '{name}' created");
        return messageBus;
    }

    /// <summary>
    /// Does a MessageBus with the given name exist?
    /// </summary>
    /// <param name="messageBusName">The name of the MessageBus to check.</param>
    /// <returns>True if a MessageBus with the given name exists, false otherwise.</returns>
    public static bool Exists(string messageBusName) => _messagesBusesByName.ContainsKey(messageBusName);

    /// <summary>
    /// Gets a MessageBus instance with the given name.
    /// </summary>
    /// <param name="messageBusName"></param>
    /// <param name="messageBus"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public static bool TryGet<T>(string messageBusName, out T messageBus) where T : MessageBusBase, new()
    {
        if (string.IsNullOrEmpty(messageBusName))
        {
            throw new ArgumentException("Null or empty MessageBus name");
        }

        if (!_messagesBusesByName.TryGetValue(messageBusName, out MessageBusBase messageBusBase))
        {
            messageBus = null;
            return false;
        }

        if (messageBusBase is not T @base)
        {
            throw new Exception(
                $"Message bus '{messageBusBase.Name}' is of type '{messageBusBase.GetType()}' but the " +
                $"requested type was '{typeof(T)}'"
            );
        }

        messageBus = @base;
        return true;
    }

    // Call this (potentially a bit heavy) method on some occasions, for example when a loading screen happens
    internal static void CheckForMemoryLeaks()
    {
        var memoryLeaks = 0;

        foreach (var messageBus in _messagesBusesByName.Values)
        {
            var handlers = messageBus.Handlers;

            for (var i = handlers.Count; i-- > 0;)
            {
                var target = handlers[i].Target;
                switch (target)
                {
                    // bypass UnityEngine.Object null equality overload
                    case null:
                        continue;
                    case UnityEngine.Object uObj when uObj == null:
                        Modules.Messaging.Instance.ModuleLogger.LogDebug(
                            $"Memory leak detected : a destroyed instance of the " +
                            $"'{target.GetType().Assembly.GetName().Name}:{target.GetType().Name}' class is holding " +
                            $"a '{messageBus.Name}' MessageBus handler"
                        );
                        messageBus.RemoveHandlerAt(i);
                        memoryLeaks++;
                        break;
                }
            }
        }

        if (memoryLeaks > 0)
        {
            Modules.Messaging.Instance.ModuleLogger.LogDebug($"{memoryLeaks} detected!");
        }
    }
}