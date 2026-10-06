#nullable enable

using System;
using System.Collections.Generic;
using AwesomeProjectionCoreUtils.Extensions;
using GameFramework;
using GameFramework.Dependencies;
using GameFramework.Events;
using GameFramework.Identification;
using MemoryPack;
using UnityEngine;

namespace UnityGameFrameworkImplementations.Core
{
    /// <summary>
    /// Base Actor class handling common setup:
    /// - Registers components in a container
    /// - Provides event bus and transform access
    /// - Handles owner changes and ownership events (providing abstract method to implement networking where needed)
    /// - Handles full state snapshot capture and restoration via ISerializedObject and MemoryPack
    /// 
    /// No networking logic; just shared Actor functionality.
    /// </summary>
    public abstract class AbstractActor : NetEntity, IActor, ISerializedObject
    {
        [HideInInspector]
        [SerializeField] private string resourcePath = "";

        /// <summary>
        /// The path to load this actor's prefab from a Resources folder (without extension),
        /// allowing it to be instantiated at runtime via Resources.Load.
        /// Automatically populated by <see cref="ActorPostprocessor"/> when the prefab is in a Resources folder.
        /// </summary>
        public string ResourcePath
        {
            get => resourcePath;
            set => resourcePath = value;
        }

        public IActor? Owner => _owner;
        
        public abstract string UUID { get; }
        public event Action? OnAnyOwnerChanged;
        public event Action? OnOwnerChanged;
        
        protected IActor? _owner;
        
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
            
            OnAnyOwnerChanged?.Invoke();
            if(_owner.IsAlive())
            {
                OnOwned(_owner!);
            }
            else
            {
                OnUnowned();
            }
            
            HandleAnyOwnerChange();
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

        #region State Persistence (ISerializedObject via MemoryPack)
        /// <summary>
        /// Serializes the state of all sibling components implementing ISerializedObject into a MemoryPack-serialized payload.
        /// Format: Dictionary&lt;uint, byte[]&gt; where the 32-bit key composites:
        /// - Upper 28 bits: TypeHash (deterministic hash of comp.GetType().FullName)
        /// - Lower 4 bits: InstanceIndex (0..15, supporting up to 16 components of the same type)
        /// </summary>
        public virtual byte[] Serialize()
        {
            var components = ComponentsContainer.GetComponents<ISerializedObject>();
            var typeCounts = new Dictionary<uint, uint>();
            var stateDict = new Dictionary<uint, byte[]>(components.Count);

            for (int i = 0; i < components.Count; i++)
            {
                var comp = components[i];
                if (ReferenceEquals(comp, this)) continue;

                uint typeHash = comp.GetType().GetDeterministicTypeHash();
                uint instanceIndex = 0;
                if (typeCounts.TryGetValue(typeHash, out uint count))
                {
                    instanceIndex = count;
                    typeCounts[typeHash] = count + 1;
                }
                else
                {
                    typeCounts[typeHash] = 1;
                }

                uint compositeKey = (typeHash << 4) | (instanceIndex & 0x0F);
                stateDict[compositeKey] = comp.Serialize();
            }

            return MemoryPackSerializer.Serialize(stateDict);
        }

        /// <summary>
        /// Deserializes and restores state to matching ISerializedObject components from a MemoryPack-serialized payload.
        /// Uses the same composite key (TypeHash 28-bit + InstanceIndex 4-bit) to support multiple components of the same type.
        /// </summary>
        public virtual void Deserialize(byte[] serializedData)
        {
            if (serializedData == null || serializedData.Length == 0) return;
            var stateDict = MemoryPackSerializer.Deserialize<Dictionary<uint, byte[]>>(serializedData);
            if (stateDict == null) return;

            var components = ComponentsContainer.GetComponents<ISerializedObject>();
            var typeCounts = new Dictionary<uint, uint>();

            for (int i = 0; i < components.Count; i++)
            {
                var comp = components[i];
                if (ReferenceEquals(comp, this)) continue;

                uint typeHash = comp.GetType().GetDeterministicTypeHash();
                uint instanceIndex = 0;
                if (typeCounts.TryGetValue(typeHash, out uint count))
                {
                    instanceIndex = count;
                    typeCounts[typeHash] = count + 1;
                }
                else
                {
                    typeCounts[typeHash] = 1;
                }

                uint compositeKey = (typeHash << 4) | (instanceIndex & 0x0F);
                if (stateDict.TryGetValue(compositeKey, out byte[] componentBytes))
                {
                    comp.Deserialize(componentBytes);
                }
            }
        }
        #endregion
    }
}
