using UnityEngine;
using UnityEngine.Events;

namespace Core.Events
{
    /// <summary>
    /// MonoBehaviour listener for IntGameEvent.
    /// Separate file so Unity can expose it in Add Component.
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
