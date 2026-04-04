using AwesomeProjectionCoreUtils.Extensions;
using SoundManager.VirtualListeners;
using UnityEngine;

namespace GameFramework.SurfaceMetadata
{
    /// <summary>
    /// Hhandles physical collision sounds using SurfaceMeta intensity groups and hierarchy.
    /// </summary>
    public abstract class CollisionSoundEmitter : SurfaceIdentifier
    {
        public abstract IAudioSource AudioSource { get; }
        
        [Header("Audio")]
        [SerializeField] private float _cooldown = 0.2f;
        
        [Header("Physics")]
        [Tooltip("Force thresholds for intensity levels (e.g., 2, 8, 15)")]
        [SerializeField] private float[] _thresholds = { 2f, 8f, 15f };

        private float _lastPlayTime = -1f;

        private void OnCollisionEnter(Collision col)
        {
            if (!AudioSource.IsAlive() || Meta == null || Time.time < _lastPlayTime + _cooldown) return;

            float force = col.impulse.magnitude;
            if (_thresholds == null || _thresholds.Length == 0 || force < _thresholds[0]) return;

            PlayImpact(Meta, force);
            _lastPlayTime = Time.time;
        }

        /// <summary>
        /// Selects intensity index based on force and triggers audio playback.
        /// </summary>
        private void PlayImpact(SurfaceMeta meta, float force)
        {
            int idx = 0;
            for (int i = 0; i < _thresholds.Length; i++)
                if (force >= _thresholds[i]) idx = i; else break;

            // Play both layers if available
            TryPlayFromGroup(meta, idx, false);
            TryPlayFromGroup(meta, idx, true);
        }

        /// <summary>
        /// Traverses SurfaceMeta hierarchy to find and play a random clip from the specific intensity group.
        /// </summary>
        private void TryPlayFromGroup(SurfaceMeta meta, int idx, bool foley)
        {
            SurfaceMeta current = meta;
            while (current != null)
            {
                var groups = foley ? current.AdditionnalFoley : current.ImpactSoundsByIntensityGroup;
                
                if (groups != null && groups.Length > 0)
                {
                    // Clamp index to ensure we don't OOB if a specific surface has fewer intensity layers
                    int clampedIdx = Mathf.Min(idx, groups.Length - 1);
                    var clips = groups[clampedIdx];

                    if (clips.Clips != null && clips.Clips.Length > 0)
                    {
                        AudioClip clip = clips.Clips[Random.Range(0, clips.Clips.Length)];
                        if (clip != null)
                        {
                            AudioSource.PlayOneShot(clip, 1f);
                            return; // Found and played, stop searching hierarchy
                        }
                    }
                }
                current = current.Parent;
            }
        }
    }
}