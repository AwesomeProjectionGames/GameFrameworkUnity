using System;
using GameFramework;
using GameFramework.Dependencies;
using GameFramework.Inventory;
using UnityEngine;
using UnityGameFrameworkImplementations.BaseImplementation;

namespace UnityGameFrameworkImplementations.Inventory
{
    /// <summary>
    /// A registry that loads items from Resources folders.
    /// Should be a global service in, for example, the IGameInstance
    /// </summary>
    public class ResourcesItemRegistry : ItemRegistry
    {
        /// <summary>
        /// Loads all prefabs in the Resources folders and registers their IItem components.
        /// </summary>
        /// <param name="resourcePath">Path within Resources folder, empty string to load everything.</param>
        public ResourcesItemRegistry(string resourcePath = "")
        {
            LoadAndRegisterItems(resourcePath);
        }

        private void LoadAndRegisterItems(string resourcePath)
        {
            // Load all GameObjects (prefabs) in the Resources folder
            var prefabs = Resources.LoadAll<GameObject>(resourcePath);

            foreach (var prefab in prefabs)
            {
                // Find all IItem components in the prefab (including children)
                var items = prefab.GetComponentsInChildren<IItemEntityComponent>(true);

                foreach (var item in items)
                {
                    try
                    {
                        var actor = item.EnsureEntity() as IActor;
                        string id = actor?.ResourcePath ?? string.Empty;
                        if (string.IsNullOrEmpty(id))
                        {
                            Debug.LogWarning($"Item in prefab '{prefab.name}' has null or empty ResourcePath on its actor, skipping.");
                            continue;
                        }

                        bool added = RegisterItem(id, item);
                        if (!added)
                        {
                            Debug.LogWarning($"Duplicate item ResourcePath '{id}' detected in prefab '{prefab.name}', skipping.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to register item '{item}' from prefab '{prefab.name}': {ex.Message}");
                    }
                }
            }
        }
    }
}
