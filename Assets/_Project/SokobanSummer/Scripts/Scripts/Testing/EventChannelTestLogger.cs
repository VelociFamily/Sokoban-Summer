using UnityEngine;

namespace SokobanSummer.Testing
{
    /// <summary>
    /// Simple logger for testing ScriptableObject event channels.
    /// Attach to GameObjects with event listener components to verify events fire correctly.
    /// </summary>
    public class EventChannelTestLogger : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private string logPrefix = "[Event Test]";

        /// <summary>
        /// Log a string event (for StringGameEventListener).
        /// </summary>
        public void LogStringEvent(string value)
        {
            if (enableLogging)
            {
                Debug.Log($"{logPrefix} String Event Received: {value}", this);
            }
        }

        /// <summary>
        /// Log an int event (for IntGameEventListener).
        /// </summary>
        public void LogIntEvent(int value)
        {
            if (enableLogging)
            {
                Debug.Log($"{logPrefix} Int Event Received: {value}", this);
            }
        }

        /// <summary>
        /// Log a float event (for FloatGameEventListener).
        /// </summary>
        public void LogFloatEvent(float value)
        {
            if (enableLogging)
            {
                Debug.Log($"{logPrefix} Float Event Received: {value}", this);
            }
        }

        /// <summary>
        /// Log a void event (for GameEventListener).
        /// </summary>
        public void LogVoidEvent()
        {
            if (enableLogging)
            {
                Debug.Log($"{logPrefix} Void Event Received", this);
            }
        }

        /// <summary>
        /// Log with a custom message (useful for identifying which event fired).
        /// </summary>
        public void LogCustomMessage(string message)
        {
            if (enableLogging)
            {
                Debug.Log($"{logPrefix} {message}", this);
            }
        }
    }
}
