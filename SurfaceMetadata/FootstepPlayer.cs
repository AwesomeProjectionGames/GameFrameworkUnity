using GameFramework.Dependencies;
using UnityEngine;

namespace GameFramework.SurfaceMetadata
{
    public enum SpecialFootstepType
    {
        None,
        Jump,
        Down,
        CrouchStart,
        CrouchEnd
    }
    
    public abstract class FootstepPlayer : MonoBehaviour, IActorComponent
    {
        public IActor Actor { get; set; }
        
        [SerializeField] private SurfaceMeta defaultSurfaceMeta;
        [SerializeField] private float castDistance = 10f;
        
        public void PlayFootStep()
        {
            PlayFootStep(SpecialFootstepType.None);
        }
        public void PlayFootStep(SpecialFootstepType specialFootstepType)
        {
            PlayFootStep(specialFootstepType, 1f);
        }
        public void PlayFootStep(SpecialFootstepType specialFootstepType, float pitch)
        {
            RaycastHit hit;
            SurfaceMeta audioModifier = defaultSurfaceMeta;
            
            if (Physics.Linecast(transform.position, transform.position + transform.TransformDirection(Vector3.down) * castDistance, out hit))
            {
                if (hit.collider.TryGetComponent(out ISurfaceModifier surface))
                {
                    audioModifier = surface.Meta;
                }
            }

            AudioClip[] clips = audioModifier.FootstepSounds;
            switch (specialFootstepType)
            {
                case SpecialFootstepType.Jump:
                    clips = audioModifier.JumpSounds;
                    break;
                case SpecialFootstepType.Down:
                    clips = audioModifier.LandingSounds;
                    break;
                case SpecialFootstepType.CrouchStart:
                    clips = audioModifier.CrouchSounds;
                    break;
                case SpecialFootstepType.CrouchEnd: 
                    clips = audioModifier.SlideSounds;
                    break;
            }
            if (clips.Length == 0) return;
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            PlayClip(clip, audioModifier.FootstepVolume, pitch);
        }

        protected abstract void PlayClip(AudioClip clip, float volume, float pitch);
        
        private void OnDrawGizmosSelected()
        {
            if (transform == null) return;
            
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + transform.TransformDirection(Vector3.down) * castDistance;
            
            // Draw the linecast ray
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPos, endPos);
            
            // Draw a sphere at the end point
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPos, 0.1f);
        }
    }
}