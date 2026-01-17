#if NETCODE
using Unity.Netcode;
using UnityEngine;
using GameFramework;
using GameFramework.Identification;
using Unity.Collections;

namespace UnityGameFrameworkImplementations.Core
{
    public class NetcodeActor : AbstractActor
    {
        private readonly NetworkVariable<FixedString64Bytes> _uuid = new NetworkVariable<FixedString64Bytes>();
        private readonly NetworkVariable<NetworkObjectReference> _syncedOwnerIdentity = new NetworkVariable<NetworkObjectReference>();

        public override string UUID => _uuid.Value.ToString();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _uuid.OnValueChanged += OnUUIDChanged;
            _syncedOwnerIdentity.OnValueChanged += OnOwnerIdentityChanged;

            // Check initial state if needed
            if (_syncedOwnerIdentity.Value.TryGet(out NetworkObject initialOwner))
            {
                 // If we spawn late, we might need to sync the owner manually to internal field
                 // But the callback might fire on spawn?
                 // In recent NGO versions, OnValueChanged is NOT fired for initial sync unless changed.
                 // So we should manually update.
                 OnOwnerIdentityChanged(default, _syncedOwnerIdentity.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            _uuid.OnValueChanged -= OnUUIDChanged;
            _syncedOwnerIdentity.OnValueChanged -= OnOwnerIdentityChanged;
            base.OnNetworkDespawn();
        }

        private void OnUUIDChanged(FixedString64Bytes oldUuid, FixedString64Bytes newUuid)
        {
             // Optional
        }

        private void OnOwnerIdentityChanged(NetworkObjectReference oldRef, NetworkObjectReference newRef)
        {
            IActor oldActor = null;
            if (oldRef.TryGet(out NetworkObject oldObj))
            {
                oldActor = oldObj.GetComponent<IActor>();
            }

            IActor newActor = null;
            if (newRef.TryGet(out NetworkObject newObj))
            {
                newActor = newObj.GetComponent<IActor>();
            }

            // Avoid redundant updates if called manually and then by callback
            if (_owner == newActor) return;

            _owner = newActor;
            OnOwnerDidChange(oldActor, newActor);
            ProcessSafeOwnershipChange();
        }

        public override bool TryToChangeUUID(string newUUID)
        {
            if (IsServer)
            {
                _uuid.Value = new FixedString64Bytes(newUUID);
                return true;
            }

            if (IsOwner)
            {
                RequestChangeUUIDServerRpc(newUUID);
                return true;
            }

            return false;
        }

        [ServerRpc]
        private void RequestChangeUUIDServerRpc(string newUUID)
        {
            _uuid.Value = new FixedString64Bytes(newUUID);
        }

        public override void SetOwner(IActor newOwner)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[NetcodeActor] SetOwner can only be called on Server");
                return;
            }

            if (newOwner is Component comp && comp.TryGetComponent<NetworkObject>(out var netObj))
            {
                 var oldRef = _syncedOwnerIdentity.Value;
                 _syncedOwnerIdentity.Value = netObj;
                 OnOwnerIdentityChanged(oldRef, netObj);

                 // Transfer ownership to the client who owns the newOwner (if any)
                 if (!netObj.IsOwnedByServer)
                 {
                      NetworkObject.ChangeOwnership(netObj.OwnerClientId);
                 }
                 else
                 {
                      NetworkObject.RemoveOwnership(); // Make server owner
                 }
            }
            else
            {
                Debug.LogError("[NetcodeActor] New owner must have a NetworkObject");
            }
        }

        public override void RemoveOwner()
        {
             if (!IsServer) return;

             var oldRef = _syncedOwnerIdentity.Value;
             _syncedOwnerIdentity.Value = default;
             OnOwnerIdentityChanged(oldRef, default);

             NetworkObject.RemoveOwnership();
        }
    }
}
#endif
