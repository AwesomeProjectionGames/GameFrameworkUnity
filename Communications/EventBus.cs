using System;
using System.Collections.Generic;
using GameFramework.Bus;

namespace UnityGameFrameworkImplementations.Communications
{
    /// <summary>
    /// A fast implementation of IEventBus.
    /// Optimized for high-frequency publishing in a single-threaded context (e.g., Unity Main Thread).
    /// </summary>
    public class EventBus : IEventBus
    {
        // Storage for subscribers. 
        // We map the Event Type to a generic Delegate.
        private readonly Dictionary<Type, Delegate> _subscriptions;

        public EventBus(int initialCapacity = 16)
        {
            // Allocating initial capacity helps avoid Dictionary resizing overhead during initialization
            _subscriptions = new Dictionary<Type, Delegate>(initialCapacity);
        }

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);

            if (_subscriptions.TryGetValue(type, out var existingDelegate))
            {
                // Delegate.Combine creates a new multicast delegate containing both lists
                _subscriptions[type] = Delegate.Combine(existingDelegate, handler);
            }
            else
            {
                _subscriptions[type] = handler;
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);

            if (_subscriptions.TryGetValue(type, out var existingDelegate))
            {
                // Delegate.Remove creates a new delegate with the handler removed
                var newDelegate = Delegate.Remove(existingDelegate, handler);

                if (newDelegate == null)
                {
                    // If no handlers remain, remove the entry to keep the dictionary clean
                    _subscriptions.Remove(type);
                }
                else
                {
                    _subscriptions[type] = newDelegate;
                }
            }
        }

        public void Publish<T>(T eventItem) where T : struct
        {
            // HOT PATH: This method must be as lean as possible.
            
            // 1. Look up the delegate for this specific struct type
            if (_subscriptions.TryGetValue(typeof(T), out var baseDelegate))
            {
                // 2. Fast cast to the specific Action<T>. 
                // This is safe because Subscribe<T> enforces the type match.
                var typedDelegate = (Action<T>)baseDelegate;

                // 3. Invoke all subscribers
                typedDelegate(eventItem);
            }
        }
        
        /// <summary>
        /// Optional helper to clear all subscriptions (useful for scene transitions or resetting state).
        /// </summary>
        public void Clear()
        {
            _subscriptions.Clear();
        }
    }
}