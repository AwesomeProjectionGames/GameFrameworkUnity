#nullable enable

using UnityEngine;

namespace GameFramework.POV
{
    /// <summary>
    /// A simple ISpectate implementation for static objects in full screen.
    /// It's intended for the default thing to spectate, before any entity has spawned (like a lobby camera).
    /// </summary>
    public class FullScreenSpectatedStatic : GameObjectUnityEventsSpectated
    {
        [SerializeField] bool requestSpectateOnEnable = true;

        private void OnEnable()
        {
            if (requestSpectateOnEnable)
            {
                POVController.FullScreen.Spectate(this);
            }
        }
    }
}