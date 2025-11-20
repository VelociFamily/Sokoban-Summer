using System.Collections.Generic;
using Core;
using UnityEngine;

namespace UI
{
    public class HatSelectionManager : MonoBehaviour
    {
        [Header("Hat Options (Menu Prefabs)")]
        public List<GameObject> hats; // menu versions of hats

        [Header("UI Arrows")]
        public GameObject leftArrow;
        public GameObject rightArrow;

        [Header("Character & Lock UI")]
        public GameObject character;   // menu display character
        public GameObject lockObject;  // lock icon if not unlocked

        private int currentIndex;
        private bool unlocked;
        private AchievementManager achievementManager; // cached when available
        private bool subscribed;

        void Start()
        {
            // Attempt to acquire AchievementManager; may not be registered yet at Start.
            if (ServiceLocator.TryGet<AchievementManager>(out achievementManager))
            {
                unlocked = achievementManager.CompleteTutorial;
                ShowHatsUI(unlocked);

                if (unlocked)
                {
                    Debug.Log("[HatSelectionManager]: Hat system unlocked - tutorial completed");
                    LoadSavedHat();
                    UpdateHatVisibility();
                }
            }
            else
            {
                unlocked = false;
                ShowHatsUI(false); // show lock until tutorial completion & service available
            }
        }

        void Update()
        {
            // Fallback acquisition if started before AchievementManager was registered.
            if (!subscribed && achievementManager == null && ServiceLocator.TryGet<AchievementManager>(out achievementManager))
            {
                SubscribeToAchievements();
            }
        }

        void OnEnable()
        {
            if (achievementManager == null && ServiceLocator.TryGet<AchievementManager>(out achievementManager))
            {
                SubscribeToAchievements();
            }
        }

        void OnDisable()
        {
            if (achievementManager != null && subscribed)
            {
                achievementManager.AchievementsChanged -= OnAchievementsChanged;
                subscribed = false;
            }
        }

        void UpdateHatVisibility()
        {
            if (hats == null || hats.Count == 0) return;
            if (currentIndex < 0 || currentIndex >= hats.Count) currentIndex = 0;

            for (var i = 0; i < hats.Count; i++)
                hats[i].SetActive(i == currentIndex);

            // Persist selection only if service exists, unlocked, and changed to avoid recursive event loop
            if (achievementManager != null && unlocked)
            {
                var hatName = hats[currentIndex].name;
                if (achievementManager.selectedHatName != hatName)
                {
                    achievementManager.SetSelectedHat(hatName);
                }
            }
        }

        public void NextHat()
        {
            if (!unlocked) return;
            currentIndex++;
            if (currentIndex >= hats.Count) currentIndex = 0;
            UpdateHatVisibility();
        }

        public void PreviousHat()
        {
            if (!unlocked) return;
            currentIndex--;
            if (currentIndex < 0) currentIndex = hats.Count - 1;
            UpdateHatVisibility();
        }

        void ShowHatsUI(bool show)
        {
            if (character != null) character.SetActive(show);
            if (leftArrow != null) leftArrow.SetActive(show);
            if (rightArrow != null) rightArrow.SetActive(show);
            if (lockObject != null) lockObject.SetActive(!show);
        }

        void LoadSavedHat()
        {
            if (achievementManager == null || hats == null || hats.Count == 0) return;
            if (string.IsNullOrEmpty(achievementManager.selectedHatName)) return;
            var index = hats.FindIndex(h => h != null && h.name == achievementManager.selectedHatName);
            if (index >= 0)
                currentIndex = index;
        }

        void SubscribeToAchievements()
        {
            if (achievementManager == null || subscribed) return;
            achievementManager.AchievementsChanged += OnAchievementsChanged;
            subscribed = true;
            // Initialize current state based on existing achievement data
            unlocked = achievementManager.CompleteTutorial;
            ShowHatsUI(unlocked);
            if (unlocked)
            {
                LoadSavedHat();
                UpdateHatVisibility();
            }
        }

        void OnAchievementsChanged()
        {
            if (achievementManager == null) return;
            var wasUnlocked = unlocked;
            unlocked = achievementManager.CompleteTutorial;
            if (unlocked != wasUnlocked)
            {
                ShowHatsUI(unlocked);
            }
            if (unlocked)
            {
                // Ensure hat selection reflects persisted choice
                LoadSavedHat();
                UpdateHatVisibility();
            }
        }
    }
}
