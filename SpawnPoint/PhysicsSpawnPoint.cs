#nullable enable

using System.Collections;
using UnityEngine;
using System;
using System.Linq;
using AwesomeProjectionCoreUtils.Extensions;

namespace GameFramework.SpawnPoint
{
    /// <summary>
    /// A spawnpoint that check if the spawnpoint (tranform box) is available (not blocked by physics or by a player).
    /// </summary>
    public class PhysicsSpawnPoint : SimpleSpawnPoint
    {
        private static readonly Vector3[] FullRayPoints = {
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1),
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0)
        };
        private static readonly Vector3[] OnlyCenterRayPoints = {
            new Vector3(0, 0, 0)
        };
        /// <summary>
        /// Return true if this spawnpoint is fully available now (not blocked by script or by physic).
        /// This is the effective availability of the spawnpoint.
        /// </summary>
        public override bool IsAvailableNow
        {
            get { return base.IsAvailableNow && !IsInRadius() && HasGround() && !_isTempDisabled; }
        }

        private bool HasGround()
        {
            if(!_useAutoGroundCheck) return true;
           return (_useOnlyCenterRaycast ? OnlyCenterRayPoints : FullRayPoints).All(point => this.PhysicsScene().Raycast(transform.position + point, Vector3.down, _maxDistanceToGround, _groundMask));
        }

        [Tooltip("Layer mask defining which objects block the spawn point.")]
        [SerializeField] LayerMask _objectMaskToInvalidate;
        [Tooltip("The radius around the spawnpoint used to determine if the spawn is available")]
        [SerializeField] internal float _objectRadiusToInvalidate = 1;
        [Tooltip("If true, checks for ground presence below the spawn point.")]
        [SerializeField] bool _useAutoGroundCheck = true;
        [Tooltip("Layer mask defining what counts as ground.")]
        [SerializeField] LayerMask _groundMask;
        [Tooltip("Maximum distance to check for ground below the spawn point.")]
        [SerializeField] float _maxDistanceToGround = 0.51f;
        [Tooltip("If true, only checks ground at the center. If false, checks corners as well.")]
        [SerializeField] bool _useOnlyCenterRaycast = false;
        bool _isTempDisabled = false;

        public override Tuple<Vector3, Quaternion> Select()
        {
            StartCoroutine(DisableSpawnOneFrameForPhysics());
            return new Tuple<Vector3, Quaternion>(transform.position, transform.rotation);
        }

        bool IsInRadius()
        {
            if (_objectRadiusToInvalidate == 0) return false;
            return Physics.CheckBox(transform.position, Vector3.one * _objectRadiusToInvalidate, Quaternion.identity, _objectMaskToInvalidate); ;
        }

        IEnumerator DisableSpawnOneFrameForPhysics()
        {
            _isTempDisabled = true;
            yield return new WaitForFixedUpdate();
            _isTempDisabled = false;
        }

        protected override void DrawDebug()
        {
            Gizmos.color = IsAvailableNow ? debugNormalColor : Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * _objectRadiusToInvalidate * 2f);
        }
    }
}