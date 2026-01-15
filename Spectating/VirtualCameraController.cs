#nullable enable

using System.Collections;
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
        
        private ICamera _masterCamera = null!;
        private bool _isTransitioning = false;

        protected virtual void Awake()
        {
            _masterCamera = GetComponent<ICamera>();
        }

        public void TransitionToCamera(ICamera cameraTarget)
        {
            TransitionToCamera(cameraTarget, new CameraTransitionSettings{ transitionDuration=0f });
        }

        public void TransitionToCamera(ICamera cameraTarget, CameraTransitionSettings transitionSettings)
        {
            if(_isTransitioning) StopAllCoroutines();
            StartCoroutine(TransitionCoroutine(cameraTarget, transitionSettings));
        }

        private void LateUpdate()
        {
            if(_isTransitioning || CurrentCamera == null) return;
            CopyCamera(CurrentCamera);
        }

        IEnumerator TransitionCoroutine(ICamera cameraTarget, CameraTransitionSettings transitionSettings)
        {
            _isTransitioning = true;
            ICamera? previousCamera = CurrentCamera;
            ICamera nextCamera = cameraTarget;
            CurrentCamera = nextCamera;
            float transitionDuration = transitionSettings.transitionDuration;
            if(previousCamera == null)
            {
                transitionDuration = 0f;
            }
            if(transitionDuration < 0.01f)
            {
                CopyCamera(nextCamera);
                _isTransitioning = false;
                yield break;
            }
            float elapsedTime = 0f;
            while(elapsedTime < transitionDuration)
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
        
        void CopyCamera(ICamera sourceCamera)
        {
            _masterCamera.Transform.position = sourceCamera.Transform.position;
            _masterCamera.Transform.rotation = sourceCamera.Transform.rotation;
            _masterCamera.FieldOfView = sourceCamera.FieldOfView;
            _masterCamera.NearClipPlane = sourceCamera.NearClipPlane;
            _masterCamera.FarClipPlane = sourceCamera.FarClipPlane;
        }
    }
}