#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Registries;
using UnityEngine;

namespace UnityGameFrameworkImplementations.Core.Registries
{
    /// <summary>
    /// Base in-memory registry holding elements indexed by case-insensitive string IDs.
    /// </summary>
    /// <typeparam name="T">Type of items stored (reference type).</typeparam>
    public class BaseRegistry<T> : IRegistry<T> where T : class
    {
        protected readonly Dictionary<string, T> _items = new(StringComparer.OrdinalIgnoreCase);

        public T? this[string id] => GetById(id);
        public IEnumerable<T> All => _items.Values;
        public int Count => _items.Count;

        public bool Contains(string id) => _items.ContainsKey(id);

        public virtual T? GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            _items.TryGetValue(id, out var item);
            return item;
        }

        public virtual bool Register(string id, T item)
        {
            if (string.IsNullOrEmpty(id) || item == null) return false;
            if (_items.ContainsKey(id))
            {
                Debug.LogWarning($"[{GetType().Name}] Duplicate id '{id}' detected, skipping.");
                return false;
            }
            _items.Add(id, item);
            return true;
        }

        public virtual bool Unregister(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            return _items.Remove(id);
        }

        public IEnumerator<T> GetEnumerator() => _items.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
