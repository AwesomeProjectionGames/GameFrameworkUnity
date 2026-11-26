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

        internal void SetController(ICameraController? controller) => _controller = controller;

        internal void ClearController(ICameraController controller)
        {
            if (ReferenceEquals(_controller, controller))
                _controller = null;
        }

        public override Vector3 WorldToScreenPoint(Vector3 position)
        {
            float screenW = Screen.width;
            float screenH = Screen.height;
            if (screenW <= 0f || screenH <= 0f) return Vector3.zero;

            Rect vp = (_controller != null && (!(_controller is Object uo) || uo != null))
                ? _controller.ViewPort
                : Rect;

            float vpW = vp.width * screenW;
            float vpH = vp.height * screenH;
            if (vpW <= 0f || vpH <= 0f) return Vector3.zero;

            float tanHalfFov = Mathf.Tan(FieldOfView * Mathf.Deg2Rad * 0.5f);
            if (tanHalfFov <= 1e-6f) return Vector3.zero;

            Vector3 localPos = Quaternion.Inverse(Rotation) * (position - Position);
            float z = localPos.z;
            if (Mathf.Abs(z) < 1e-6f)
                z = z >= 0f ? 1e-6f : -1e-6f;

            float aspect = vpW / vpH;
            float ndcX = localPos.x / (z * tanHalfFov * aspect);
            float ndcY = localPos.y / (z * tanHalfFov);

            float screenX = (vp.x + (ndcX * 0.5f + 0.5f) * vp.width) * screenW;
            float screenY = (vp.y + (ndcY * 0.5f + 0.5f) * vp.height) * screenH;
            return new Vector3(screenX, screenY, localPos.z);
        }

        [SerializeField] private float fieldOfView = 60f;
        [SerializeField] private float nearClipPlane = 0.1f;
        [SerializeField] private float farClipPlane = 1000f;

        private bool _isActive = false;
        private ICameraController? _controller;
    }
}