#nullable enable
using AwesomeProjectionCoreUtils.Extensions;
using UnityEngine;

namespace GameFramework.Effects
{
    /// <summary>
    /// Abstract base class for objects that react to simple value changes
    /// (like health, stamina, energy, etc.) via an exposed value source.
    /// </summary>
    /// <typeparam name="T">The type of the observed value.</typeparam>
    public abstract class SimpleValueReactorMonoBehaviour<T> : MonoBehaviour
    {
        [SerializeField, Tooltip("Optional value source to observe.")]
        private IExposeSimpleValue<T>? _observedSource;

        /// <summary>
        /// The source of the simple value to observe.
        /// Setting this will automatically unsubscribe from the old source and subscribe to the new one.
        /// </summary>
        public IExposeSimpleValue<T>? ObservedSource
        {
            get => _observedSource;
            set
            {
                if (_observedSource == value)
                    return;

                var oldSource = _observedSource;
                _observedSource = value;
                OnObservedSourceChanged(oldSource, _observedSource);
            }
        }


        /// <summary>
        /// Called when the observed source changes.
        /// </summary>
        /// <param name="oldSource">The previous source.</param>
        /// <param name="newSource">The new source.</param>
        protected virtual void OnObservedSourceChanged(IExposeSimpleValue<T>? oldSource,
            IExposeSimpleValue<T>? newSource)
        {
            // Unsubscribe from old source
            if (oldSource.IsAlive())
                oldSource!.OnValueChanged -= OnObservedValueChanged;

            // Subscribe to new source
            if (newSource.IsAlive())
            {
                newSource!.OnValueChanged += OnObservedValueChanged;

                // Trigger initial value update
                OnObservedValueChanged(newSource.CurrentValue);
            }
        }

        /// <summary>
        /// Called when the observed value changes.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        protected abstract void OnObservedValueChanged(T newValue);

        protected virtual void OnDestroy()
        {
            // Clean up subscriptions
            if (_observedSource != null)
                _observedSource.OnValueChanged -= OnObservedValueChanged;
        }
    }
}
