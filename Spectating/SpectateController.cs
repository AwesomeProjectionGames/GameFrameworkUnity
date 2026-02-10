#nullable enable

using AwesomeProjectionCoreUtils.Extensions;
using UnityEngine;

namespace GameFramework.Spectating
{
    /// <summary>
    /// A spectate controller that manages spectating for a single point of view.
    /// </summary>
    public class SpectateController : VirtualCameraController, ISpectateController
    {
        /// <summary>
        /// Gets the singleton instance of the full screen POV controller.
        /// BE SURE TO LIMIT USAGE OF THIS (for example, just get this instance once in a controller, and then use the reference from the controller instead to improve reusability, even for split screen).
        /// </summary>
        public static ISpectateController? FullScreen { get; private set; }

        /// <summary>
        /// The current spectate instance that this controller is managing.
        /// Keep in mind that this instance is not guaranteed to be alive (null + unity lifecycle).
        /// </summary>
        public ISpectate? CurrentSpectate { get; private set; }

        [SerializeField] private bool isFullScreen = true;

        protected override void Awake()
        {
            if (isFullScreen)
            {
                if (FullScreen.IsAlive())
                {
                    Debug.LogWarning("Multiple SpectateController instances detected. Overriding FullScreen instance.");
                }
                FullScreen = this;
            }
            base.Awake();
        }

        public void Spectate(ISpectate spectate)
        {
            if (CurrentSpectate.IsAlive())
            {
                CurrentSpectate!.StopSpectating();
            }
            CurrentSpectate = spectate;
            if (CurrentSpectate.IsAlive())
            {
                CurrentSpectate!.StartSpectating(this);
            }
        }

        public void StopSpectating()
        {
            if (CurrentSpectate != null)
            {
                ISpectate lastSpectate = CurrentSpectate;
                CurrentSpectate = null;
                lastSpectate.StopSpectating();
            }
        }
    }
}