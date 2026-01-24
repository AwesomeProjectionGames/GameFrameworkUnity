using System;
using System.Collections.Generic;

namespace UnityGameFrameworkImplementations.Communications
{
    public static class IValueObservableExtension
    {
        /// <summary>
        /// Notifies observers (don't check if value is different).
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="observers">The list of observers managing the subscriptions.</param>
        /// <param name="newValue">The new value to set.</param>
        public static void Notify<T>(this List<IObserver<T>> observers, T newValue)
        {
            for (int i = observers.Count - 1; i >= 0; i--)
            {
                observers[i].OnNext(newValue);
            }
        }

        /// <summary>
        /// Subscribes an observer, adds it to the list, and immediately pushes the current value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="observers">The list of observers managing the subscriptions.</param>
        /// <param name="observer">The subscriber.</param>
        /// <param name="currentValue">The current value to push immediately upon subscription.</param>
        /// <returns>A disposable that removes the observer from the list.</returns>
        public static IDisposable SubscribeAndNotify<T>(this List<IObserver<T>> observers, IObserver<T> observer, T currentValue)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
                // Push the current value to the new subscriber immediately
                observer.OnNext(currentValue);
            }
            return new Unsubscriber<T>(observers, observer);
        }
}