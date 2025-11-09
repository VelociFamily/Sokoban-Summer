using UnityEngine;
using UnityEngine.Events;

namespace Core.Events
{
    /// <summary>
    /// MonoBehaviour component that listens to a GameEvent and invokes a UnityEvent response
    /// Attach this to GameObjects to respond to ScriptableObject event channels
    /// </summary>
    public class GameEventListener : MonoBehaviour
    {
        [Header("Event Channel")]
        [Tooltip("The GameEvent ScriptableObject to listen to")]
        [SerializeField] private GameEvent gameEvent;

        [Header("Response")]
        [Tooltip("Unity event invoked when the GameEvent is raised")]
        [SerializeField] private UnityEvent response;

        private void OnEnable()
        {
            if (gameEvent != null)
            {
                gameEvent.RegisterListener(this);
            }
        }

        private void OnDisable()
        {
            if (gameEvent != null)
            {
                gameEvent.UnregisterListener(this);
            }
        }

        /// <summary>
        /// Called by the GameEvent when it's raised
        /// </summary>
        public void OnEventRaised()
        {
            response?.Invoke();
        }
    }
}
