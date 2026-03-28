using GameFramework.Dependencies;
using GameFramework.Events;
using UnityEngine;

namespace GameFramework.Spectating
{
    /// <summary>
    /// A GameObjectUnityEventsSpectated but specific to actors
    /// When actors are owned, we automatically spectate this actor (if it's owned by the local player)
    /// </summary>
    public class ActorUnityEventsSpectated : GameObjectUnityEventsSpectated, IEntityComponent
    {
        public IEntity Entity { get; set; }

        private void Start()
        {
            Entity.EventDispatcher.Subscribe<OnActorOwnedEvent>(OnActorOwned);
            Entity.EventDispatcher.Subscribe<OnActorUnownedEvent>(OnActorUnowned);
        }

        private void OnDestroy()
        {
            Entity.EventDispatcher.Unsubscribe<OnActorOwnedEvent>(OnActorOwned);
            Entity.EventDispatcher.Unsubscribe<OnActorUnownedEvent>(OnActorUnowned);
        }

        private void OnActorOwned(OnActorOwnedEvent evt)
        {
            evt.Actor.Controller?.SpectateController?.Spectate(this);
        }
        
        private void OnActorUnowned(OnActorUnownedEvent evt)
        {
            if (evt.Actor.Controller?.SpectateController?.CurrentSpectate == this)
            {
                Debug.LogWarning($"[ActorUnityEventsSpectated] Cannot stop spectating actor {evt.Actor.UUID} because it is currently being spectated.", evt.Actor.Transform);
            }
            evt.Actor.Controller?.SpectateController?.StopSpectating();
        }
    }
}