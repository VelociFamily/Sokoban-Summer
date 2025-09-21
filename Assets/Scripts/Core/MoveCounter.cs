using TMPro;
using UnityEngine;

namespace Core
{
    public class MoveCounter : MonoBehaviour
    {
        public static MoveCounter Instance;

        [Header("Move Counter")]
        public int moveCount;
        public TextMeshProUGUI moveText;

        [Header("Timer")]
        public TextMeshProUGUI timerText;
        public GameObject levelCompleteCanvas; // Canvas that ends the timer

        private float timer;
        private bool timerRunning = true;

        /// <summary>
        /// Initialize the MoveCounter - can be called by other systems
        /// </summary>
        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                timer = 0f;
                timerRunning = true;
                Debug.Log("[MoveCounter]: Instance initialized");
            }
            else
            {
                Debug.LogWarning($"[MoveCounter]: Duplicate instance detected on '{gameObject.name}' - destroying");
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            // Timer logic
            if (timerRunning)
            {
                timer += Time.deltaTime;

                if (timerText != null)
                {
                    var minutes = Mathf.FloorToInt(timer / 60f);
                    var seconds = Mathf.FloorToInt(timer % 60f);
                    var milliseconds = Mathf.FloorToInt((timer * 100f) % 100f); // Get hundredths

                    timerText.text = $"Time: {minutes:00}:{seconds:00}.{milliseconds:00}";
                }

                // Stop timer if levelCompleteCanvas is active
                if (levelCompleteCanvas != null && levelCompleteCanvas.activeSelf)
                {
                    timerRunning = false;
                }
            }
        }

        public void IncrementMove()
        {
            moveCount++;
        
            if (moveText != null)
                moveText.text = "Moves: " + moveCount;
            else
                Debug.LogWarning("[MoveCounter]: Move text UI component not assigned - cannot update display");
        }

        public void ResetCounter()
        {
            moveCount = 0;
            timer = 0f;
            timerRunning = true;
        
            Debug.Log("[MoveCounter]: Game counters reset for new attempt");
        
            if (moveText != null)
                moveText.text = "Moves: 0";
            else
                Debug.LogWarning("[MoveCounter]: Move text UI component not assigned - cannot update display");

            if (timerText != null)
                timerText.text = "Time: 00:00.00";
            else
                Debug.LogWarning("[MoveCounter]: Timer text UI component not assigned - cannot update display");
        }
        public float GetElapsedTime()
        {
            return timer;
        }
    }
}
