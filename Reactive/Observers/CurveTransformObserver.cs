using UnityEngine;

namespace UnityGameFrameworkImplementations.Reactive
{
    /// <summary>
    /// Reacts to a float value by transforming a GameObject's position, rotation, or scale based on an AnimationCurve.
    /// </summary>
    public class CurveTransformObserver : ValueObserverMonoBehaviour<float>
    {
        public enum TransformProperty
        {
            PositionX,
            PositionY,
            PositionZ,
            RotationX,
            RotationY,
            RotationZ,
            ScaleX,
            ScaleY,
            ScaleZ
        }
        public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public TransformProperty propertyToAffect = TransformProperty.PositionY;
        public float curveStartValue = 0f;
        public float curveEndValue = 1f;
        
        protected override void OnObservedValueChanged(float newValue)
        {
            float curveValue = Mathf.Lerp(curveStartValue, curveEndValue, curve.Evaluate(newValue));
            Vector3 currentPosition = transform.localPosition;
            Vector3 currentRotation = transform.localEulerAngles;
            Vector3 currentScale = transform.localScale;
            switch (propertyToAffect)
            {
                case TransformProperty.PositionX:
                    transform.localPosition = new Vector3(curveValue, currentPosition.y, currentPosition.z);
                    break;
                case TransformProperty.PositionY:
                    transform.localPosition = new Vector3(currentPosition.x, curveValue, currentPosition.z);
                    break;
                case TransformProperty.PositionZ:
                    transform.localPosition = new Vector3(currentPosition.x, currentPosition.y, curveValue);
                    break;
                case TransformProperty.RotationX:
                    transform.localEulerAngles = new Vector3(curveValue, currentRotation.y, currentRotation.z);
                    break;
                case TransformProperty.RotationY:
                    transform.localEulerAngles = new Vector3(currentRotation.x, curveValue, currentRotation.z);
                    break;
                case TransformProperty.RotationZ:
                    transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, curveValue);
                    break;
                case TransformProperty.ScaleX:
                    transform.localScale = new Vector3(curveValue, currentScale.y, currentScale.z);
                    break;
                case TransformProperty.ScaleY:
                    transform.localScale = new Vector3(currentScale.x, curveValue, currentScale.z);
                    break;
                case TransformProperty.ScaleZ:
                    transform.localScale = new Vector3(currentScale.x, currentScale.y, curveValue);
                    break;
            }
        }
    }
}