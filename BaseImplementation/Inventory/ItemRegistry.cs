#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Identification;
using GameFramework.Inventory;
using UnityEngine;

namespace UnityGameFrameworkImplementations.Inventory
{
    public class ItemRegistry : IItemRegistry
    {
        private readonly Dictionary<string, IItemEntityComponent> _items = new Dictionary<string, IItemEntityComponent>(StringComparer.OrdinalIgnoreCase);

        public IItemEntityComponent? this[string id] => GetItemOfId(id);

        public IEnumerable<IItemEntityComponent> AllItems => _items.Values;

        public IItemEntityComponent? GetItemOfId(string id)
        {
            if (!_items.TryGetValue(id, out var item))
            {
                Debug.LogError($"Item with id '{id}' not found.");
                return null;
            }

            return item;
        }
        
        public T? GetByID<T>(string id) where T : IHaveUUID
        {
            return (T?)GetItemOfId(id);
        }

        public bool RegisterItem(string id, IItemEntityComponent item)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (item == null) throw new ArgumentNullException(nameof(item));

            if (_items.ContainsKey(id))
                return false; // Already recorded

            _items.Add(id, item);
            return true;
        }

        public IEnumerator<IItemEntityComponent> GetEnumerator() => _items.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}