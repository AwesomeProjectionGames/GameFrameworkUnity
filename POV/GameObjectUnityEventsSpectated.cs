#nullable enable

using AwesomeProjectionCoreUtils.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace GameFramework.POV
{
    /// <summary>
    /// A simple MonoBehaviour that invokes UnityEvents when spectating starts and ends.
    /// Intended to enable things like UI, camera, etc.
    /// </summary>
    [RequireComponent(typeof(ICamera))]
    public class GameObjectUnityEventsSpectated : MonoBehaviour, ISpectate
    {
        public bool IsSpectating { get; set; }
        public ISpectateController? SpectateController { get; set; }
        public ICamera GetSpectateCamera() => _camera;
        
        [SerializeField] private UnityEvent? onSpectate;
        [SerializeField] private UnityEvent? onSpectateEnd;

        private ICamera _camera = null!;
        
        private void Awake()
        {
            _camera = GetComponent<ICamera>();
            OnStopSpectating();
        }
        
        public void OnStartSpectating()
        {
            onSpectate?.Invoke();
        }

        public void OnStopSpectating()
        {
            onSpectateEnd?.Invoke();
        }
    }
}