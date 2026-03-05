#nullable enable
using System;
using System.Linq;
using AwesomeProjectionCoreUtils.Extensions;
using GameFramework.Reactive;
using UnityEngine;

namespace UnityGameFrameworkImplementations.Reactive
{
    /// <summary>
    /// Abstract base class for objects that react to simple value changes
    /// (like health, stamina, energy, etc.) via an exposed value source.
    /// </summary>
    /// <typeparam name="T">The type of the observed value.</typeparam>
    public abstract class ValueObserverMonoBehaviour<T> : MonoBehaviour, IValueObserver<T>
    {
        [SerializeField, Tooltip("Optional value source to observe.")]
        private IValueObservable<T>? _observedSource;
        
        [SerializeField]
        [Tooltip("If no source is assigned, try to find one on Awake in object or parents")]
        private bool tryAutoFindOnAwake = true;
        
        [SerializeField]
        [Tooltip("Channels name of a SimpleValue to observe automatically (if tryAutoFindOnAwake is true).")]
        private string exposedSimpleValueChannelName = "";
        
        private IDisposable? _subscriptionDisposable;

        /// <summary>
        /// The source of the simple value to observe.
        /// Setting this will automatically unsubscribe from the old source and subscribe to the new one.
        /// </summary>
        public IValueObservable<T>? ObservedSource
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

        protected virtual void Awake()
        {
            if (tryAutoFindOnAwake && !_observedSource.IsAlive())
            {
                //Try to find the first matching source in this object or parents (with exposedSimpleValueChannelName)
                var foundSource = GetComponentsInParent<IValueObservable<T>>()
                    .FirstOrDefault(source => string.IsNullOrEmpty(exposedSimpleValueChannelName) ||
                                              source.Name == exposedSimpleValueChannelName);
                if (foundSource.IsAlive())
                {
                    ObservedSource = foundSource;
                }
            }
        }


        /// <summary>
        /// Called when the observed source changes.
        /// </summary>
        /// <param name="oldSource">The previous source.</param>
        /// <param name="newSource">The new source.</param>
        protected virtual void OnObservedSourceChanged(IValueObservable<T>? oldSource,
            IValueObservable<T>? newSource)
        {
            // Unsubscribe from old source
            CleanupSubscription();

            // Subscribe to new source
            if (newSource.IsAlive())
            {
                newSource!.Subscribe(this);
            }
        }

        /// <summary>
        /// Called when the observed value changes.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        protected abstract void OnObservedValueChanged(T newValue);

        protected virtual void OnDestroy()
        {
            CleanupSubscription();
        }
        
        private void CleanupSubscription()
        {
            if (_subscriptionDisposable != null)
            {
                _subscriptionDisposable.Dispose();
                _subscriptionDisposable = null;
            }
        }

        public virtual void OnCompleted() {}

        public virtual void OnError(Exception error){}

        public virtual void OnNext(T value)
        {
            OnObservedValueChanged(value);
        }
    }
}
