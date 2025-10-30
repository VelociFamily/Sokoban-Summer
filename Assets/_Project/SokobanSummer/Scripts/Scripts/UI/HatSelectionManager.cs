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

        void Start()
        {
            // unlocked only if tutorial is complete
            unlocked = (AchievementManager.Instance != null && AchievementManager.Instance.CompleteTutorial);
            ShowHatsUI(unlocked);

            if (unlocked)
            {
                Debug.Log("[HatSelectionManager]: Hat system unlocked - tutorial completed");
                UpdateHatVisibility();

                // Load saved hat if exists
                if (!string.IsNullOrEmpty(AchievementManager.Instance.selectedHatName))
                {
                    var index = hats.FindIndex(h => h.name == AchievementManager.Instance.selectedHatName);
                    if (index >= 0)
                    {
                        currentIndex = index;
                        UpdateHatVisibility();
                    }
                }
            }
        }

        void Update()
        {
            if (!unlocked && AchievementManager.Instance != null && AchievementManager.Instance.CompleteTutorial)
            {
                unlocked = true;
                Debug.Log("[HatSelectionManager]: Hat system newly unlocked during gameplay");
                ShowHatsUI(true);
                UpdateHatVisibility();
            }
        }

        void UpdateHatVisibility()
        {
            for (var i = 0; i < hats.Count; i++)
            {
                hats[i].SetActive(i == currentIndex);
            }

            // save selected hat name to AchievementManager
            if (AchievementManager.Instance != null && hats.Count > 0)
            {
                AchievementManager.Instance.SetSelectedHat(hats[currentIndex].name);
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
            character.SetActive(show);
            leftArrow.SetActive(show);
            rightArrow.SetActive(show);
            lockObject.SetActive(!show);
        }
    }
}
