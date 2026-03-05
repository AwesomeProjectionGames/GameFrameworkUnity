using UnityEngine;

namespace UnityGameFrameworkImplementations.Reactive
{
    /// <summary>
    /// A Unity component that smoothly interpolates a float value over time.
    /// It observes changes from a source and exposes the smoothed value for effect systems.
    /// </summary>
    public class SimpleFloatSmoother : ValueConverter<float>
    {
        [SerializeField] private float smoothingSpeed = 5f;
        
        private float _targetValue;
        
        protected override void OnObservedValueChanged(float newValue)
        { 
            _targetValue = newValue;
        }

        private void Update()
        {
            if (Mathf.Approximately(CurrentValue, _targetValue))
                return;

            SetValue(Mathf.Lerp(CurrentValue, _targetValue, Time.deltaTime * smoothingSpeed));
        }
    }
}