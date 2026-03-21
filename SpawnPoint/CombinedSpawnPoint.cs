#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeProjectionCoreUtils.Extensions;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameFramework.SpawnPoint
{
    /// <summary>
    /// A spawn point that aggregates multiple other spawn points (children of specified parents).
    /// When selected, it picks a random available spawn point from its collection.
    /// </summary>
    public class CombinedSpawnPoint : BaseSpawnPoint
    {
        [Tooltip("The parents transforms that contain the spawn points to be used.")]
        [SerializeField] Transform[] spawnPointsParents = null!;
        List<ISpawnPoint> _spawnPoints = new List<ISpawnPoint>();
        private void Awake()
        {
            Assert.IsTrue(spawnPointsParents.Length > 0, "No spawn points parents found in the CombinedSpawnPoint");
            foreach (var spawnPointsParent in spawnPointsParents)
            {
                _spawnPoints.AddRange(spawnPointsParent.GetComponentsInChildren<ISpawnPoint>());
            }
            _spawnPoints = _spawnPoints.Where(spawnPoint => spawnPoint != null && spawnPoint != this).ToList();
            Assert.IsTrue(_spawnPoints.Count > 0, "No spawn points found in the CombinedSpawnPoint");
        }

        public override bool IsAvailableNow => IsValid;

        /// <summary>
        /// Returns true if at least one of the aggregated spawn points is available.
        /// Setting this value is not supported as it depends on the children.
        /// </summary>
        public override bool IsValid { 
            get => _spawnPoints.Any(spawnPoint => spawnPoint.IsAlive() && spawnPoint.IsAvailableNow);
            set
            {
                Debug.LogError("Setting the validity of a combined spawn point is not supported");
            }
        }

        /// <summary>
        /// Select a random spawn point from the spawn points
        /// </summary>
        /// <returns>The position and rotation of the selected spawn point</returns>
        public override Tuple<Vector3, Quaternion> Select()
        {
            // 1. Try to find the ideal candidates
            var availablePoints = _spawnPoints.Where(sp => sp.IsAlive() && sp.IsAvailableNow);

            if (availablePoints.Any())
            {
                return availablePoints.RandomElement().Select();
            }

            // 2. Fallback: If no one is "available," just pick any alive point
            var fallbackPoints = _spawnPoints.Where(sp => sp.IsAlive());
    
            if (fallbackPoints.Any())
            {
                return fallbackPoints.RandomElement().Select();
            }

            // 3. Absolute safety: Return a default (e.g., world origin) or log an error
            Debug.LogWarning("No valid spawn points found! Spawning at origin.");
            return new Tuple<Vector3, Quaternion>(Vector3.zero, Quaternion.identity);
        }
    }
}