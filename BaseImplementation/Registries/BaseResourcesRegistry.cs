#nullable enable

using GameFramework.Registries;
using UnityEngine;

namespace UnityGameFrameworkImplementations.Core.Registries
{
    /// <summary>
    /// Abstract registry that automatically scans and loads items from Unity's Resources folders at instantiation.
    /// </summary>
    /// <typeparam name="TAsset">Unity Object type loaded from Resources (ScriptableObject, GameObject, etc.).</typeparam>
    /// <typeparam name="TItem">Type stored in the registry.</typeparam>
    public abstract class BaseResourcesRegistry<TAsset, TItem> : BaseRegistry<TItem>
        where TAsset : UnityEngine.Object
        where TItem : class
    {
        protected BaseResourcesRegistry(string resourcePath = "")
        {
            LoadFromResources(resourcePath);
        }

        /// <summary>
        /// Extracts the identifier and registry item from the loaded resource asset.
        /// </summary>
        /// <param name="asset">The asset loaded from Resources.</param>
        /// <param name="id">Output unique identifier.</param>
        /// <param name="item">Output item to register.</param>
        /// <returns>True if the item is valid and should be registered; otherwise false.</returns>
        protected abstract bool TryExtractItem(TAsset asset, out string id, out TItem item);

        private void LoadFromResources(string resourcePath)
        {
            var assets = Resources.LoadAll<TAsset>(resourcePath);
            foreach (var asset in assets)
            {
                if (TryExtractItem(asset, out var id, out var item))
                {
                    Register(id, item);
                }
            }
            Debug.Log($"[{GetType().Name}] Loaded {Count} item(s) from Resources/{resourcePath}.");
        }
    }
}
