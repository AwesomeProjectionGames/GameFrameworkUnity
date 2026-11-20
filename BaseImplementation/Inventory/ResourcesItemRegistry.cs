#nullable enable

using GameFramework;
using GameFramework.Inventory;
using UnityEngine;
using UnityGameFrameworkImplementations.BaseImplementation;
using UnityGameFrameworkImplementations.Core.Registries;

namespace UnityGameFrameworkImplementations.Inventory
{
    /// <summary>
    /// A registry that automatically loads all item entity components from prefabs in Resources folders.
    /// Implements IRegistry&lt;IItemEntityComponent&gt; via BaseResourcesRegistry.
    /// </summary>
    public class ResourcesItemRegistry : BaseResourcesRegistry<GameObject, IItemEntityComponent>
    {
        public ResourcesItemRegistry(string resourcePath = "") : base(resourcePath)
        {
        }

        protected override bool TryExtractItem(GameObject prefab, out string id, out IItemEntityComponent item)
        {
            item = prefab.GetComponentInChildren<IItemEntityComponent>(true);
            if (item != null)
            {
                var actor = item.EnsureEntity() as IActor;
                id = actor?.ResourcePath ?? string.Empty;
                return !string.IsNullOrEmpty(id);
            }

            id = string.Empty;
            return false;
        }
    }
}
