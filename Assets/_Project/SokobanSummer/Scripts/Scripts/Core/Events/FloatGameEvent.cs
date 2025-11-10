using UnityEngine;
using UnityEngine.Events;

namespace Core.Events
{
    /// <summary>
    /// Game event that passes a float value
    /// Use for: timer values, progress percentages, speed multipliers, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "New Float Event", menuName = "Sokoban Summer/Events/Float Event")]
    public class FloatGameEvent : GenericGameEvent<float>
    {
    }

    /// <summary>
    /// MonoBehaviour listener for FloatGameEvent
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
