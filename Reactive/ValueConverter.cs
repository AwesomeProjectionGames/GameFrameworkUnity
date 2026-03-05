
using System;
using System.Collections.Generic;
using GameFramework.Reactive;
using UnityEngine;

namespace UnityGameFrameworkImplementations.Reactive
{
    /// <summary>
    /// Base Unity component for transforming or filtering reactive values.
    /// It observes a source, applies logic, and exposes the result via <see cref="IValueObservable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value being observed and converted.</typeparam>
    public abstract class ValueConverter<T> : ValueObserverMonoBehaviour<T>, IValueObservable<T>
    {
        public string Name => transformedChannelName;
        public T CurrentValue { get; protected set; }
        
        [SerializeField] private string transformedChannelName = "TransformedValue";
        
        protected readonly List<IObserver<T>> Observers = new();

        /// <summary>
        /// Sets the exposed value.
        /// If the value changes, notifies listeners.
        /// </summary>
        /// <param name="value">New value to expose.</param>
        protected virtual void SetValue(T value)
        {
            if (Equals(CurrentValue, value))
                return;

            CurrentValue = value;
            Observers.Notify(value);
        }
        
        public IDisposable Subscribe(IObserver<T> observer)
        {
            return Observers.SubscribeAndNotify(observer, CurrentValue);
        }
    }
}