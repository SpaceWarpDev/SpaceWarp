using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace SpaceWarp.API.Messaging;

[PublicAPI]
public static class MessageBusManager
{
	private static Dictionary<string, MessageBusBase> _messagesBusesByName = new Dictionary<string, MessageBusBase>();

	public static T Add<T>(string name) where T : MessageBusBase, new()
	{
		if (string.IsNullOrEmpty(name))
			throw new ArgumentException("Null or empty MessageBus name");

		if (_messagesBusesByName.ContainsKey(name))
			throw new Exception($"MessageBus '{name}' exists already");

		var messageBus = new T
		{
			Name = name
		};
		_messagesBusesByName.Add(name, messageBus);

		Modules.Messaging.Instance.ModuleLogger.LogDebug($"MessageBus '{name}' created");
		return messageBus;
	}

	public static bool Exists(string messageBusName) => _messagesBusesByName.ContainsKey(messageBusName);

	public static bool TryGet<T>(string messageBusName, out T messageBus) where T : MessageBusBase, new()
	{
		if (string.IsNullOrEmpty(messageBusName))
			throw new ArgumentException("Null or empty MessageBus name");

		if (!_messagesBusesByName.TryGetValue(messageBusName, out MessageBusBase messageBusBase))
		{
			messageBus = null;
			return false;
		}

		if (messageBusBase is not T @base)
			throw new Exception(
				$"Message bus '{messageBusBase.Name}' is of type '{messageBusBase.GetType()}' but the requested type was '{typeof(T)}'");
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
						Modules.Messaging.Instance.ModuleLogger.LogDebug($"Memory leak detected : a destroyed instance of the '{target.GetType().Assembly.GetName().Name}:{target.GetType().Name}' class is holding a '{messageBus.Name}' MessageBus handler");
						messageBus.RemoveHandlerAt(i);
						memoryLeaks++;
						break;
				}
			}
		}

		if (memoryLeaks > 0)
			Modules.Messaging.Instance.ModuleLogger.LogDebug($"{memoryLeaks} detected!");
	}
}