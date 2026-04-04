using UnityEngine;

namespace GameFramework.SurfaceMetadata
{
    [CreateAssetMenu(fileName = "SurfaceMeta", menuName = "AwesomeProjection/Surface Metadata/SurfaceMeta", order = 1)]
    public class SurfaceMeta : ScriptableObject
    {
        [Tooltip("If an array is empty, it will use sounds of the parent surface (if any). Consider adding a 'generic' surface as a parent to avoid having to fill all arrays for each surface.")]
        public SurfaceMeta Parent;
        
        public string SurfaceName;
        public AudioClip[] FootstepSounds;
        public AudioClip[] JumpSounds;
        public AudioClip[] LandingSounds;
        public AudioClip[] CrouchSounds;
        public AudioClip[] SlideSounds;
        
        public AudioClip ScrapeSound;
        public AudioClip[] ContentsWhenShaken;
        public AudioClip[][] ImpactSoundsByIntensityGroup;
        public AudioClip[][] AdditionnalFole;
        
        public float FootstepVolume = 1f;
    }
}