#nullable enable

using UnityEngine;
using UnityEngine.Events;

namespace GameFramework.POV
{
    /// <summary>
    /// A simple MonoBehaviour that invokes UnityEvents when spectating starts and ends.
    /// Intended to enable things like UI, camera, etc.
    /// </summary>
    public class GameObjectUnityEventsSpectated : MonoBehaviour, ISpectate
    {
        public bool IsSpectating { get; set; }
        public ISpectateController? SpectateController { get; set; }
        
        [SerializeField] private UnityEvent? onSpectate;
        [SerializeField] private UnityEvent? onSpectateEnd;
        
        private void Awake()
        {
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