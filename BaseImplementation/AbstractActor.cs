#nullable enable

using System;
using System.Collections.Generic;
using AwesomeProjectionCoreUtils.Extensions;
using GameFramework;
using GameFramework.Bus;
using GameFramework.Dependencies;
using GameFramework.Events;
using GameFramework.Identification;
using UnityEngine;
using UnityGameFrameworkImplementations.Communications;

namespace UnityGameFrameworkImplementations.Core
{
    /// <summary>
    /// Base Actor class handling common setup:
    /// - Registers components in a container
    /// - Provides event bus and transform access
    /// - Handles owner changes and ownership events (providing abstract method to implement networking where needed)
    /// 
    /// No networking logic; just shared Actor functionality.
    /// </summary>
    public abstract class AbstractActor : NetBehaviour, IActor
    {
        public Transform Transform => transform;
        public IEventBus EventDispatcher => _eventBus;
        public IComponentsContainer ComponentsContainer => _componentsContainer;
        public IActor? Owner => _owner;
        
        public abstract string UUID { get; }
        public event Action? OnAnyOwnerChanged;
        public event Action? OnOwnerChanged;
        
        private readonly DeferredEventBus _eventBus = new();
        private readonly ComponentsContainer _componentsContainer = new();
        private readonly Dictionary<Type, IReadOnlyList<IActorComponent>> _componentCache = new Dictionary<Type, IReadOnlyList<IActorComponent>>();
        
        protected IActor? _owner;

        protected virtual void Awake()
        {
            var allComponents = GetComponentsInChildren<IActorComponent>(true);
            AssignActorToComponents(allComponents); // all.Actor = this
            _componentsContainer.RegisterComponents(allComponents);
        }

        protected virtual void Update()
        {
            _eventBus.Tick(Time.deltaTime);
        }

        private void AssignActorToComponents(IActorComponent[] components)
        {
            foreach (var component in components)
            {
                component.Actor = this;
            }
        }
        
        /// <summary>
        /// Try to add a new actor as a parent owner.
        /// Can fail if not permitted (like, not the network owner of this object)
        /// </summary>
        public abstract void SetOwner(IActor newOwner);
        
        /// <summary>
        /// Try to remove the direct owner of this pawn.
        /// Can fail if not permitted (like, not the network owner of this object)
        /// </summary>
        public abstract void RemoveOwner();
        
        /// <summary>
        /// Call this to change UUID if permitted.
        /// Should be done close to spawning / actor instantiation.
        /// Can fail if not permitted (like, not the network owner of this object)
        /// </summary>
        /// <param name="newUUID">The new UUID to apply</param>
        /// <returns>True if the changed has been applyed or scheduled, false if any error prevented this to happen</returns>
        public abstract bool TryToChangeUUID(string newUUID);
        
        /// <summary>
        /// Call when direct owner changed. It will rewire events and fire OnOwnerChanged.
        /// </summary>
        /// <param name="lastOwner">The last owner. Can be null</param>
        /// <param name="newOwner">The new owner. Can be null</param>
        protected virtual void OnOwnerDidChange(IActor? lastOwner, IActor? newOwner)
        {
            OnOwnerChanged?.Invoke();
            if (lastOwner.IsAlive())
            {
                lastOwner.OnAnyOwnerChanged -= HandleAnyOwnerChange;
            }

            if (newOwner.IsAlive())
            {
                newOwner.OnAnyOwnerChanged += HandleAnyOwnerChange;
            }
            HandleAnyOwnerChange();
        }
        
        /// <summary>
        /// Fire OnAnyOwnerChanged and call OnOwned or OnUnowned depending on current owner state
        /// </summary>
        protected virtual void ProcessSafeOwnershipChange()
        {
            OnAnyOwnerChanged?.Invoke();
            if(_owner.IsAlive())
            {
                OnOwned(_owner!);
            }
            else
            {
                OnUnowned();
            }
        }
        
        /// <summary>
        /// Called for everybody, when this actor gets owned by a new owner.
        /// </summary>
        /// <param name="newOwner">The new owner actor.</param>
        protected virtual void OnOwned(IActor newOwner)
        {
            EventDispatcher.Publish(new OnActorOwnedEvent(this));
        }
        
        /// <summary>
        /// Called for everybody, when this actor loses its owner actor.
        /// </summary>
        protected virtual void OnUnowned()
        {
            EventDispatcher.Publish(new OnActorUnownedEvent(this));
        }

        /// <summary>
        /// For 'nested' ownership scenarios, signal that you want to give back ownership to the previous owner in the chain.
        /// For example, exiting a vehicle could give back ownership to the player that owned the vehicle.
        /// </summary>
        public void GiveBackOwnership()
        {
            EventDispatcher.Publish(new OnActorWantsToGiveBackOwnershipEvent(this));
        }

        /// <summary>
        /// Called when any owner in parenting hierarchy changed.
        /// Should be used to update network ownership-dependent logic (with (this as IActor).Controller?.Machine),
        /// because the only one controller can own a chain of actors.
        /// </summary>
        protected virtual void HandleAnyOwnerChange()
        {
            
        }
        
        /// <summary>
        /// Compare two IHaveUUID, only by UUID string
        /// </summary>
        /// <param name="other">The other to compare with</param>
        /// <returns>True if same</returns>
        public virtual bool Equals(IHaveUUID? other)
        {
            if (other == null) return false;
            return UUID == other.UUID;
        }
    }
}