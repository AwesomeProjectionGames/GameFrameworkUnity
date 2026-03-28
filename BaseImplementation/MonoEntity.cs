using GameFramework;
using GameFramework.Bus;
using GameFramework.Dependencies;
using UnityEngine;
using UnityGameFrameworkImplementations.BaseImplementation;
using UnityGameFrameworkImplementations.Communications;

namespace UnityGameFrameworkImplementations.Core
{
    /// <summary>
    /// The base class for an Object that has significant presence in the game world / has some functionality but not networked (manager or info class).
    /// - Registers components in a container
    /// - Provides event bus and transform access
    /// </summary>
    public class MonoEntity : MonoBehaviour, IEntity, IEntityComponent
    {
        public IEntity Entity { get; set; }
        
        public Transform Transform => transform;
        public IEventBus EventDispatcher => DeferredEventBus;
        public IComponentsContainer ComponentsContainer => ComponentsContainerField;
        
        protected readonly DeferredEventBus DeferredEventBus = new();
        protected readonly ComponentsContainer ComponentsContainerField = new();

        protected virtual void Awake()
        {
            this.InitializeEntityComponents(ComponentsContainerField);
            this.InjectEntityDependencies();
        }

        protected virtual void Update()
        {
            DeferredEventBus.Tick(Time.deltaTime);
        }
    }
}