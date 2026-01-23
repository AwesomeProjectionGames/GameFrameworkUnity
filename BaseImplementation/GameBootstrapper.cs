using System;
using System.Linq;
using UnityEngine;

namespace UnityGameFrameworkImplementations.Core
{
#if !DISABLE_BOOTSTRAPPER
    /// <summary>
    /// Guarantee the game instance exists before the first scene even finishes loading.
    /// Automatically instanciate so you don't need to creat a prefab yourslef.
    /// For teyting, add flag DISABLE_BOOTSTRAPPER if you do not want this happening automatically.
    /// Or you can set a dummy GameInstance.Instance before this execute.
    /// </summary>
    public static class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitializeGameFramework()
        {
            if (GameInstance.Instance != null) return;

            // 1. Find all types in the assembly that inherit from GameInstance
            var gameInstanceType = GetAllTypes()
                .FirstOrDefault(t => typeof(GameInstance).IsAssignableFrom(t) 
                                     && !t.IsAbstract 
                                     && t != typeof(GameInstance)); // Prefer subclasses

            // Fallback: If no subclass found, use the base class
            if (gameInstanceType == null)
            {
                gameInstanceType = typeof(GameInstance);
            }

            Debug.Log($"[Bootstrapper] Auto-initializing GameInstance type: {gameInstanceType.Name}");

            // 2. Create GameObject and add the specific component type found
            var hostGo = new GameObject($"[{gameInstanceType.Name}]");
            hostGo.AddComponent(gameInstanceType);
        }

        private static System.Collections.Generic.IEnumerable<Type> GetAllTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes());
        }
    }
#endif
}