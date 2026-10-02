#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace UnityGameFrameworkImplementations.Core
{
    /// <summary>
    /// A processor that automatically updates the ResourcePath of AbstractActor components in prefabs when they are imported or modified.
    /// The ResourcePath is the path relative to the enclosing Resources folder, without the .prefab extension,
    /// so it can be used directly with Resources.Load.
    /// </summary>
    public class ActorPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                if (!assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) continue;

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null) continue;

                var actors = prefab.GetComponentsInChildren<AbstractActor>(true);
                if (actors.Length == 0) continue;

                if (!TryGetResourceRelativePath(assetPath, out string targetResourcePath))
                {
                    Debug.LogWarning(
                        $"'{assetPath}' contains an AbstractActor but is not under a Resources folder, so it has no Resources-relative ResourcePath.", prefab);
                    continue;
                }

                bool modified = false;

                foreach (var actor in actors)
                {
                    if (actor.ResourcePath != targetResourcePath)
                    {
                        actor.ResourcePath = targetResourcePath;
                        EditorUtility.SetDirty(actor);
                        modified = true;
                    }
                }

                if (modified)
                {
                    AssetDatabase.SaveAssets();
                    Debug.Log($"Updated actor ResourcePath for: {assetPath} -> {targetResourcePath}");
                }
            }
        }

        /// <summary>
        /// Produces the path used with Resources.Load: the path after the last "Resources/"
        /// segment, without the .prefab extension.
        /// Returns false when the prefab is not under a Resources folder.
        /// </summary>
        private static bool TryGetResourceRelativePath(string assetPath, out string resourcePath)
        {
            string normalized = assetPath.Replace('\\', '/');
            const string token = "Resources/";
            int idx = normalized.LastIndexOf(token, StringComparison.OrdinalIgnoreCase);

            if (idx < 0)
            {
                resourcePath = string.Empty;
                return false;
            }

            string relative = normalized.Substring(idx + token.Length);
            if (relative.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                relative = relative.Substring(0, relative.Length - ".prefab".Length);
            }

            resourcePath = relative;
            return relative.Length > 0;
        }
    }
}
#endif
