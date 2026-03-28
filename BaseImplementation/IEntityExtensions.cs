using System.Collections.Generic;
using GameFramework;
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
    }
}
