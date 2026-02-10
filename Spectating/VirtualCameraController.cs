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
        }

        public void TransitionToCamera(ICamera cameraTarget)
        {
            TransitionToCamera(cameraTarget, new CameraTransitionSettings { transitionDuration = 0f });
        }

        public void TransitionToCamera(ICamera cameraTarget, CameraTransitionSettings transitionSettings)
        {
            if (_isTransitioning) StopAllCoroutines();
            StartCoroutine(TransitionCoroutine(cameraTarget, transitionSettings));
        }

        protected virtual void LateUpdate()
        {
            if (_isTransitioning) return;
            CopyCamera(CurrentCamera);
        }

        IEnumerator TransitionCoroutine(ICamera cameraTarget, CameraTransitionSettings transitionSettings)
        {
            _isTransitioning = true;
            ICamera? previousCamera = CurrentCamera;
            ICamera nextCamera = cameraTarget;
            CurrentCamera = nextCamera;
            float transitionDuration = transitionSettings.transitionDuration;
            if (previousCamera == null)
            {
                transitionDuration = 0f;
            }
            if (transitionDuration < 0.01f)
            {
                CopyCamera(nextCamera);
                _isTransitioning = false;
                yield break;
            }
            float elapsedTime = 0f;
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / transitionDuration);
                LerpCameras(previousCamera!, nextCamera, t);
                yield return null;
            }
            CopyCamera(nextCamera);
            _isTransitioning = false;
        }

        void LerpCameras(ICamera fromCamera, ICamera toCamera, float t)
        {
            _masterCamera.Transform.position = Vector3.Lerp(fromCamera.Transform.position, toCamera.Transform.position, t);
            _masterCamera.Transform.rotation = Quaternion.Slerp(fromCamera.Transform.rotation, toCamera.Transform.rotation, t);
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
            if (!sourceCamera.IsAlive()) return;
            _masterCamera.Transform.position = sourceCamera.Transform.position;
            _masterCamera.Transform.rotation = sourceCamera.Transform.rotation;
            _masterCamera.FieldOfView = sourceCamera.FieldOfView;
            _masterCamera.NearClipPlane = sourceCamera.NearClipPlane;
            _masterCamera.FarClipPlane = sourceCamera.FarClipPlane;
        }
    }
}