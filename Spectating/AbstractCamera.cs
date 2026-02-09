using GameFramework.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace GameFramework.Spectating
{
    /// <summary>
    /// A base implementation of the ICamera interface using MonoBehaviour.
    /// More friendly to inspector (that cannot serialize interfaces).
    /// </summary>
    public abstract class AbstractCamera : MonoBehaviour, ICamera, IRaycastable
    {
        public abstract bool IsActive { get; set; }
        public abstract float FieldOfView { get; set; }
        public abstract float NearClipPlane { get; set; }
        public abstract float FarClipPlane { get; set; }
        public abstract Rect Rect { get; set; }
        public Transform Transform => transform;

        [Tooltip("Invoked when the state of the camera changed.")]
        public UnityEvent<bool> OnActiveStateChanged { get; } = new UnityEvent<bool>();

        public RaycastHit? Raycast(LayerMask mask, float maxDistance)
        {
            bool hasHit = Raycast(mask, maxDistance, out var hit);
            return hasHit ? hit : null;
        }

        public bool Raycast(LayerMask mask, float maxDistance, out RaycastHit hit)
        {
            return Physics.Raycast(Transform.position, Transform.forward, out hit, maxDistance, mask);
        }
    }
}