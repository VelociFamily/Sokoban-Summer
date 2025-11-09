using Core;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Binds a TextMeshProUGUI component to MoveCounter's OnMovesChanged event
    /// Add this component to a GameObject with TextMeshProUGUI to display move count
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class MoveCounterUIBinding : MonoBehaviour
    {
        [Header("UI Reference")]
        [Tooltip("The TextMeshProUGUI component to update (auto-assigned if not set)")]
        [SerializeField] private TextMeshProUGUI moveText;

        [Header("Formatting")]
        [Tooltip("Format string for displaying moves. Use {0} for the move count.")]
        [SerializeField] private string formatString = "Moves: {0}";

        private MoveCounter moveCounter;

        private void Awake()
        {
            // Auto-assign TextMeshProUGUI if not set
            if (moveText == null)
            {
                moveText = GetComponent<TextMeshProUGUI>();
            }
        }

        private void OnEnable()
        {
            // Get MoveCounter from ServiceLocator
            if (ServiceLocator.TryGet<MoveCounter>(out var counter))
            {
                moveCounter = counter;
                moveCounter.OnMovesChanged += OnMovesChanged;
                
                // Initialize with current value
                UpdateDisplay(moveCounter.moveCount);
            }
            else
            {
                Debug.LogWarning($"[MoveCounterUIBinding]: MoveCounter not found in ServiceLocator on '{gameObject.name}'");
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            if (moveCounter != null)
            {
                moveCounter.OnMovesChanged -= OnMovesChanged;
            }
        }

        private void OnMovesChanged(object sender, MoveCountChangedEventArgs e)
        {
            UpdateDisplay(e.MoveCount);
        }

        private void UpdateDisplay(int moveCount)
        {
            if (moveText != null)
            {
                moveText.text = string.Format(formatString, moveCount);
            }
        }
    }
}
