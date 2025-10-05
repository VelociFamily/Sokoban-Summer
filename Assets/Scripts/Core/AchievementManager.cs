using System.Collections;
using System.Threading.Tasks;
using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core;

namespace Core
{
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        // Public properties to access the state without allowing direct modification
        [Header("Achievement States")]
        public bool ConfuseAndSpeed { get; private set; }

        public bool CompleteTutorial { get; private set; }

        public bool CompleteLevelTwo { get; private set; }

        // Flags to prevent premature unlocking


        [Header("Selected Hat")]
        public string selectedHatName = ""; // stores chosen hat name

        private TextMeshProUGUI achievementText;

        private Coroutine hideAchievementCoroutine;
        private float displayDuration = 5f;

        private bool confuseAndSpeedUnlocked;


        /// <summary>
        /// Initialize the AchievementManager asynchronously
        /// </summary>
        public async Task InitializeAsync()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;

            // Initialize default unlocks (first level should always be unlocked)
            InitializeDefaultUnlocks();

            // Load persisted achievement state if available
            if (SaveFacade.Instance != null)
            {
                var a = SaveFacade.Instance.Achievements;
                if (a != null)
                {
                    ConfuseAndSpeed = a.confuseAndSpeed;
                    CompleteTutorial = a.completeTutorial || CompleteTutorial; // preserve default unlock
                    CompleteLevelTwo = a.completeLevelTwo;
                    selectedHatName = a.selectedHatName ?? selectedHatName;
                }
            }

            // Initialize UI elements
            await InitializeUIAsync();

            Debug.Log("[AchievementManager]: Initialized asynchronously with default unlocks");
        }

        private async Task InitializeUIAsync()
        {
            FindAchievementText();
            UpdateAchievementDisplay(true);
            await Task.Yield(); // Ensure async behavior
        }

        /// <summary>
        /// Initialize default unlocks (first level should always be unlocked)
        /// </summary>
        private void InitializeDefaultUnlocks()
        {
            // Always unlock the tutorial level by default
            CompleteTutorial = true;
            Debug.Log("[AchievementManager]: Tutorial level unlocked by default on game start");
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (hideAchievementCoroutine != null)
            {
                StopCoroutine(hideAchievementCoroutine);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            FindAchievementText();
            UpdateAchievementDisplay(true);

            // Check if the loaded scene is the final tutorial completion scene (Level Two)
            if (!SceneInfo.IsLevelTwo(scene)) return;
            UnlockTutorial();
            CompleteTutorial = true;
        }

        private void Update()
        {
            // Only check achievements if the active scene is a gameplay scene (not menu)
            var activeScene = SceneManager.GetActiveScene();
            if (SceneInfo.IsMainMenuScene(activeScene))
            {
                return; // Skip achievement checks in menu scene
            }

            if (SceneInfo.IsLevelTwo(activeScene))
            {
                UnlockTutorial();
                CompleteTutorial = true;
            }

            CheckPowerUpAchievement();
            UpdateAchievementDisplay();
        }

        // ======================
        // CHECKS
        // ======================

        private void CheckPowerUpAchievement()
        {
            // Ensure achievement is only unlocked when conditions are met during gameplay
            if (confuseAndSpeedUnlocked ||
                ConfusePowerDown.confuseTurns <= 0 ||
                TeleportPowerUp.teleportTimes <= 0) return;
            confuseAndSpeedUnlocked = true; // Prevents re-unlocking
            UnlockConfuseAndSpeed();
        }

        // ======================
        // UNLOCK METHODS (call these from triggers/events)
        // ======================

        public void UnlockConfuseAndSpeed()
        {
            ConfuseAndSpeed = true;
            Debug.Log("[AchievementManager]: Achievement unlocked - 'Confuse and Speed' (used both power-ups simultaneously)");
            if (SaveFacade.Instance != null)
            {
                SaveFacade.Instance.Achievements.confuseAndSpeed = true;
                SaveFacade.Instance.SaveAchievements();
            }
        }

        public void UnlockTutorial()
        {
            CompleteTutorial = true;
            Debug.Log("[AchievementManager]: Achievement unlocked - 'Tutorial Complete' (finished all tutorial levels)");
            if (SaveFacade.Instance != null)
            {
                SaveFacade.Instance.Achievements.completeTutorial = true;
                SaveFacade.Instance.SaveAchievements();
            }
        }

        public void UnlockLevelTwo()
        {
            CompleteLevelTwo = true;
            Debug.Log("[AchievementManager]: Achievement unlocked - 'Level Two Complete' (completed second main level)");
            if (SaveFacade.Instance != null)
            {
                SaveFacade.Instance.Achievements.completeLevelTwo = true;
                SaveFacade.Instance.SaveAchievements();
            }
        }

        // ======================
        // HAT SYSTEM
        // ======================

        public void SetSelectedHat(string hatName)
        {
            selectedHatName = hatName;
            Debug.Log($"[AchievementManager]: Hat selection changed to '{hatName}'");
            if (SaveFacade.Instance != null)
            {
                SaveFacade.Instance.Achievements.selectedHatName = hatName ?? "";
                SaveFacade.Instance.SaveAll();
            }
        }

        // ======================
        // UI DISPLAY
        // ======================

        private void FindAchievementText()
        {
            achievementText = null;
            var achievementObj = GameObject.FindWithTag("achievement");
            if (achievementObj != null)
            {
                achievementText = achievementObj.GetComponent<TextMeshProUGUI>();
            }
        }

        private void UpdateAchievementDisplay(bool forceUpdate = false)
        {
            if (achievementText == null) return;

            // Check against the private fields
            if (!forceUpdate) return;
            var display = "Achievements:\n";
            if (ConfuseAndSpeed || (SaveFacade.Instance?.Achievements.confuseAndSpeed ?? false)) display += "- Confuse and Speed Combo\n";
            if (CompleteTutorial || (SaveFacade.Instance?.Achievements.completeTutorial ?? false)) display += "- Completed Tutorial\n";
            if (CompleteLevelTwo || (SaveFacade.Instance?.Achievements.completeLevelTwo ?? false)) display += "- Completed Level Two\n";
            achievementText.text = display;
            if (hideAchievementCoroutine != null) StopCoroutine(hideAchievementCoroutine);
            hideAchievementCoroutine = StartCoroutine(HideAchievementAfterDelay(displayDuration));
        }

        private IEnumerator HideAchievementAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (achievementText != null) achievementText.text = "";
            hideAchievementCoroutine = null;
        }
    }
}
