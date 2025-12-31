using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core;

namespace UI
{
    /// <summary>
    /// Bridges MoveCounter and AchievementManager into the persistent HUD.
    /// Shows moves/time only in gameplay scenes and surfaces short achievement toasts.
    /// </summary>
    public class PersistentMoveTimerUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI achievementToastText;
        [SerializeField] private CanvasGroup hudCanvasGroup;
        [SerializeField] private CanvasGroup achievementToastGroup;

        [Header("Settings")]
        [SerializeField] private bool hideOutsideGameplay = true;
        [SerializeField] private float achievementToastSeconds = 3f;
        [SerializeField] private string movesLabelFormat = "Moves: {0}";
        [SerializeField] private string timerLabelFormat = "{0:00}:{1:00}";
        [SerializeField] private string achievementToastPrefix = "Achievement Unlocked: ";

        private MoveCounter moveCounter;
        private AchievementManager achievementManager;
        private Coroutine toastRoutine;
        private bool cachedConfuseAndSpeed;
        private bool cachedTutorial;
        private bool cachedLevelTwo;
        private bool isGameplayScene;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            TryBindServices();
            UpdateVisibility(SceneManager.GetActiveScene());
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            UnsubscribeMoveCounter();
            UnsubscribeAchievementManager();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryBindServices();
            UpdateVisibility(scene);
        }

        private void OnActiveSceneChanged(Scene from, Scene to)
        {
            UpdateVisibility(to);
        }

        private void TryBindServices()
        {
            TryBindMoveCounter();
            TryBindAchievementManager();
        }

        private void TryBindMoveCounter()
        {
            if (moveCounter != null) return;

            // Prefer local component on the same GameObject (common in PersistentUI setup)
            var local = GetComponent<MoveCounter>();
            if (local != null)
            {
                moveCounter = local;
            }

            if (!ServiceLocator.TryGet<MoveCounter>(out var mc))
            {
                mc = FindFirstObjectByType<MoveCounter>(FindObjectsInactive.Include);
            }

            if (moveCounter == null && mc != null)
            {
                moveCounter = mc;
            }

            if (moveCounter == null) return;

            moveCounter.OnMovesChanged += HandleMovesChanged;
            moveCounter.OnTimerChanged += HandleTimerChanged;
            moveCounter.OnLevelCompleted += HandleLevelCompleted;

            HandleMovesChanged(this, new MoveCountChangedEventArgs(moveCounter.moveCount));
            HandleTimerChanged(this, new TimerChangedEventArgs(moveCounter.GetElapsedTime()));

            // Update visibility now that we have a counter
            UpdateVisibility(SceneManager.GetActiveScene());
        }

        private void UnsubscribeMoveCounter()
        {
            if (moveCounter == null) return;
            moveCounter.OnMovesChanged -= HandleMovesChanged;
            moveCounter.OnTimerChanged -= HandleTimerChanged;
            moveCounter.OnLevelCompleted -= HandleLevelCompleted;
            moveCounter = null;
        }

        private void TryBindAchievementManager()
        {
            if (achievementManager != null) return;
            if (!ServiceLocator.TryGet<AchievementManager>(out var manager))
            {
                manager = FindFirstObjectByType<AchievementManager>(FindObjectsInactive.Include);
            }

            if (manager == null) return;

            achievementManager = manager;
            cachedConfuseAndSpeed = achievementManager.ConfuseAndSpeed;
            cachedTutorial = achievementManager.CompleteTutorial;
            cachedLevelTwo = achievementManager.CompleteLevelTwo;

            achievementManager.AchievementsChanged += HandleAchievementsChanged;
        }

        private void UnsubscribeAchievementManager()
        {
            if (achievementManager == null) return;
            achievementManager.AchievementsChanged -= HandleAchievementsChanged;
            achievementManager = null;
        }

        private void HandleMovesChanged(object sender, MoveCountChangedEventArgs args)
        {
            if (movesText == null) return;
            movesText.text = string.Format(movesLabelFormat, args.MoveCount);
        }

        private void HandleTimerChanged(object sender, TimerChangedEventArgs args)
        {
            if (timerText == null) return;
            var time = TimeSpan.FromSeconds(args.ElapsedTime);
            var minutes = (int)time.TotalMinutes;
            timerText.text = string.Format(timerLabelFormat, minutes, time.Seconds);
        }

        private void HandleLevelCompleted(object sender, EventArgs e)
        {
            if (hideOutsideGameplay && !isGameplayScene)
            {
                SetCanvasGroupVisibility(hudCanvasGroup, false);
            }
        }

        private void HandleAchievementsChanged()
        {
            if (achievementManager == null) return;
            CheckAchievement(ref cachedConfuseAndSpeed, achievementManager.ConfuseAndSpeed, "Confuse and Speed");
            CheckAchievement(ref cachedTutorial, achievementManager.CompleteTutorial, "Tutorial Complete");
            CheckAchievement(ref cachedLevelTwo, achievementManager.CompleteLevelTwo, "Level Two Complete");
        }

        private void CheckAchievement(ref bool cachedState, bool currentState, string friendlyName)
        {
            if (!cachedState && currentState)
            {
                ShowAchievementToast(friendlyName);
            }
            cachedState = currentState;
        }

        private void ShowAchievementToast(string achievementName)
        {
            if (achievementToastText == null) return;

            if (toastRoutine != null)
            {
                StopCoroutine(toastRoutine);
            }

            achievementToastText.text = $"{achievementToastPrefix}{achievementName}";
            if (achievementToastGroup != null)
            {
                SetCanvasGroupVisibility(achievementToastGroup, true);
            }

            toastRoutine = StartCoroutine(HideAchievementAfterDelay());
        }

        private IEnumerator HideAchievementAfterDelay()
        {
            yield return new WaitForSeconds(achievementToastSeconds);
            if (achievementToastText != null)
            {
                achievementToastText.text = string.Empty;
            }
            if (achievementToastGroup != null)
            {
                SetCanvasGroupVisibility(achievementToastGroup, false);
            }
            toastRoutine = null;
        }

        private void UpdateVisibility(Scene scene)
        {
            isGameplayScene = SceneInfo.IsGameplayScene(scene) || AnyGameplaySceneLoaded();

            if (hideOutsideGameplay && hudCanvasGroup != null)
            {
                var visible = isGameplayScene && moveCounter != null;
                SetCanvasGroupVisibility(hudCanvasGroup, visible);
            }
            else if (!isGameplayScene)
            {
                ClearTexts();
            }
        }

        private void ClearTexts()
        {
            if (movesText != null) movesText.text = string.Empty;
            if (timerText != null) timerText.text = string.Empty;
        }

        private bool AnyGameplaySceneLoaded()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (!s.isLoaded) continue;
                if (SceneInfo.IsGameplayScene(s))
                {
                    return true;
                }
            }
            return false;
        }

        private void SetCanvasGroupVisibility(CanvasGroup group, bool visible)
        {
            if (group == null) return;
            group.alpha = visible ? 1f : 0f;
            group.blocksRaycasts = visible;
            group.interactable = visible;
        }
    }
}
