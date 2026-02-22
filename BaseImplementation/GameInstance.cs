#nullable enable

using System.Collections.Generic;
using UnityEngine;
using GameFramework;
using GameFramework.Bus;
using GameFramework.Dependencies;
using UnityGameFrameworkImplementations.BaseImplementation.Inventory;
using UnityGameFrameworkImplementations.BaseImplementation.Optimisation;
using UnityGameFrameworkImplementations.Communications;

namespace UnityGameFrameworkImplementations.Core
{
    /// <summary>
    /// Concrete implementation of the global GameInstance.
    /// Manages the lifecycle of global services, the system-wide event bus, and the active GameMode.
    /// Persists across scene loads via DontDestroyOnLoad.
    /// </summary>
    [DefaultExecutionOrder(-9999)] // Ensure this initializes before almost everything else
    public class GameInstance : MonoBehaviour, IGameInstance
    {
        /// <summary>
        /// Singleton accessor.
        /// This should be the only singleton in the entire game!
        /// Use IComponentsContainer to register other global services in a more testable way.
        /// </summary>
        public static IGameInstance? Instance { get; private set; }

        #region IGameInstance Implementation

        public IEventBus EventBus => _eventBus;
        public IComponentsContainer Services => _services;
        public IGameMode? CurrentGameMode { get; set; }

        #endregion

        private readonly DeferredEventBus _eventBus = new();
        private readonly ComponentsContainer _services = new();
        

        private void Awake()
        {
            ManageSingleton();
            
            if (Instance == this)
            {
                _services.RegisterComponents(GetServices());
            }
        }

        private void Update()
        {
            if (Instance != this) return;
            _eventBus.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                CurrentGameMode = null;
                Instance = null;
            }
        }

        /// <summary>
        /// Handles the DontDestroyOnLoad singleton logic.
        /// </summary>
        private void ManageSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[GameInstance] Duplicate GameInstance detected on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }


        /// <summary>
        /// At the start of the game, register all services to be used via dependency injection.
        /// All services returned by this method will be registered in the Services container.
        /// </summary>
        protected virtual IEnumerable<object> GetServices()
        {
            yield return new ResourcesItemRegistry();
            yield return gameObject.AddComponent<UpdateDistributor>();
        }
    }
}