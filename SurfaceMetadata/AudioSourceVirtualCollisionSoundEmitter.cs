using AwesomeProjectionCoreUtils.Extensions;
using SoundManager.VirtualListeners;
using UnityEngine;

namespace GameFramework.SurfaceMetadata
{
    /// <summary>
    /// Hhandles physical collision sounds using SurfaceMeta intensity groups and hierarchy.
    /// </summary>
    public class AudioSourceVirtualCollisionSoundEmitter : CollisionSoundEmitter
    {
        [SerializeField] private AudioSourceVirtual audioSource;

        /// <summary>
        /// Gets the IAudioSource interface for the attached AudioSource.
        /// </summary>
        public override IAudioSource AudioSource => audioSource;
    }
}