using JetBrains.Annotations;

namespace SpaceWarp.API.Messaging;

/// <summary>
/// MessageBus that can be used to publish messages with no arguments to subscribers.
/// </summary>
[PublicAPI]
public class MessageBus : MessageBusBase
{
    private readonly List<Action> _handlers = new();
    internal override IReadOnlyList<Delegate> Handlers => _handlers;
    internal override void RemoveHandlerAt(int index) => _handlers.RemoveAt(index);

    /// <summary>
    /// Subscribes a handler to the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Subscribe(Action handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException();
        }

        _handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a handler from the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(Action handler)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            if (_handlers[i] == handler)
            {
                _handlers.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    public void Publish()
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            try
            {
                _handlers[i].Invoke();
            }
            catch (Exception e)
            {
                Modules.Messaging.Instance.ModuleLogger.LogDebug($"Error handling message '{Name}' : {e}");
            }
        }
    }
}

/// <summary>
/// MessageBus that can be used to publish messages with 1 argument to subscribers.
/// </summary>
[PublicAPI]
public class MessageBus<T1> : MessageBusBase
{
    private readonly List<Action<T1>> _handlers = new();
    internal override IReadOnlyList<Delegate> Handlers => _handlers;
    internal override void RemoveHandlerAt(int index) => _handlers.RemoveAt(index);

    /// <summary>
    /// Subscribes a handler to the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Subscribe(Action<T1> handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException();
        }

        _handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a handler from the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(Action<T1> handler)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            if (_handlers[i] == handler)
            {
                _handlers.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    public void Publish(T1 arg0)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            try
            {
                _handlers[i].Invoke(arg0);
            }
            catch (Exception e)
            {
                Modules.Messaging.Instance.ModuleLogger.LogDebug($"Error handling message '{Name}' : {e}");
            }
        }
    }
}

/// <summary>
/// MessageBus that can be used to publish messages with 2 argument to subscribers.
/// </summary>
[PublicAPI]
public class MessageBus<T1, T2> : MessageBusBase
{
    private readonly List<Action<T1, T2>> _handlers = new();
    internal override IReadOnlyList<Delegate> Handlers => _handlers;
    internal override void RemoveHandlerAt(int index) => _handlers.RemoveAt(index);

    /// <summary>
    /// Subscribes a handler to the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Subscribe(Action<T1, T2> handler)
    {
        if (handler == null)
            throw new ArgumentNullException();

        _handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a handler from the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(Action<T1, T2> handler)
    {
        for (int i = _handlers.Count; i-- > 0;)
            if (_handlers[i] == handler)
                _handlers.RemoveAt(i);
    }

    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    public void Publish(T1 arg0, T2 arg1)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            try
            {
                _handlers[i].Invoke(arg0, arg1);
            }
            catch (Exception e)
            {
                Modules.Messaging.Instance.ModuleLogger.LogDebug($"Error handling message '{Name}' : {e}");
            }
        }
    }
}

/// <summary>
/// MessageBus that can be used to publish messages with 3 arguments to subscribers.
/// </summary>
[PublicAPI]
public class MessageBus<T1, T2, T3> : MessageBusBase
{
    private readonly List<Action<T1, T2, T3>> _handlers = new();
    internal override IReadOnlyList<Delegate> Handlers => _handlers;
    internal override void RemoveHandlerAt(int index) => _handlers.RemoveAt(index);

    /// <summary>
    /// Subscribes a handler to the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Subscribe(Action<T1, T2, T3> handler)
    {
        if (handler == null)
            throw new ArgumentNullException();

        _handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a handler from the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(Action<T1, T2, T3> handler)
    {
        for (var i = _handlers.Count; i-- > 0;)
            if (_handlers[i] == handler)
                _handlers.RemoveAt(i);
    }

    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    public void Publish(T1 arg0, T2 arg1, T3 arg2)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            try
            {
                _handlers[i].Invoke(arg0, arg1, arg2);
            }
            catch (Exception e)
            {
                Modules.Messaging.Instance.ModuleLogger.LogDebug($"Error handling message '{Name}' : {e}");
            }
        }
    }
}

/// <summary>
/// MessageBus that can be used to publish messages with 4 arguments to subscribers.
/// </summary>
[PublicAPI]
public class MessageBus<T1, T2, T3, T4> : MessageBusBase
{
    private readonly List<Action<T1, T2, T3, T4>> _handlers = new();
    internal override IReadOnlyList<Delegate> Handlers => _handlers;
    internal override void RemoveHandlerAt(int index) => _handlers.RemoveAt(index);

