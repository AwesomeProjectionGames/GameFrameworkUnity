#nullable enable

using AwesomeProjectionCoreUtils.Extensions;

namespace GameFramework.POV
{
    /// <summary>
    /// A POV controller that manages spectating for a single point of view.
    /// </summary>
    public class POVController : ISpectateController
    {
        private static ISpectateController? _fullScreen;
        
        /// <summary>
        /// Gets the singleton instance of the full screen POV controller.
        /// BE SURE TO LIMIT USAGE OF THIS (for example, just get this instance once in a controller, and then use the reference from the controller instead to improve reusability, even for split screen).
        /// </summary>
        public static ISpectateController FullScreen
        {
            get
            {
                if (_fullScreen == null)
                {
                    _fullScreen = new POVController();
                }
                return _fullScreen;
            }
        }
        
        /// <summary>
        /// The current spectate instance that this controller is managing.
        /// Keep in mind that this instance is not guaranteed to be alive (null + unity lifecycle).
        /// </summary>
        public ISpectate? CurrentSpectate { get; private set; }
        
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