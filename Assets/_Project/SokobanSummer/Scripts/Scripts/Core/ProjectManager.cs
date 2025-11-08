using TMPro;
using UnityEngine;

namespace Core
{
    public class ProjectManager : MonoBehaviour
    {
        [Header("Level Completion Trigger")]
        public GameObject levelCompleteCanvas;

        [Header("Performance Targets")]
        public int moveThreshold = 10;         // Under this = 2 stars
        public float timeThreshold = 30f;      // Under this = 3 stars

        [Header("Star UI")]
        public GameObject[] stars;             // 0: 1 star, 1: 2 stars, 2: 3 stars

        [Header("TextMeshPro UI")]
        public TMP_Text completionMessageText; // Assign in inspector, should start inactive

        private bool levelCompleted;

        private void Update()
        {
            if (!levelCompleted && levelCompleteCanvas != null && levelCompleteCanvas.activeSelf)
            {
                EvaluatePerformance();
                ShowCompletionMessage();
                levelCompleted = true;
            }
        }

        public void EvaluatePerformance()
        {
            if (!ServiceLocator.TryGet<MoveCounter>(out var moveCounter) || moveCounter == null)
            {
                Debug.LogError("[ProjectManager]: MoveCounter not registered in ServiceLocator - cannot evaluate performance");
                return;
            }

            var movesUsed = moveCounter.moveCount;
            var timeUsed = moveCounter.GetElapsedTime();

            // Award base 1 star
            if (stars.Length > 0 && stars[0] != null)
                stars[0].SetActive(true);

            // 2 stars if move threshold is met
            if (movesUsed <= moveThreshold && stars.Length > 1 && stars[1] != null)
                stars[1].SetActive(true);

            // 3 stars if time threshold is met
            if (timeUsed <= timeThreshold && stars.Length > 2 && stars[2] != null)
                stars[2].SetActive(true);

            Debug.Log($"[ProjectManager]: Level completed - Performance: {movesUsed} moves, {timeUsed:F2}s (Targets: ≤{moveThreshold} moves, ≤{timeThreshold:F2}s)");
        }

        private void ShowCompletionMessage()
        {
            if (completionMessageText == null)
            {
                Debug.LogWarning("[ProjectManager]: Completion message text component not assigned - cannot display completion tips");
                return;
            }

            // Compose message showing thresholds for the player
            var message = $"Try to finish with:\n" +
                          $"- Moves ≤ {moveThreshold}\n" +
                          $"- Time ≤ {timeThreshold:F2} seconds";

            completionMessageText.text = message;
            completionMessageText.gameObject.SetActive(true);
        }
    }
}
