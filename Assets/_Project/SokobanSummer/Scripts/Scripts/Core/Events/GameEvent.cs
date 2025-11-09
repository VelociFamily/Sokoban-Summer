using System.Collections.Generic;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Base class for ScriptableObject-based game events (void/no parameters)
    /// Use this for simple signals like "LevelCompleted", "GamePaused", etc.
    /// </summary>
    [CreateAssetMenu(fileName = "New Game Event", menuName = "Sokoban Summer/Events/Game Event (Void)")]
    public class GameEvent : ScriptableObject
    {
        private readonly List<GameEventListener> listeners = new List<GameEventListener>();

        /// <summary>
        /// Raise the event, notifying all registered listeners
        /// </summary>
        public void Raise()
        {
            // Iterate backwards in case listeners unregister during callback
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised();
            }
        }

        /// <summary>
        /// Register a listener to receive callbacks when this event is raised
        /// </summary>
        public void RegisterListener(GameEventListener listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        /// <summary>
        /// Unregister a listener from this event
        /// </summary>
        public void UnregisterListener(GameEventListener listener)
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
}