    /// <summary>
    /// Subscribes a handler to the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Subscribe(Action<T1, T2, T3, T4> handler)
    {
        if (handler == null)
            throw new ArgumentNullException();

        _handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a handler from the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(Action<T1, T2, T3, T4> handler)
    {
        for (var i = _handlers.Count; i-- > 0;)
            if (_handlers[i] == handler)
                _handlers.RemoveAt(i);
    }

    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    public void Publish(T1 arg0, T2 arg1, T3 arg2, T4 arg3)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            try
            {
                _handlers[i].Invoke(arg0, arg1, arg2, arg3);
            }
            catch (Exception e)
            {
                Modules.Messaging.Instance.ModuleLogger.LogDebug($"Error handling message '{Name}' : {e}");
            }
        }
    }
}

/// <summary>
/// MessageBus that can be used to publish messages with 5 arguments to subscribers.
/// </summary>
[PublicAPI]
public class MessageBus<T1, T2, T3, T4, T5> : MessageBusBase
{
    private readonly List<Action<T1, T2, T3, T4, T5>> _handlers = new();
    internal override IReadOnlyList<Delegate> Handlers => _handlers;
    internal override void RemoveHandlerAt(int index) => _handlers.RemoveAt(index);

    /// <summary>
    /// Subscribes a handler to the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Subscribe(Action<T1, T2, T3, T4, T5> handler)
    {
        if (handler == null)
            throw new ArgumentNullException();

        _handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a handler from the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(Action<T1, T2, T3, T4, T5> handler)
    {
        for (var i = _handlers.Count; i-- > 0;)
            if (_handlers[i] == handler)
                _handlers.RemoveAt(i);
    }

    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    public void Publish(T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            try
            {
                _handlers[i].Invoke(arg0, arg1, arg2, arg3, arg4);
            }
            catch (Exception e)
            {
                Modules.Messaging.Instance.ModuleLogger.LogDebug($"Error handling message '{Name}' : {e}");
            }
        }
    }
}

/// <summary>
/// MessageBus that can be used to publish messages with 6 arguments to subscribers.
/// </summary>
[PublicAPI]
public class MessageBus<T1, T2, T3, T4, T5, T6> : MessageBusBase
{
    private readonly List<Action<T1, T2, T3, T4, T5, T6>> _handlers = new();
    internal override IReadOnlyList<Delegate> Handlers => _handlers;
    internal override void RemoveHandlerAt(int index) => _handlers.RemoveAt(index);

    /// <summary>
    /// Subscribes a handler to the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Subscribe(Action<T1, T2, T3, T4, T5, T6> handler)
    {
        if (handler == null)
            throw new ArgumentNullException();

        _handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a handler from the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(Action<T1, T2, T3, T4, T5, T6> handler)
    {
        for (var i = _handlers.Count; i-- > 0;)
            if (_handlers[i] == handler)
                _handlers.RemoveAt(i);
    }

    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    public void Publish(T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            try
            {
                _handlers[i].Invoke(arg0, arg1, arg2, arg3, arg4, arg5);
            }
            catch (Exception e)
            {
                Modules.Messaging.Instance.ModuleLogger.LogDebug($"Error handling message '{Name}' : {e}");
            }
        }
    }
}

/// <summary>
/// MessageBus that can be used to publish messages with 7 arguments to subscribers.
/// </summary>
[PublicAPI]
public class MessageBus<T1, T2, T3, T4, T5, T6, T7> : MessageBusBase
{
    private readonly List<Action<T1, T2, T3, T4, T5, T6, T7>> _handlers = new();
    internal override IReadOnlyList<Delegate> Handlers => _handlers;
    internal override void RemoveHandlerAt(int index) => _handlers.RemoveAt(index);

    /// <summary>
    /// Subscribes a handler to the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Subscribe(Action<T1, T2, T3, T4, T5, T6, T7> handler)
    {
        if (handler == null)
            throw new ArgumentNullException();

        _handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a handler from the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(Action<T1, T2, T3, T4, T5, T6, T7> handler)
    {
        for (var i = _handlers.Count; i-- > 0;)
            if (_handlers[i] == handler)
                _handlers.RemoveAt(i);
    }

    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    public void Publish(T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            try
            {
                _handlers[i].Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
            }
            catch (Exception e)
            {
                Modules.Messaging.Instance.ModuleLogger.LogDebug($"Error handling message '{Name}' : {e}");
            }
        }
    }
}

/// <summary>
/// MessageBus that can be used to publish messages with 8 arguments to subscribers.
/// </summary>
[PublicAPI]
public class MessageBus<T1, T2, T3, T4, T5, T6, T7, T8> : MessageBusBase
{
    private readonly List<Action<T1, T2, T3, T4, T5, T6, T7, T8>> _handlers = new();
    internal override IReadOnlyList<Delegate> Handlers => _handlers;
    internal override void RemoveHandlerAt(int index) => _handlers.RemoveAt(index);

