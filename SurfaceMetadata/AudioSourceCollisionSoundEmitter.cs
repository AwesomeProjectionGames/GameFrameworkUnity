using AwesomeProjectionCoreUtils.Extensions;
using SoundManager.VirtualListeners;
using UnityEngine;

namespace GameFramework.SurfaceMetadata
{
    /// <summary>
    /// Hhandles physical collision sounds using SurfaceMeta intensity groups and hierarchy.
    /// </summary>
    public class AudioSourceCollisionSoundEmitter : CollisionSoundEmitter
    {
        [SerializeField] private AudioSource audioSource;
        
        private AudioSourceAdapter _cachedAdapter;

        /// <summary>
        /// Gets the IAudioSource interface for the attached AudioSource.
        /// </summary>
        public override IAudioSource AudioSource
        {
            get
            {
                if (_cachedAdapter == null && audioSource != null)
                {
                    _cachedAdapter = new AudioSourceAdapter(audioSource);
                }
                return _cachedAdapter;
            }
        }
    }
}