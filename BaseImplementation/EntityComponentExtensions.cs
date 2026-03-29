using System;
using System.Collections.Generic;
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

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            var currentType = component.GetType();
            while (currentType != null && currentType != typeof(object))
            {
                // 2. Handle Fields
                var fields = currentType.GetFields(flags);
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
                        Debug.LogError($"[InjectEntityDependencies] Required dependency '{field.FieldType.Name}' missing for '{currentType.Name}' on '{component.Entity.Transform.gameObject.name}'", component.Entity.Transform);
                    }
                }

                // 3. Handle Properties
                var properties = currentType.GetProperties(flags);
                foreach (var prop in properties)
                {
                    var attr = prop.GetCustomAttribute<BindEntityComponentAttribute>();
                    if (attr == null) continue;

                    if (!prop.CanWrite)
                    {
                        Debug.LogError($"[InjectEntityDependencies] Property '{prop.Name}' in '{currentType.Name}' is marked for binding but is read-only.");
                        continue;
                    }

                    if (TryResolveDependency(component.Entity, prop.PropertyType, out var result))
                    {
                        prop.SetValue(component, result);
                    }
                    else if (attr.IsRequired)
                    {
                        Debug.LogError($"[InjectEntityDependencies] Required dependency '{prop.PropertyType.Name}' missing for '{currentType.Name}' on '{component.Entity.Transform.gameObject.name}'", component.Entity.Transform);
                    }
                }
                
                currentType = currentType.BaseType;
            }
        }
        
        private static bool TryResolveDependency(IEntity entity, Type targetType, out object result)
        {
            // Handle array / collection requests (e.g., IReadOnlyList<T>, IEnumerable<T>, T[], IList<T>, List<T>)
            if (targetType.IsGenericType)
            {
                var genericType = targetType.GetGenericTypeDefinition();
                if (genericType == typeof(IEnumerable<>) || 
                    genericType == typeof(IReadOnlyList<>) ||
                    genericType == typeof(IReadOnlyCollection<>) ||
                    genericType == typeof(IList<>) ||
                    genericType == typeof(ICollection<>) ||
                    genericType == typeof(List<>))
                {
                    Type elementType = targetType.GetGenericArguments()[0];
                    if (TryResolveComponents(entity, elementType, out var componentsList))
                    {
                        if (genericType == typeof(List<>))
                        {
                            result = Activator.CreateInstance(targetType, componentsList);
                        }
                        else
                        {
                            result = componentsList;
                        }
                        return true;
                    }
                }
            }
            if (targetType.IsArray)
            {
                Type elementType = targetType.GetElementType()!;
                if (TryResolveComponents(entity, elementType, out var componentsArray))
                {
                    result = componentsArray;
                    return true;
                }
            }

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

        private static bool TryResolveComponents(IEntity entity, Type elementType, out object result)
        {
            // Use reflection to call Generic GetComponents<T>()
            var method = typeof(IComponentsContainer).GetMethod(nameof(IComponentsContainer.GetComponents))?.MakeGenericMethod(elementType);
            if (method == null)
            {
                result = null;
                return false;
            }

            var localComponents = method.Invoke(entity.ComponentsContainer, null) as System.Collections.IEnumerable;
            
            // If the local container doesn't have it, we might want to check GameMode/GameInstance just like single fields
            // For now, let's just attempt local resolving. If we want to strictly follow single resolve pattern:
            bool hasElements = false;
            if (localComponents != null)
            {
                foreach (var _ in localComponents) { hasElements = true; break; }
            }

            if (hasElements)
            {
                result = localComponents;
                return true;
            }

            // Fallback to game mode
            if (GameInstance.Instance?.CurrentGameMode?.ComponentsContainer != null)
            {
                var gmComponents = method.Invoke(GameInstance.Instance.CurrentGameMode.ComponentsContainer, null) as System.Collections.IEnumerable;
                foreach (var _ in gmComponents ?? Array.Empty<object>()) { hasElements = true; break; }
                if (hasElements)
                {
                    result = gmComponents;
                    return true;
                }
            }

            // Fallback to global GameInstance
            if (GameInstance.Instance?.ComponentsContainer != null)
            {
                var giComponents = method.Invoke(GameInstance.Instance.ComponentsContainer, null) as System.Collections.IEnumerable;
                foreach (var _ in giComponents ?? Array.Empty<object>()) { hasElements = true; break; }
                if (hasElements)
                {
                    result = giComponents;
                    return true;
                }
            }

            result = null;
            return false;
        }
    }
}