    /// <summary>
    /// Subscribes a handler to the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Subscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8> handler)
    {
        if (handler == null)
            throw new ArgumentNullException();

        _handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a handler from the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8> handler)
    {
        for (var i = _handlers.Count; i-- > 0;)
            if (_handlers[i] == handler)
                _handlers.RemoveAt(i);
    }

    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    public void Publish(T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            try
            {
                _handlers[i].Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }
            catch (Exception e)
            {
                Modules.Messaging.Instance.ModuleLogger.LogDebug($"Error handling message '{Name}' : {e}");
            }
        }
    }
}

/// <summary>
/// MessageBus that can be used to publish messages with 9 arguments to subscribers.
/// </summary>
[PublicAPI]
public class MessageBus<T1, T2, T3, T4, T5, T6, T7, T8, T9> : MessageBusBase
{
    private readonly List<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>> _handlers = new();
    internal override IReadOnlyList<Delegate> Handlers => _handlers;
    internal override void RemoveHandlerAt(int index) => _handlers.RemoveAt(index);

    /// <summary>
    /// Subscribes a handler to the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Subscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> handler)
    {
        if (handler == null)
            throw new ArgumentNullException();

        _handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a handler from the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> handler)
    {
        for (var i = _handlers.Count; i-- > 0;)
            if (_handlers[i] == handler)
                _handlers.RemoveAt(i);
    }

    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    public void Publish(T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            try
            {
                _handlers[i].Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            }
            catch (Exception e)
            {
                Modules.Messaging.Instance.ModuleLogger.LogDebug($"Error handling message '{Name}' : {e}");
            }
        }
    }
}

/// <summary>
/// MessageBus that can be used to publish messages with 10 arguments to subscribers.
/// </summary>
[PublicAPI]
public class MessageBus<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : MessageBusBase
{
    private readonly List<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> _handlers = new();
    internal override IReadOnlyList<Delegate> Handlers => _handlers;
    internal override void RemoveHandlerAt(int index) => _handlers.RemoveAt(index);

    /// <summary>
    /// Subscribes a handler to the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Subscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> handler)
    {
        if (handler == null)
            throw new ArgumentNullException();

        _handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a handler from the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> handler)
    {
        for (var i = _handlers.Count; i-- > 0;)
            if (_handlers[i] == handler)
                _handlers.RemoveAt(i);
    }

    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    public void Publish(T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            try
            {
                _handlers[i].Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
            }
            catch (Exception e)
            {
                Modules.Messaging.Instance.ModuleLogger.LogDebug($"Error handling message '{Name}' : {e}");
            }
        }
    }
}

/// <summary>
/// MessageBus that can be used to publish messages with 11 arguments to subscribers.
/// </summary>
[PublicAPI]
public class MessageBus<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : MessageBusBase
{
    private readonly List<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> _handlers = new();
    internal override IReadOnlyList<Delegate> Handlers => _handlers;
    internal override void RemoveHandlerAt(int index) => _handlers.RemoveAt(index);

    /// <summary>
    /// Subscribes a handler to the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Subscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> handler)
    {
        if (handler == null)
            throw new ArgumentNullException();

        _handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a handler from the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> handler)
    {
        for (var i = _handlers.Count; i-- > 0;)
            if (_handlers[i] == handler)
                _handlers.RemoveAt(i);
    }

    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    public void Publish(T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9,
        T11 arg10)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            try
            {
                _handlers[i].Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
            }
            catch (Exception e)
            {
                Modules.Messaging.Instance.ModuleLogger.LogDebug($"Error handling message '{Name}' : {e}");
            }
        }
    }
}

/// <summary>
/// MessageBus that can be used to publish messages with 12 arguments to subscribers.
/// </summary>
[PublicAPI]
public class MessageBus<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : MessageBusBase
{
    private readonly List<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> _handlers = new();
    internal override IReadOnlyList<Delegate> Handlers => _handlers;
    internal override void RemoveHandlerAt(int index) => _handlers.RemoveAt(index);

    /// <summary>
    /// Subscribes a handler to the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Subscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> handler)
    {
        if (handler == null)
            throw new ArgumentNullException();

        _handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a handler from the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> handler)
    {
        for (var i = _handlers.Count; i-- > 0;)
            if (_handlers[i] == handler)
                _handlers.RemoveAt(i);
    }

    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    public void Publish(T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9,
        T11 arg10, T12 arg11)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            try
            {
                _handlers[i].Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
            }
            catch (Exception e)
            {
                Modules.Messaging.Instance.ModuleLogger.LogDebug($"Error handling message '{Name}' : {e}");
            }
        }
    }
}

