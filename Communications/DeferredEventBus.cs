using System;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Bus;

namespace UnityGameFrameworkImplementations.Communications
{
    /// <summary>
    /// A deferred implementation of IEventBus, slightly
    /// Events published here are QUEUED and only executed when Update() is called (safely with try/catch for each event).
    /// Optimized to avoid boxing struct events.
    /// </summary>
    public class DeferredEventBus : IEventBus, ITickable
    {
        // ---------------------------------------------------------
        // Inner Logic: Type-Agnostic Queue Interface
        // ---------------------------------------------------------
        private interface IDeferredEventQueue
        {
            /// <summary>
            /// Flushes the queue and invokes the delegates found in the subscription map.
            /// </summary>
            void Dispatch(Dictionary<Type, Delegate> subscriptions);
            
            /// <summary>
            /// Clears any pending events without invoking them.
            /// </summary>
            void Clear();
        }

        // ---------------------------------------------------------
        // Inner Logic: Type-Specific Queue Implementation
        // ---------------------------------------------------------
        private class DeferredEventQueue<T> : IDeferredEventQueue
        {
            private readonly Queue<T> _queue = new Queue<T>();

            public void Enqueue(T item)
            {
                _queue.Enqueue(item);
            }

            public void Dispatch(Dictionary<Type, Delegate> subscriptions)
            {
                // If the queue is empty, do nothing
                if (_queue.Count == 0) return;

                // 1. Check if anyone is actually listening
                Delegate baseDelegate = null;
                Action<T> typedDelegate = null;

                if (subscriptions.TryGetValue(typeof(T), out baseDelegate))
                {
                    typedDelegate = (Action<T>)baseDelegate;
                }

                // 2. Process the queue
                // Note: We use a while loop to handle the current batch.
                // If a handler publishes a new event of Type T immediately, 
                // it will be added to the queue and processed in THIS frame 
                // (Breadth-first recursion).
                while (_queue.Count > 0)
                {
                    T eventItem = _queue.Dequeue();

                    // If no one is listening, we just dequeued and discarded. That's fine.
                    if (typedDelegate == null) continue;

                    try
                    {
                        // Invoke the multicast delegate
                        typedDelegate(eventItem);
                    }
                    catch (Exception e)
                    {
                        // Log the error so you can fix the bug, but don't crash the game
                        UnityEngine.Debug.LogError($"Error processing event {typeof(T).Name}: {e}");
                        UnityEngine.Debug.LogException(e);
                    }
                }
            }

            public void Clear()
            {
                _queue.Clear();
            }
        }

        // ---------------------------------------------------------
        // Main Bus Logic
        // ---------------------------------------------------------

        private readonly Dictionary<Type, Delegate> _subscriptions;
        
        // Map Type -> Queue for fast lookup during Publish
        private readonly Dictionary<Type, IDeferredEventQueue> _queuesMap;
        
        // List of Queues for safe iteration during Tick (Fixes the Exception)
        private readonly List<IDeferredEventQueue> _queuesList;

        public DeferredEventBus(int initialCapacity = 16)
        {
            _subscriptions = new Dictionary<Type, Delegate>(initialCapacity);
            _queuesMap = new Dictionary<Type, IDeferredEventQueue>(initialCapacity);
            _queuesList = new List<IDeferredEventQueue>(initialCapacity);
        }

        public void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);

            if (_subscriptions.TryGetValue(type, out var existingDelegate))
            {
                _subscriptions[type] = Delegate.Combine(existingDelegate, handler);
            }
            else
            {
                _subscriptions[type] = handler;
            }
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_subscriptions.TryGetValue(type, out var existingDelegate))
            {
                var newDelegate = Delegate.Remove(existingDelegate, handler);
                if (newDelegate == null)
                {
                    _subscriptions.Remove(type);
                }
                else
                {
                    _subscriptions[type] = newDelegate;
                }
            }
        }

        public void Publish<T>(T eventItem)
        {
            var type = typeof(T);

            // 1. Get or Create the specific queue
            if (!_queuesMap.TryGetValue(type, out var buffer))
            {
                buffer = new DeferredEventQueue<T>();
                _queuesMap[type] = buffer;
                _queuesList.Add(buffer);
            }

            // 2. Cast and Enqueue (No boxing required because DeferredEventQueue<T> knows the type)
            ((DeferredEventQueue<T>)buffer).Enqueue(eventItem);
        }

        public void Clear()
        {
            _subscriptions.Clear();
            
            // Clear contents of queues
            foreach(var queue in _queuesList)
            {
                queue.Clear();
            }
            
            // Clear the containers
            _queuesMap.Clear();
            _queuesList.Clear();
        }
        
        /// <summary>
        /// Call this once per frame (e.g., in MonoBehaviour.Update).
        /// This processes all queued events.
        /// </summary>
        public void Tick(float deltaTime)
        {
            // A standard for-loop allows the collection size to grow during iteration.
            // If a handler Publishes a NEW event type, _queuesList.Count increases,
            // and the loop will simply continue to that new index in the same frame.
            
            for (int i = 0; i < _queuesList.Count; i++)
            {
                _queuesList[i].Dispatch(_subscriptions);
            }
        }
    }
}