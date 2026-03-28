using System;
using System.Reflection;
using AwesomeProjectionCoreUtils.Extensions;
using GameFramework;
using GameFramework.Dependencies;
using UnityEngine;
using UnityGameFrameworkImplementations.Core;

namespace UnityGameFrameworkImplementations.BaseImplementation
{
    /// <summary>
    /// Attribute to mark fields or properties for automatic injection from the Entity's components.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class BindEntityComponentAttribute : Attribute
    {
        /// <summary>
        /// If true, logs an error if the component is not found on the Entity.
        /// </summary>
        public bool IsRequired { get; set; } = true;

        public BindEntityComponentAttribute(bool isRequired = true)
        {
            IsRequired = isRequired;
        }
    }
    
    /// <summary>
    /// A static class providing extension methods for entity components in the network-specific context.
    /// </summary>
    public static class EntityComponentExtensions
    {
        /// <summary>
        /// Automatically populates fields and properties marked with [BindEntityComponent] 
        /// by resolving them against the component's Entity.
        /// Like a lightweight Dependency Injection system for Entity Components.
        /// You should call this method in the Start method of your IEntityComponent implementation (as OnEnable/Awake might be too early / race condition).
        /// </summary>
        /// <param name="component">The component requesting injection.</param>
        public static void InjectEntityDependencies(this IEntityComponent component)
        {
            // Safety check: Ensure we are working with a MonoBehaviour context if needed, 
            // though IEntityComponent is sufficient for the logic.
            
            // 1. Ensure the Entity is assigned.
            // If called in Awake/OnEnable, the Entity might not be assigned by the parent MonoEntity yet.
            // We attempt to find it if it is missing.
            if (component.Entity == null && component is MonoBehaviour mb)
            {
                Debug.LogWarning($"[InjectEntityDependencies] 'Entity' is null for '{component.GetType().Name}'. Attempting to find Entity in parent hierarchy.");
                component.Entity = mb.GetComponentInParent<IEntity>();
            }

            if (component.Entity == null)
            {
                Debug.LogWarning($"[InjectEntityDependencies] Could not inject dependencies for '{component.GetType().Name}' because 'Entity' is null.");
                return;
            }

            var type = component.GetType();
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            // 2. Handle Fields
            var fields = type.GetFields(flags);
            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<BindEntityComponentAttribute>();
                if (attr == null) continue;

                if (TryResolveDependency(component.Entity, field.FieldType, out var result))
                {
                    field.SetValue(component, result);
                }
                else if (attr.IsRequired)
                {
                    Debug.LogError($"[InjectEntityDependencies] Required dependency '{field.FieldType.Name}' missing for '{type.Name}' on '{component.Entity.Transform.gameObject.name}'", component.Entity.Transform);
                }
            }

            // 3. Handle Properties
            var properties = type.GetProperties(flags);
            foreach (var prop in properties)
            {
                var attr = prop.GetCustomAttribute<BindEntityComponentAttribute>();
                if (attr == null) continue;

                if (!prop.CanWrite)
                {
                    Debug.LogError($"[InjectEntityDependencies] Property '{prop.Name}' in '{type.Name}' is marked for binding but is read-only.");
                    continue;
                }

                if (TryResolveDependency(component.Entity, prop.PropertyType, out var result))
                {
                    prop.SetValue(component, result);
                }
                else if (attr.IsRequired)
                {
                    Debug.LogError($"[InjectEntityDependencies] Required dependency '{prop.PropertyType.Name}' missing for '{type.Name}' on '{component.Entity.Transform.gameObject.name}'", component.Entity.Transform);
                }
            }
        }
        
        private static bool TryResolveDependency(IEntity entity, Type targetType, out object? result)
        {
            result = entity.ComponentsContainer.GetComponent(targetType);
            if(result == null)
            {
                // Try to get from the Entity itself
                if (targetType.IsInstanceOfType(entity))
                {
                    result = entity;
                }
            }
            if(result == null)
            {
                // Try to get from the game mode (which can be useful for shared services or managers)
                result = GameInstance.Instance?.CurrentGameMode?.ComponentsContainer.GetComponent(targetType);
            }
            if(result == null)
            {
                // Try to get from the global GameInstance
                result = GameInstance.Instance?.ComponentsContainer.GetComponent(targetType);
            }
            return result != null;
        }
    }
}