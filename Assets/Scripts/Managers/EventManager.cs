// Located at: Assets/Scripts/Managers/EventManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A static class for managing game-wide events.
/// This system is type-based, using structs to define event data.
/// </summary>
public static class EventManager
{
    // A dictionary to store event delegates, with the event type as the key.
    private static readonly Dictionary<Type, Delegate> s_events = new();

    /// <summary>
    /// Subscribes a listener to an event of a specific type.
    /// </summary>
    /// <param name="listener">The method to be called when the event is triggered.</param>
    /// <typeparam name="T">The struct type of the event.</typeparam>
    public static void AddListener<T>(Action<T> listener) where T : struct
    {
        var eventType = typeof(T);
        if (s_events.TryGetValue(eventType, out var existingDelegate))
        {
            s_events[eventType] = Delegate.Combine(existingDelegate, listener);
        }
        else
        {
            s_events[eventType] = listener;
        }

        // --- DEBUG: Log when a listener is added ---
        Debug.Log($"[EventManager] Listener for event '{eventType.Name}' added from method '{listener.Method.Name}' in class '{listener.Method.DeclaringType.Name}'.");
    }

    /// <summary>
    /// Unsubscribes a listener from an event.
    /// It's crucial to do this when the listening object is destroyed to prevent memory leaks.
    /// </summary>
    /// <param name="listener">The listener method to unsubscribe.</param>
    /// <typeparam name="T">The struct type of the event.</typeparam>
    public static void RemoveListener<T>(Action<T> listener) where T : struct
    {
        var eventType = typeof(T);
        if (s_events.TryGetValue(eventType, out var existingDelegate))
        {
            var newDelegate = Delegate.Remove(existingDelegate, listener);
            if (newDelegate == null)
            {
                s_events.Remove(eventType);
            }
            else
            {
                s_events[eventType] = newDelegate;
            }

            // --- DEBUG: Log when a listener is removed ---
            Debug.Log($"[EventManager] Listener for event '{eventType.Name}' removed from method '{listener.Method.Name}' in class '{listener.Method.DeclaringType.Name}'.");
        }
    }

    /// <summary>
    // Triggers an event, notifying all subscribed listeners.
    /// </summary>
    /// <param name="eventArgs">The data associated with the event.</param>
    /// <typeparam name="T">The struct type of the event.</typeparam>
    public static void TriggerEvent<T>(T eventArgs) where T : struct
    {
        if (s_events.TryGetValue(typeof(T), out var existingDelegate) && existingDelegate is Action<T> action)
        {
            action.Invoke(eventArgs);
        }
        else
        {
            // --- DEBUG: Log if an event is triggered with no listeners ---
            Debug.LogWarning($"[EventManager] Event '{typeof(T).Name}' was triggered, but has no listeners.");
        }
    }
}