using UnityEngine;

namespace GameFramework.Effects
{
    /// <summary>
    /// Unity-specific abstract base class used to expose a simple value of type <typeparamref name="T"/>
    /// through a MonoBehaviour, so visual effect systems (VFX Graph, shaders, UI, etc.)
    /// can bind and react to it.
    /// </summary>
    /// <typeparam name="T">The type of the value to expose.</typeparam>
    public abstract class ExposeSimpleValueMonoBehaviour<T> : MonoBehaviour, IExposeSimpleValue<T>
    {
        [SerializeField]
        [Tooltip("Identifier/channel used for auto-binding in effect systems.")]
        protected string _name = string.Empty;
        
        public string Name => _name;
        public T CurrentValue { get; protected set; } = default!;
        public event System.Action<T>? OnValueChanged;

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
            OnValueChanged?.Invoke(CurrentValue);
        }
    }
}