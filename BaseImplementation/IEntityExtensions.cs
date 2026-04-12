#nullable enable

using AwesomeProjectionCoreUtils.Extensions;
using GameFramework;
using GameFramework.Bus;
using GameFramework.Dependencies;
using UnityEngine;
using UnityGameFrameworkImplementations.Communications;

namespace UnityGameFrameworkImplementations.Core
{
    public static class IEntityExtensions
    {
        public static void InitializeEntityComponents(this IEntity entity, ComponentsContainer componentsContainer)
        {
            if (entity is Component component)
            {
                var allComponents = component.GetComponentsInChildren<IEntityComponent>(true);
                foreach (var entityComponent in allComponents)
                {
                    entityComponent.Entity = entity;
                }

                componentsContainer.RegisterComponents(allComponents);
            }
        }
        
        public static IGameMode? GameMode(this IEntity entity)
        {
            return GameInstance.Instance?.CurrentGameMode;
        }
        
        public static IEventBus? GlobalEventDispatcher(this IEntity entity)
        {
            return GameInstance.Instance?.EventDispatcher;
        }
        
        /// <summary>
        /// Spawn / Instantiate a clone of this entity at the specified location and rotation.
        /// </summary>
        public static IEntity? Clone(this IEntity entity, bool destroyWithScene = true)
        {
            return entity.GameMode()?.Spawn(entity, destroyWithScene);
        }
        
        /// <summary>
        /// Spawn / Instantiate a clone of this entity at the specified location and rotation.
        /// </summary>
        public static IEntity? Clone(this IEntity entity, Vector3 location, Quaternion rotation, bool destroyWithScene = true)
        {
            return entity.GameMode()?.SpawnAtLocation(entity, location, rotation, destroyWithScene);
        }
        
        /// <summary>
        /// Clones the MeshRenderers (and optionally Colliders) from this GameObject, returning a new GameObject representing the cloned visual hierarchy.
        /// </summary>
        /// <returns>The gameobject clone with only mesh and optionnally, colliders</returns>
        public static GameObject CloneVisual(this IEntity entity, bool withCollider = false, Vector3? localPosition = null, Quaternion? localRotation = null, Transform? parent = null)
        {
            return entity.Transform.gameObject.CloneVisual();
        }
        /// <summary>
        /// Moves the entity to the specified position and rotation. If the entity has a physics component, it will be teleported using the physics system. Otherwise, the transform will be directly modified.
        /// </summary>
        public static void Move(this IEntity entity, Vector3 position, Quaternion rotation)
        {
            if (entity is IPawn pawn)
            {
                pawn.Teleport(position, rotation);
            }
            else if (entity.ComponentsContainer.GetComponent<IPhysics>() is { } physics && physics.IsAlive())
            {
                physics.Teleport(position, rotation);
            }
            else
            {
                entity.Transform.position = position;
                entity.Transform.rotation = rotation;
            }
        }
    }
}
