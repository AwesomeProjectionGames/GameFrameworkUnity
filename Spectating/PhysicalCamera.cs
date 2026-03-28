#nullable enable

using System;
using UnityEngine;

namespace GameFramework.Spectating
{
    /// <summary>
    /// Unity component that represents a physical camera in the scene (implementing ICamera, like a wrapper).
    /// </summary>
    public class PhysicalCamera : AbstractCamera
    {
        public override bool IsActive
        {
            get => Camera.enabled;
            set
            {
                if (Camera.enabled != value)
                {
                    Camera.enabled = value;
                    foreach (var component in _componentsToEnableWithCamera)
                    {
                        if (component != null)
                        {
                            component.enabled = value;
                        }
                    }
                    OnActiveStateChanged.Invoke(value);
                }
            }
        }

        public override float FieldOfView
        {
            get => Camera.fieldOfView;
            set => Camera.fieldOfView = value;
        }

        public override float NearClipPlane
        {
            get => Camera.nearClipPlane;
            set => Camera.nearClipPlane = value;
        }

        public override float FarClipPlane
        {
            get => Camera.farClipPlane;
            set => Camera.farClipPlane = value;
        }

        public override Rect Rect
        {
            get => Camera.rect;
            set => Camera.rect = value;
        }
        
        public override Vector3 WorldToScreenPoint(Vector3 position)
        {
            return Camera.WorldToScreenPoint(position);
        }

        private Camera Camera => _camera ??= GetComponent<Camera>();
        
        [SerializeField] private Camera? _camera;
        [SerializeField] MonoBehaviour[] _componentsToEnableWithCamera = Array.Empty<MonoBehaviour>();
    }
}