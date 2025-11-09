using Core;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Binds a TextMeshProUGUI component to MoveCounter's OnTimerChanged event
    /// Add this component to a GameObject with TextMeshProUGUI to display elapsed time
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TimerUIBinding : MonoBehaviour
    {
        [Header("UI Reference")]
        [Tooltip("The TextMeshProUGUI component to update (auto-assigned if not set)")]
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Formatting")]
        [Tooltip("Format string for displaying time. Use {0} for minutes, {1} for seconds, {2} for hundredths.")]
        [SerializeField] private string formatString = "Time: {0:00}:{1:00}.{2:00}";

        private MoveCounter moveCounter;

        private void Awake()
        {
            // Auto-assign TextMeshProUGUI if not set
            if (timerText == null)
            {
                timerText = GetComponent<TextMeshProUGUI>();
            }
        }

        private void OnEnable()
        {
            // Get MoveCounter from ServiceLocator
            if (ServiceLocator.TryGet<MoveCounter>(out var counter))
            {
                moveCounter = counter;
                moveCounter.OnTimerChanged += OnTimerChanged;
                
                // Initialize with current value
                UpdateDisplay(moveCounter.GetElapsedTime());
            }
            else
            {
                Debug.LogWarning($"[TimerUIBinding]: MoveCounter not found in ServiceLocator on '{gameObject.name}'");
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            if (moveCounter != null)
            {
                moveCounter.OnTimerChanged -= OnTimerChanged;
            }
        }

        private void OnTimerChanged(object sender, TimerChangedEventArgs e)
        {
            UpdateDisplay(e.ElapsedTime);
        }

        private void UpdateDisplay(float elapsedTime)
        {
            if (timerText != null)
            {
                var minutes = Mathf.FloorToInt(elapsedTime / 60f);
                var seconds = Mathf.FloorToInt(elapsedTime % 60f);
                var hundredths = Mathf.FloorToInt((elapsedTime * 100f) % 100f);
                
                timerText.text = string.Format(formatString, minutes, seconds, hundredths);
            }
        }
    }
}
