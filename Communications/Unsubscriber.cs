using System;
using System.Collections.Generic;

namespace UnityGameFrameworkImplementations.Communications
{
    /// <summary>
    /// Handles unsubscription from an observable sequence.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the observable sequence.</typeparam>
    public class Unsubscriber<T> : IDisposable
    {
        private readonly List<IObserver<T>> _observers;
        private readonly IObserver<T> _observer;
        
        /// <summary>
        /// Create a new unsubscriber to remove the observer from the observers list on Dispose.
        /// </summary>
        /// <param name="observers">The list of observers (reference) to remove from.</param>
        /// <param name="observer">The observer to remove.</param>
        public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        /// <summary>
        /// Dispose method to remove the observer from the observers list (from instance creation).
        /// </summary>
        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
            {
                _observers.Remove(_observer);
            }
        }
    }
}