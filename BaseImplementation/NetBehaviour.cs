using UnityEngine;

#if MIRROR
using Mirror;
#elif NETCODE
using Unity.Netcode;
#endif

namespace UnityGameFrameworkImplementations.Core
{
#if MIRROR
    /// <summary>
    /// Base class to link network types with NetworkBehaviour when using Mirror.
    /// Useful so base abstract classes like AbstractActor can inherit from it
    /// without knowing the networking solution.
    /// </summary>
    public abstract class NetBehaviour : NetworkBehaviour {}

#elif NETCODE
    /// <summary>
    /// Base class to link network types with NetworkBehaviour when using Netcode for GameObjects.
    /// Useful so base abstract classes like AbstractActor can inherit from it
    /// without knowing the networking solution.
    /// </summary>
    public abstract class NetBehaviour : NetworkBehaviour {}
#else
    /// <summary>
    /// Base class to link network types with MonoBehaviour when no networking solution is used.
    /// Useful so base abstract classes like AbstractActor can inherit from it
    /// without knowing the networking solution.
    /// </summary>
    public abstract class NetBehaviour : MonoBehaviour {}
#endif
}