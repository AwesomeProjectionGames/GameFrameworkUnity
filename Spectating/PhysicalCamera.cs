#nullable enable

using System;
using UnityEngine;
using UnityEngine.Events;

namespace GameFramework.Spectating
{
    /// <summary>
    /// Unity component that represents a physical camera in the scene (implementing ICamera, like a wrapper).
    /// </summary>
    [RequireComponent(typeof(Camera))]
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
                    if (AudioListener != null)
                    {
                        AudioListener.enabled = value;
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

        private Camera Camera => _camera ??= GetComponent<Camera>();

        private AudioListener? AudioListener
        {
            get
            {
                if (!_hasCheckedAudioListener)
                {
                    _audioListener = GetComponent<AudioListener>();
                    _hasCheckedAudioListener = true;
                }
                return _audioListener;
            }
        }
        private Camera? _camera;
        private AudioListener? _audioListener;
        private bool _hasCheckedAudioListener = false;
    }
}