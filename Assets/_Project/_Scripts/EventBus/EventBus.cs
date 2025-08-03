using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple Event Bus system for decoupled communication between game systems
/// </summary>
public static class EventBus
{
    private static Dictionary<Type, List<IEventBinding>> _bindings = new Dictionary<Type, List<IEventBinding>>();
    
    /// <summary>
    /// Subscribe to an event type with a callback
    /// </summary>
    public static void Subscribe<T>(Action<T> callback) where T : IGameEvent
    {
        Type eventType = typeof(T);
        
        if (!_bindings.ContainsKey(eventType))
        {
            _bindings[eventType] = new List<IEventBinding>();
        }
        
        EventBinding<T> binding = new EventBinding<T>(callback);
        _bindings[eventType].Add(binding);
    }
    
    /// <summary>
    /// Unsubscribe from an event type
    /// </summary>
    public static void Unsubscribe<T>(Action<T> callback) where T : IGameEvent
    {
        Type eventType = typeof(T);
        
        if (!_bindings.TryGetValue(eventType, out List<IEventBinding> bindingList)) return;
        
        IEventBinding bindingToRemove = bindingList.Find(binding => 
            binding is EventBinding<T> eventBinding && 
            eventBinding.Callback.Equals(callback));
            
        if (bindingToRemove != null)
        {
            _bindings[eventType].Remove(bindingToRemove);
            
            // Clean up empty lists
            if (_bindings[eventType].Count == 0)
            {
                _bindings.Remove(eventType);
            }
        }
    }
    
    /// <summary>
    /// Fire an event to all subscribers
    /// </summary>
    public static void Fire<T>(T eventData) where T : IGameEvent
    {
        Type eventType = typeof(T);
        
        if (!_bindings.TryGetValue(eventType, out List<IEventBinding> bindingList)) return;
        
        // Create a copy to avoid modification during iteration
        List<IEventBinding> bindingsCopy = new List<IEventBinding>(bindingList);
        
        foreach (IEventBinding binding in bindingsCopy)
        {
            if (binding is EventBinding<T> eventBinding)
            {
                try
                {
                    eventBinding.Callback?.Invoke(eventData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error executing event callback for {eventType.Name}: {ex.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// Clear all event bindings (useful for scene transitions)
    /// </summary>
    public static void Clear()
    {
        _bindings.Clear();
    }
    
    /// <summary>
    /// Clear all bindings for a specific event type
    /// </summary>
    public static void Clear<T>() where T : IGameEvent
    {
        Type eventType = typeof(T);
        if (_bindings.ContainsKey(eventType))
        {
            _bindings.Remove(eventType);
        }
    }
    
    /// <summary>
    /// Get the number of subscribers for an event type
    /// </summary>
    public static int GetSubscriberCount<T>() where T : IGameEvent
    {
        Type eventType = typeof(T);
        return _bindings.TryGetValue(eventType, out List<IEventBinding> binding) ? binding.Count : 0;
    }
    
    /// <summary>
    /// Check if there are any subscribers for an event type
    /// </summary>
    public static bool HasSubscribers<T>() where T : IGameEvent
    {
        return GetSubscriberCount<T>() > 0;
    }
}

/// <summary>
/// Base interface for all game events
/// </summary>
public interface IGameEvent { }

/// <summary>
/// Internal interface for event bindings
/// </summary>
internal interface IEventBinding { }

/// <summary>
/// Internal class to store event callbacks
/// </summary>
internal class EventBinding<T> : IEventBinding where T : IGameEvent
{
    public Action<T> Callback { get; }
    
    public EventBinding(Action<T> callback)
    {
        Callback = callback;
    }
}