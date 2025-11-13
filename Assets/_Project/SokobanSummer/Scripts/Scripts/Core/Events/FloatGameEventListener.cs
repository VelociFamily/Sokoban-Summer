using UnityEngine;
using UnityEngine.Events;

namespace Core.Events
{
    /// <summary>
    /// MonoBehaviour listener for FloatGameEvent.
    /// Separate file so Unity can expose it in Add Component.
    /// </summary>
    public class FloatGameEventListener : MonoBehaviour, IGenericGameEventListener<float>
    {
        [Header("Event Channel")]
        [Tooltip("The FloatGameEvent ScriptableObject to listen to")]
        [SerializeField] private FloatGameEvent gameEvent;

        [Header("Response")]
        [Tooltip("Unity event invoked when the FloatGameEvent is raised (receives float parameter)")]
        [SerializeField] private UnityEvent<float> response;

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

        public void OnEventRaised(float data)
        {
            response?.Invoke(data);
        }
    }
}
