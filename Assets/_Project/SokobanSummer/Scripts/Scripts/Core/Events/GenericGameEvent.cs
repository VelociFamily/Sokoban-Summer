using System.Collections.Generic;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Generic base class for game events that pass data of type T
    /// </summary>
    public abstract class GenericGameEvent<T> : ScriptableObject
    {
        private readonly List<IGenericGameEventListener<T>> listeners = new List<IGenericGameEventListener<T>>();

        /// <summary>
        /// Raise the event with data, notifying all registered listeners
        /// </summary>
        public void Raise(T data)
        {
            // Iterate backwards in case listeners unregister during callback
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised(data);
            }
        }

        /// <summary>
        /// Register a listener to receive callbacks when this event is raised
        /// </summary>
        public void RegisterListener(IGenericGameEventListener<T> listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        /// <summary>
        /// Unregister a listener from this event
        /// </summary>
        public void UnregisterListener(IGenericGameEventListener<T> listener)
        {
            listeners.Remove(listener);
        }

        /// <summary>
        /// Clear all listeners (useful for cleanup or testing)
        /// </summary>
        public void ClearListeners()
        {
            listeners.Clear();
        }
    }

    /// <summary>
    /// Interface for components that listen to generic game events
    /// </summary>
    public interface IGenericGameEventListener<T>
    {
        void OnEventRaised(T data);
    }
}