/// <summary>
/// MessageBus that can be used to publish messages with 13 arguments to subscribers.
/// </summary>
[PublicAPI]
public class MessageBus<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : MessageBusBase
{
    private readonly List<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> _handlers = new();
    internal override IReadOnlyList<Delegate> Handlers => _handlers;
    internal override void RemoveHandlerAt(int index) => _handlers.RemoveAt(index);

    /// <summary>
    /// Subscribes a handler to the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Subscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> handler)
    {
        if (handler == null)
            throw new ArgumentNullException();

        _handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a handler from the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> handler)
    {
        for (var i = _handlers.Count; i-- > 0;)
            if (_handlers[i] == handler)
                _handlers.RemoveAt(i);
    }

    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    public void Publish(T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9,
        T11 arg10, T12 arg11, T13 arg12)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            try
            {
                _handlers[i].Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
            }
            catch (Exception e)
            {
                Modules.Messaging.Instance.ModuleLogger.LogDebug($"Error handling message '{Name}' : {e}");
            }
        }
    }
}

/// <summary>
/// MessageBus that can be used to publish messages with 14 arguments to subscribers.
/// </summary>
[PublicAPI]
public class MessageBus<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : MessageBusBase
{
    private readonly List<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>> _handlers = new();
    internal override IReadOnlyList<Delegate> Handlers => _handlers;
    internal override void RemoveHandlerAt(int index) => _handlers.RemoveAt(index);

    /// <summary>
    /// Subscribes a handler to the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Subscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> handler)
    {
        if (handler == null)
            throw new ArgumentNullException();

        _handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a handler from the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> handler)
    {
        for (var i = _handlers.Count; i-- > 0;)
            if (_handlers[i] == handler)
                _handlers.RemoveAt(i);
    }

    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    public void Publish(T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9,
        T11 arg10, T12 arg11, T13 arg12, T14 arg13)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            try
            {
                _handlers[i].Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12,
                    arg13);
            }
            catch (Exception e)
            {
                Modules.Messaging.Instance.ModuleLogger.LogDebug($"Error handling message '{Name}' : {e}");
            }
        }
    }
}

/// <summary>
/// MessageBus that can be used to publish messages with 15 arguments to subscribers.
/// </summary>
[PublicAPI]
public class MessageBus<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : MessageBusBase
{
    private readonly List<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>> _handlers = new();
    internal override IReadOnlyList<Delegate> Handlers => _handlers;
    internal override void RemoveHandlerAt(int index) => _handlers.RemoveAt(index);

    /// <summary>
    /// Subscribes a handler to the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Subscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> handler)
    {
        if (handler == null)
            throw new ArgumentNullException();

        _handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a handler from the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> handler)
    {
        for (var i = _handlers.Count; i-- > 0;)
            if (_handlers[i] == handler)
                _handlers.RemoveAt(i);
    }

    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    public void Publish(T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9,
        T11 arg10, T12 arg11, T13 arg12, T14 arg13, T15 arg14)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            try
            {
                _handlers[i].Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12,
                    arg13, arg14);
            }
            catch (Exception e)
            {
                Modules.Messaging.Instance.ModuleLogger.LogDebug($"Error handling message '{Name}' : {e}");
            }
        }
    }
}

/// <summary>
/// MessageBus that can be used to publish messages with 16 arguments to subscribers.
/// </summary>
[PublicAPI]
public class MessageBus<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : MessageBusBase
{
    private readonly List<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>> _handlers =
        new();

    internal override IReadOnlyList<Delegate> Handlers => _handlers;
    internal override void RemoveHandlerAt(int index) => _handlers.RemoveAt(index);

    /// <summary>
    /// Subscribes a handler to the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Subscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> handler)
    {
        if (handler == null)
            throw new ArgumentNullException();

        _handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a handler from the MessageBus.
    /// </summary>
    /// <param name="handler">The handler to unsubscribe.</param>
    public void Unsubscribe(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> handler)
    {
        for (var i = _handlers.Count; i-- > 0;)
            if (_handlers[i] == handler)
                _handlers.RemoveAt(i);
    }

    /// <summary>
    /// Publishes a message to all subscribers.
    /// </summary>
    public void Publish(T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9,
        T11 arg10, T12 arg11, T13 arg12, T14 arg13, T15 arg14, T16 arg15)
    {
        for (var i = _handlers.Count; i-- > 0;)
        {
            try
            {
                _handlers[i].Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12,
                    arg13, arg14, arg15);
            }
            catch (Exception e)
            {
                Modules.Messaging.Instance.ModuleLogger.LogDebug($"Error handling message '{Name}' : {e}");
            }
        }
    }
}