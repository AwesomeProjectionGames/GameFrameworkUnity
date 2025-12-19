#nullable enable

using System;
using UnityEngine;

namespace GameFramework.SpawnPoint
{
    /// <summary>
    /// Just link interface with unity mono behaviour to use in editor.
    /// </summary>
    public abstract class BaseSpawnPoint : MonoBehaviour, ISpawnPoint
    {
        public abstract bool IsAvailableNow { get; }
        public abstract bool IsValid { get; set; }
        public abstract Tuple<Vector3, Quaternion> Select();
    }
}