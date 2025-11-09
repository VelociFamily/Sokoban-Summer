using UnityEngine;
using UnityEngine.Events;

namespace Core.Events
{
    /// <summary>
    /// Game event that passes an integer value
    /// Use for: move counts, scores, level numbers, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "New Int Event", menuName = "Sokoban Summer/Events/Int Event")]
    public class IntGameEvent : GenericGameEvent<int>
    {
    }

    /// <summary>
    /// MonoBehaviour listener for IntGameEvent
    /// </summary>
    public class IntGameEventListener : MonoBehaviour, IGenericGameEventListener<int>
    {
        [Header("Event Channel")]
        [Tooltip("The IntGameEvent ScriptableObject to listen to")]
        [SerializeField] private IntGameEvent gameEvent;

        [Header("Response")]
        [Tooltip("Unity event invoked when the IntGameEvent is raised (receives int parameter)")]
        [SerializeField] private UnityEvent<int> response;

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

        public void OnEventRaised(int data)
        {
            response?.Invoke(data);
        }
    }
}
