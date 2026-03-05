using GameFramework;
using UnityEngine;

namespace UnityGameFrameworkImplementations.Reactive
{
    [RequireComponent(typeof(ICamera))]
    public class CurveCameraObserver : ValueObserverMonoBehaviour<float>
    {
        public enum CameraProperty
        {
            FieldOfView,
            NearClipPlane,
            FarClipPlane
        }

        [SerializeField] private AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private CameraProperty propertyToAffect;
        [SerializeField] private float curveStartValue = 0f;
        [SerializeField] private float curveEndValue = 1f;

        private ICamera _camera;

        protected override void Awake()
        {
            _camera = GetComponent<ICamera>();
            base.Awake();
        }

        protected override void OnObservedValueChanged(float newValue)
        {
            float curveValue = Mathf.Lerp(
                curveStartValue,
                curveEndValue,
                curve.Evaluate(newValue)
            );

            switch (propertyToAffect)
            {
                case CameraProperty.FieldOfView:
                    _camera.FieldOfView = curveValue;
                    break;
                case CameraProperty.NearClipPlane:
                    _camera.NearClipPlane = curveValue;
                    break;
                case CameraProperty.FarClipPlane:
                    _camera.FarClipPlane = curveValue;
                    break;
            }
        }
    }
}