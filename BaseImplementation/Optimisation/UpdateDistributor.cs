using System;
using System.Collections.Generic;
using GameFramework;
using UnityEngine;

namespace UnityGameFrameworkImplementations.BaseImplementation.Optimisation
{
    [DefaultExecutionOrder(-100)] // Ensures this runs before scripts relying on it
    public class UpdateDistributor : MonoBehaviour, ITickDistributor
    {
        public int UpdatesPerFrame { get; set; } = 1;

        private TickGroup _updateGroup;
        private TickGroup _fixedUpdateGroup;

        private void Awake()
        {
            _updateGroup = new TickGroup();
            _fixedUpdateGroup = new TickGroup();
        }

        public void Subscribe(Action callback, TickType tickType)
        {
            if (callback == null) return;

            if (tickType == TickType.Update) _updateGroup.Add(callback);
            else _fixedUpdateGroup.Add(callback);
        }

        public void Unsubscribe(Action callback, TickType tickType)
        {
            if (callback == null) return;

            if (tickType == TickType.Update) _updateGroup.Remove(callback);
            else _fixedUpdateGroup.Remove(callback);
        }

        private void Update() => _updateGroup.Process(UpdatesPerFrame);
        private void FixedUpdate() => _fixedUpdateGroup.Process(UpdatesPerFrame);

        // ==========================================
        // INTERNAL SAFE ITERATION LOGIC
        // ==========================================
        private class TickGroup
        {
            private readonly List<Action> _subscribers = new();
            private int _currentIndex = 0;

            public void Add(Action callback)
            {
                if (!_subscribers.Contains(callback))
                {
                    _subscribers.Add(callback);
                }
            }

            public void Remove(Action callback)
            {
                int index = _subscribers.IndexOf(callback);
                if (index >= 0)
                {
                    // Lazy deletion: prevent CollectionModified errors
                    _subscribers[index] = null; 
                }
            }

            public void Process(int limit)
            {
                if (_subscribers.Count == 0) return;

                int processedCount = 0;
                int startIterationIndex = _currentIndex;

                // Loop until we hit our per-frame limit, or we loop through the whole list
                while (processedCount < limit && _subscribers.Count > 0)
                {
                    if (_currentIndex >= _subscribers.Count)
                    {
                        _currentIndex = 0;
                        CleanupNulls(); // Compact the list when we wrap around
                        if (_subscribers.Count == 0) break;
                    }

                    Action currentAction = _subscribers[_currentIndex];

                    // Unity-specific check: Is the underlying Object destroyed?
                    if (currentAction != null && IsTargetAlive(currentAction))
                    {
                        try
                        {
                            currentAction.Invoke();
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[UpdateDistributor] Error executing tick: {e}");
                            _subscribers[_currentIndex] = null; // Mark corrupted actions for removal
                        }
                        processedCount++;
                    }
                    else if (currentAction != null)
                    {
                        // Target is dead but wasn't explicitly unsubscribed
                        _subscribers[_currentIndex] = null; 
                    }

                    _currentIndex++;

                    // Safety break to prevent infinite loops if the list is mostly nulls
                    if (_currentIndex == startIterationIndex && processedCount == 0) break;
                }
            }

            private void CleanupNulls()
            {
                // RemoveAll shifts array elements safely in one operation, minimizing GC
                _subscribers.RemoveAll(action => action == null);
            }

            private bool IsTargetAlive(Action action)
            {
                // If it's a static method, Target is null, which is fine and alive
                if (action.Target == null) return true;

                // If it belongs to a Unity Object, ensure Unity hasn't destroyed it
                if (action.Target is UnityEngine.Object unityObj)
                {
                    return unityObj != null; 
                }

                // Standard C# object
                return true;
            }
        }
    }
}