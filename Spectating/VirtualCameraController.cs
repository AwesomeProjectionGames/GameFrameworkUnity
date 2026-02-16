#nullable enable

using System.Collections;
using AwesomeProjectionCoreUtils.Extensions;
using UnityEngine;

namespace GameFramework.Spectating
{
    /// <summary>
    /// Unity component that controls virtual cameras and their transitions.
    /// </summary>
    [RequireComponent(typeof(ICamera))]
    public class VirtualCameraController : MonoBehaviour, ICameraController
    {
        public ICamera? CurrentCamera { get; protected set; }

        protected ICamera _masterCamera = null!;
        private bool _isTransitioning;
        
        private Rect _viewPortRect = new Rect(0, 0, 1, 1);
        
        // This serves as a snapshot of the camera state when interrupting a transition
        private SnapshotCamera? _snapshotCamera;

        public Rect ViewPort { get => _viewPortRect; set => SetViewPort(value); }
        
        private void SetViewPort(Rect viewPort)
        {
            _viewPortRect = viewPort;
            if (_masterCamera.IsAlive())
            {
                _masterCamera.Rect = _viewPortRect;
            }
        }

        protected virtual void Awake()
        {
            _masterCamera = GetComponent<ICamera>();
            if (!_masterCamera.IsAlive())
            {
                Debug.LogError("No ICamera component found on the GameObject. Please add one to use VirtualCameraController.");
                return;
            }
            _masterCamera.Rect = _viewPortRect;
            
            // Create a snapshot object
            _snapshotCamera = new SnapshotCamera();
        }

        public void TransitionToCamera(ICamera cameraTarget)
        {
            TransitionToCamera(cameraTarget, new CameraTransitionSettings { transitionDuration = 0f });
        }

        public void TransitionToCamera(ICamera cameraTarget, CameraTransitionSettings transitionSettings)
        {
            ICamera? fromCamera = CurrentCamera;
            
            if (_isTransitioning)
            {
                // We are interrupting a transition.
                // We take a snapshot of the current master camera state into our snapshot camera.
                _snapshotCamera!.CopyFrom(_masterCamera);
                fromCamera = _snapshotCamera;
                
                StopAllCoroutines();
            }
            
            StartCoroutine(TransitionCoroutine(fromCamera, cameraTarget, transitionSettings));
        }

        protected virtual void LateUpdate()
        {
            if (_isTransitioning) return;
            CopyCamera(CurrentCamera);
        }

        IEnumerator TransitionCoroutine(ICamera? fromCamera, ICamera cameraTarget, CameraTransitionSettings transitionSettings)
        {
            _isTransitioning = true;
            CurrentCamera = cameraTarget;
            
            float transitionDuration = transitionSettings.transitionDuration;
            
            // If fromCamera is null, we can't transition from anything, so snap.
            if (fromCamera == null)
            {
                transitionDuration = 0f;
            }
            
            if (transitionDuration < 0.01f)
            {
                CopyCamera(cameraTarget);
                _isTransitioning = false;
                yield break;
            }
            
            float elapsedTime = 0f;
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / transitionDuration);
                
                LerpCameras(fromCamera, cameraTarget, t);
                yield return null;
            }
            
            CopyCamera(cameraTarget);
            _isTransitioning = false;
        }

        void LerpCameras(ICamera? fromCamera, ICamera? toCamera, float t)
        {
            if (fromCamera == null || !fromCamera.IsAlive() || toCamera == null || !toCamera.IsAlive()) return;
            _masterCamera.Position = Vector3.Lerp(fromCamera.Position, toCamera.Position, t);
            _masterCamera.Rotation = Quaternion.Slerp(fromCamera.Rotation, toCamera.Rotation, t);
            _masterCamera.FieldOfView = Mathf.Lerp(fromCamera.FieldOfView, toCamera.FieldOfView, t);
            _masterCamera.NearClipPlane = Mathf.Lerp(fromCamera.NearClipPlane, toCamera.NearClipPlane, t);
            _masterCamera.FarClipPlane = Mathf.Lerp(fromCamera.FarClipPlane, toCamera.FarClipPlane, t);
        }

        /// <summary>
        /// Copy everything from the source camera to the master camera, except Rect (so visual is same but maybe not in the same screen position/size).
        /// </summary>
        /// <param name="sourceCamera"></param>
        void CopyCamera(ICamera? sourceCamera)
        {
            if (sourceCamera == null || !sourceCamera.IsAlive()) return;
            _masterCamera.Position = sourceCamera.Position;
            _masterCamera.Rotation = sourceCamera.Rotation;
            _masterCamera.FieldOfView = sourceCamera.FieldOfView;
            _masterCamera.NearClipPlane = sourceCamera.NearClipPlane;
            _masterCamera.FarClipPlane = sourceCamera.FarClipPlane;
        }

        private class SnapshotCamera : ICamera
        {
            public bool IsActive { get; set; }
            public Vector3 Position { get; set; }
            public Quaternion Rotation { get; set; }
            public Transform? Transform => null;
            
            public float FieldOfView { get; set; }
            public float NearClipPlane { get; set; }
            public float FarClipPlane { get; set; }
            public Rect Rect { get; set; }

            public void CopyFrom(ICamera other)
            {
                Position = other.Position;
                Rotation = other.Rotation;
                FieldOfView = other.FieldOfView;
                NearClipPlane = other.NearClipPlane;
                FarClipPlane = other.FarClipPlane;
                Rect = other.Rect;
            }
        }
    }
}