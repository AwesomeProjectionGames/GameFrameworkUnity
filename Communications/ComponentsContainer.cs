#nullable enable

using System;
using System.Collections.Generic;
using GameFramework.Dependencies;

namespace UnityGameFrameworkImplementations.Communications
{
    public class ComponentsContainer : IComponentsContainer
    {
        // Stores arrays of specific types (e.g., Key: typeof(IHealth) -> Value: IHealth[])
        private readonly Dictionary<Type, Array> _componentCache = new();

        /// <summary>
        /// Analyzes the provided components and categorizes them by all their implemented interfaces and base types.
        /// Converts lists to typed Arrays to ensure O(1) access and type safety.
        /// </summary>
        public void RegisterComponents(IEnumerable<object> components)
        {
            // 1. Bucket all components into a temporary list structure
            var tempBuckets = new Dictionary<Type, List<object>>();

            foreach (var component in components)
            {
                var assignableTypes = GetAssignableTypes(component.GetType());

                foreach (var type in assignableTypes)
                {
                    if (!tempBuckets.TryGetValue(type, out var list))
                    {
                        list = new List<object>();
                        tempBuckets[type] = list;
                    }
                    list.Add(component);
                }
            }

            // 2. Convert Buckets to Typed Arrays and cache them
            _componentCache.Clear();
            foreach (var kvp in tempBuckets)
            {
                Type typeKey = kvp.Key;
                var objList = kvp.Value;

                // 1. Create the specific array type (e.g., IHealth[], int[])
                // We use the base class 'Array' instead of casting to 'object[]'
                Array typedArray = Array.CreateInstance(typeKey, objList.Count);

                // 2. Use ICollection.CopyTo to fill the array efficiently
                // This handles the copying internally and avoids the explicit manual loop
                ((System.Collections.ICollection)objList).CopyTo(typedArray, 0);

                _componentCache[typeKey] = typedArray;
            }
        }

        private IEnumerable<Type> GetAssignableTypes(Type concreteType)
        {
            // Add all interfaces
            var types = new HashSet<Type>(concreteType.GetInterfaces());

            // Add all base classes
            var currentBase = concreteType;
            while (currentBase != null && currentBase != typeof(object))
            {
                types.Add(currentBase);
                currentBase = currentBase.BaseType;
            }

            return types;
        }

        #region IComponentsContainer Implementation
        public object? GetService(Type componentType)
        {
            if (_componentCache.TryGetValue(componentType, out var list))
            {
                return list.Length > 0 ? list.GetValue(0) : null;
            }

            return null;
        }

        public IReadOnlyList<T> GetComponents<T>()
        {
            if (_componentCache.TryGetValue(typeof(T), out var list))
            {
                return (T[])list;
            }

            return Array.Empty<T>();
        }
        #endregion

    }
}