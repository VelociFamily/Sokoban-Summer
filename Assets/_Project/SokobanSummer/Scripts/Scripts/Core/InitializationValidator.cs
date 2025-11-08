using System.Threading.Tasks;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Simple validation script to test the new async initialization system
    /// </summary>
    public class InitializationValidator : MonoBehaviour
    {
        [Header("Validation Settings")]
        public bool runValidationOnStart = true;
        public float validationDelay = 2.0f;

        private async void Start()
        {
            if (runValidationOnStart)
            {
                Debug.Log("[InitializationValidator]: Starting validation in " + validationDelay + " seconds...");
                await Task.Delay((int)(validationDelay * 1000));
                await ValidateInitializationAsync();
            }
        }

        /// <summary>
        /// Validate that all services are properly initialized
        /// </summary>
        public async Task ValidateInitializationAsync()
        {
            Debug.Log("[InitializationValidator]: Beginning validation of initialization system...");

            var validationResults = new System.Text.StringBuilder();
            validationResults.AppendLine("=== Initialization Validation Results ===");

            // Validate InputService
            ValidateInputService(validationResults);

            // Validate AchievementManager
            ValidateAchievements(validationResults);

            // Validate MoveCounter
            ValidateMoveCounter(validationResults);

            validationResults.AppendLine("=== End Validation Results ===");
            Debug.Log(validationResults.ToString());

            await Task.Yield();
        }

        private void ValidateInputService(System.Text.StringBuilder results)
        {
            try
            {
                if (!ServiceLocator.TryGet<InputService>(out var inputService) || inputService == null)
                {
                    results.AppendLine("✗ InputService: Not registered in ServiceLocator");
                    return;
                }
                
                var inputActions = inputService.InputActions;

                results.AppendLine($"✓ InputService: Initialized");
                results.AppendLine($"  - InputActions: {(inputActions != null ? "✓ Found" : "✗ Missing")}");
            }
            catch (System.Exception ex)
            {
                results.AppendLine($"✗ InputService: Failed - {ex.Message}");
            }
        }

        private void ValidateAchievements(System.Text.StringBuilder results)
        {
            try
            {
                if (!ServiceLocator.TryGet<AchievementManager>(out var achievementManager) || achievementManager == null)
                {
                    results.AppendLine("✗ AchievementManager: Not registered in ServiceLocator");
                    return;
                }
            
                results.AppendLine($"✓ AchievementManager: ✓ Found");
            }
            catch (System.Exception ex)
            {
                results.AppendLine($"✗ AchievementManager: Failed - {ex.Message}");
            }
        }

        private void ValidateMoveCounter(System.Text.StringBuilder results)
        {
            try
            {
                if (!ServiceLocator.TryGet<MoveCounter>(out var moveCounter) || moveCounter == null)
                {
                    results.AppendLine("✗ MoveCounter: Not registered in ServiceLocator");
                    return;
                }
            
                results.AppendLine($"✓ MoveCounter: ✓ Found");
            }
            catch (System.Exception ex)
            {
                results.AppendLine($"✗ MoveCounter: Failed - {ex.Message}");
            }
        }

        /// <summary>
        /// Manual validation trigger for testing
        /// </summary>
        [ContextMenu("Run Validation")]
        public async void RunValidation()
        {
            await ValidateInitializationAsync();
        }
    }
}
