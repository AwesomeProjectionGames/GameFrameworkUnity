#nullable enable

using UnityEngine;

namespace GameFramework.Spectating
{
    /// <summary>
    /// A virtual camera that only exposes properties but uses only one camera under the hood for all virtual cameras (using VirtualCameraController).
    /// </summary>
    public class VirtualCamera : AbstractCamera
    {
        public override bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnActiveStateChanged?.Invoke(_isActive);
            }
        }

        public override float FieldOfView
        {
            get => fieldOfView;
            set => fieldOfView = value;
        }

        public override float NearClipPlane
        {
            get => nearClipPlane;
            set => nearClipPlane = value;
        }

        public override float FarClipPlane
        {
            get => farClipPlane;
            set => farClipPlane = value;
        }

        public override Rect Rect { get; set; } = new Rect(0, 0, 1, 1);

        [SerializeField] private float fieldOfView = 60f;
        [SerializeField] private float nearClipPlane = 0.1f;
        [SerializeField] private float farClipPlane = 1000f;

        private bool _isActive = false;
    }
}