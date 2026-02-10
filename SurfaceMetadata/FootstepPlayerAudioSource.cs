using UnityEngine;

namespace GameFramework.SurfaceMetadata
{
    public class FootstepPlayerAudioSource : FootstepPlayer
    {
        [SerializeField] private AudioSource source;

        protected override void PlayClip(AudioClip clip, float volume, float pitch)
        {
            if (source == null) return;
            source.pitch = pitch;
            source.PlayOneShot(clip, volume);
        }
    }
}

