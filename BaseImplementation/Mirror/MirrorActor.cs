#if MIRROR
using Mirror;
using UnityEngine;
using GameFramework;
using GameFramework.Identification;

namespace UnityGameFrameworkImplementations.Core
{
    public class MirrorActor : AbstractActor
    {
        [SyncVar(hook = nameof(OnUUIDChanged))]
        private string _uuid = string.Empty;

        [SyncVar(hook = nameof(OnOwnerIdentityChanged))]
        private NetworkIdentity _syncedOwnerIdentity;

        public override string UUID => _uuid;

        private void OnUUIDChanged(string oldUuid, string newUuid)
        {
            // Optional: Logic when UUID changes
        }

        private void OnOwnerIdentityChanged(NetworkIdentity oldOwner, NetworkIdentity newOwner)
        {
            IActor oldActor = oldOwner != null ? oldOwner.GetComponent<IActor>() : null;
            IActor newActor = newOwner != null ? newOwner.GetComponent<IActor>() : null;

            _owner = newActor;
            OnOwnerDidChange(oldActor, newActor);
            ProcessSafeOwnershipChange();
        }

        public override bool TryToChangeUUID(string newUUID)
        {
            if (isServer)
            {
                _uuid = newUUID;
                return true;
            }

            if (isOwned) // Mirror's hasAuthority equivalent for newer versions, or use isOwned
            {
                CmdChangeUUID(newUUID);
                return true;
            }

            return false;
        }

        [Command]
        private void CmdChangeUUID(string newUUID)
        {
            _uuid = newUUID;
        }

        public override void SetOwner(IActor newOwner)
        {
            if (!isServer)
            {
                Debug.LogWarning("[MirrorActor] SetOwner can only be called on Server");
                return;
            }

            if (newOwner is Component comp && comp.TryGetComponent<NetworkIdentity>(out var identity))
            {
                 NetworkIdentity oldIdentity = _syncedOwnerIdentity;
                 _syncedOwnerIdentity = identity;
                 OnOwnerIdentityChanged(oldIdentity, identity);

                 // Also transfer network authority if the new owner has a client connection
                 if (netIdentity.connectionToClient != null)
                 {
                     netIdentity.RemoveClientAuthority();
                 }

                 if (identity.connectionToClient != null)
                 {
                     netIdentity.AssignClientAuthority(identity.connectionToClient);
                 }
            }
            else
            {
                Debug.LogError("[MirrorActor] New owner must have a NetworkIdentity");
            }
        }

        public override void RemoveOwner()
        {
             if (!isServer) return;

             NetworkIdentity oldIdentity = _syncedOwnerIdentity;
             _syncedOwnerIdentity = null;
             OnOwnerIdentityChanged(oldIdentity, null);

             if (netIdentity.connectionToClient != null)
             {
                 netIdentity.RemoveClientAuthority();
             }
        }
    }
}
#endif
