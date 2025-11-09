using UnityEngine;
using UnityEngine.Events;

namespace Core.Events
{
    /// <summary>
    /// Game event that passes a string value
    /// Use for: level names, power-up types, messages, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "New String Event", menuName = "Sokoban Summer/Events/String Event")]
    public class StringGameEvent : GenericGameEvent<string>
    {
    }

    /// <summary>
    /// MonoBehaviour listener for StringGameEvent
    /// </summary>
    public class StringGameEventListener : MonoBehaviour, IGenericGameEventListener<string>
    {
        [Header("Event Channel")]
        [Tooltip("The StringGameEvent ScriptableObject to listen to")]
        [SerializeField] private StringGameEvent gameEvent;

        [Header("Response")]
        [Tooltip("Unity event invoked when the StringGameEvent is raised (receives string parameter)")]
        [SerializeField] private UnityEvent<string> response;

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

        public void OnEventRaised(string data)
        {
            response?.Invoke(data);
        }
    }
}
