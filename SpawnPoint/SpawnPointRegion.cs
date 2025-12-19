#nullable enable

using UnityEngine;
using System;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace GameFramework.SpawnPoint
{
    /// <summary>
    /// A spawnregion that return a random position inside it (between bound A and B).
    /// </summary>
    public class SpawnPointRegion : SimpleSpawnPoint
    {
        [FormerlySerializedAs("_boundA")]
        [Tooltip("First corner of the spawn region.")]
        [SerializeField] internal Transform? boundA;
        [FormerlySerializedAs("_boundB")]
        [Tooltip("Second corner of the spawn region.")]
        [SerializeField] internal Transform? boundB;

        /// <summary>
        /// Select a random position inside the region.
        /// </summary>
        /// <returns>The position and rotation to spawn the object</returns>
        public override Tuple<Vector3, Quaternion> Select()
        {
            if (boundA == null || boundB == null)
            {
                Debug.LogWarning("SpawnPointRegion: BoundA or BoundB is not set. Taking the spawn point's transform as fallback.");
                return new Tuple<Vector3, Quaternion>(transform.position, transform.rotation);
            }
            Vector3 randomAxis = new Vector3(Random.Range(0f, 1f), Random.Range(0f,1f), Random.Range(0f, 1f));
            Vector3 position = new Vector3(Mathf.Lerp(boundA.position.x, boundB.position.x,randomAxis.x), Mathf.Lerp(boundA.position.y, boundB.position.y, randomAxis.y), Mathf.Lerp(boundA.position.z, boundB.position.z, randomAxis.z));
            Quaternion rotation = Quaternion.Lerp(boundA.rotation, boundB.rotation, randomAxis.x);
            return new Tuple<Vector3, Quaternion>(position, rotation);
        }

        protected override void DrawDebug()
        {
            if (boundA == null || boundB == null) return;
            Gizmos.color = IsAvailableNow ? debugNormalColor : Color.red;
            Bounds bound = new Bounds(boundA.position, Vector3.zero);
            bound.Encapsulate(boundB.position);
            Gizmos.DrawWireCube(bound.center, bound.size);
        }
    }
}