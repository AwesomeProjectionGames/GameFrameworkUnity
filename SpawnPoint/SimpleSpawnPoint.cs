#nullable enable

using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace GameFramework.SpawnPoint
{
	/// <summary>
	/// A simple default spawn point implementation that return transform position and rotation + draw gizmos.
	/// </summary>
	public class SimpleSpawnPoint : BaseSpawnPoint
	{
		public override bool IsAvailableNow => _isValid;

		private bool _isValid = true;

		public override bool IsValid
		{
			get => _isValid;
			set => _isValid = value;
		}

        [FormerlySerializedAs("_debugNormalColor")]
        [Tooltip("Color of the gizmo when the spawn point is available.")]
        [SerializeField] internal Color debugNormalColor;
	
		/// <summary>
		/// Select this spawnpoint
		/// </summary>
		/// <returns>The position and rotation to spawn the object</returns>
		public override Tuple<Vector3, Quaternion> Select()
		{
			return new Tuple<Vector3, Quaternion>(transform.position, transform.rotation);
		}

		protected virtual void DrawDebug()
		{
            Gizmos.color = (IsAvailableNow ? debugNormalColor : Color.red);
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }

        private void OnDrawGizmos()
        {
            DrawDebug();
        }
    }
}