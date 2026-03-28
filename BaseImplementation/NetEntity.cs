using System;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Bus;
using GameFramework.Dependencies;
using UnityEngine;
using UnityGameFrameworkImplementations.Communications;

namespace UnityGameFrameworkImplementations.Core
{
    /// <summary>
    /// Base for AbstractActor and NetworkedActor.
    /// - Registers components in a container
    /// - Provides event bus and transform access
    /// </summary>
    public class NetEntity : NetBehaviour, IEntity
    {
        public Transform Transform => transform;
        public IEventBus EventDispatcher => DeferredEventBus;
        public IComponentsContainer ComponentsContainer => ComponentsContainerField;
        
        protected readonly DeferredEventBus DeferredEventBus = new();
        protected readonly ComponentsContainer ComponentsContainerField = new();

        protected virtual void Awake()
        {
            this.InitializeEntityComponents(ComponentsContainerField);
        }

        protected virtual void Update()
        {
            DeferredEventBus.Tick(Time.deltaTime);
        }
    }
}