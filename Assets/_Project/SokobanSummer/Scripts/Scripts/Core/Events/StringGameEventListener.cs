using UnityEngine;
using UnityEngine.Events;

namespace Core.Events
{
    /// <summary>
    /// MonoBehaviour listener for StringGameEvent.
    /// File name matches class name so Unity can add it via Add Component.
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
