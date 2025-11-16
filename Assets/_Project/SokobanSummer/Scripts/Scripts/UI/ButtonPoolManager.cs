using System.Collections.Generic;
using UnityEngine;
using Core;

namespace UI
{
    /// <summary>
    /// Manages object pooling for UI buttons to avoid runtime Destroy/Instantiate calls.
    /// Single responsibility: Button lifecycle and pooling.
    /// </summary>
    public class ButtonPoolManager
    {
        private readonly Transform container;
        private readonly GameObject buttonPrefab;
        private readonly GameObject headerPrefab;
        private readonly Sprite lockedLevelSprite;

        private readonly List<GameObject> activeButtons = new List<GameObject>();
        private readonly List<GameObject> buttonPool = new List<GameObject>();
        private readonly List<GameObject> activeHeaders = new List<GameObject>();
        private readonly List<GameObject> headerPool = new List<GameObject>();

        private readonly List<DynamicLevelButton> activeLevelButtons = new List<DynamicLevelButton>();

        public IReadOnlyList<DynamicLevelButton> ActiveLevelButtons => activeLevelButtons;
        public IReadOnlyList<GameObject> ActiveButtons => activeButtons;

        public ButtonPoolManager(Transform container, GameObject buttonPrefab, GameObject headerPrefab, Sprite lockedLevelSprite)
        {
            this.container = container;
            this.buttonPrefab = buttonPrefab;
            this.headerPrefab = headerPrefab;
            this.lockedLevelSprite = lockedLevelSprite;
        }

        /// <summary>
        /// Get or create a level button from the pool
        /// </summary>
        public GameObject GetButton()
        {
            GameObject button;

            if (buttonPool.Count > 0)
            {
                // Reuse from pool
                button = buttonPool[buttonPool.Count - 1];
                buttonPool.RemoveAt(buttonPool.Count - 1);
                button.SetActive(true);
            }
            else
            {
                // Create new instance
                button = Object.Instantiate(buttonPrefab, container);
            }

            activeButtons.Add(button);
            return button;
        }

        /// <summary>
        /// Get or create a header from the pool
        /// </summary>
        public GameObject GetHeader()
        {
            if (headerPrefab == null)
                return null;

            GameObject header;

            if (headerPool.Count > 0)
            {
                // Reuse from pool
                header = headerPool[headerPool.Count - 1];
                headerPool.RemoveAt(headerPool.Count - 1);
                header.SetActive(true);
            }
            else
            {
                // Create new instance
                header = Object.Instantiate(headerPrefab, container);
            }

            activeHeaders.Add(header);
            return header;
        }

        /// <summary>
        /// Create and setup a level button
        /// </summary>
        public DynamicLevelButton CreateLevelButton(LevelManager.LevelInfo levelInfo)
        {
            var buttonObj = GetButton();
            
            var dynamicButton = buttonObj.GetComponent<DynamicLevelButton>();
            if (dynamicButton == null)
            {
                Debug.LogError("[ButtonPoolManager] Level button prefab must have DynamicLevelButton component.");
                return null;
            }

            if (lockedLevelSprite != null)
            {
                dynamicButton.SetLockSprite(lockedLevelSprite);
            }

            dynamicButton.SetupLevel(levelInfo);
            activeLevelButtons.Add(dynamicButton);

            return dynamicButton;
        }

        /// <summary>
        /// Return all active buttons to the pool without destroying them
        /// </summary>
        public void ReturnAllToPool()
        {
            // Return buttons to pool
            foreach (var button in activeButtons)
            {
                if (button != null)
                {
                    button.SetActive(false);
                    buttonPool.Add(button);
                }
            }
            activeButtons.Clear();
            activeLevelButtons.Clear();

            // Return headers to pool
            foreach (var header in activeHeaders)
            {
                if (header != null)
                {
                    header.SetActive(false);
                    headerPool.Add(header);
                }
            }
            activeHeaders.Clear();
        }

        /// <summary>
        /// Destroy all pooled and active objects (for cleanup)
        /// </summary>
        public void DestroyAll()
        {
            foreach (var button in activeButtons)
            {
                if (button != null)
                    Object.Destroy(button);
            }
            activeButtons.Clear();
            activeLevelButtons.Clear();

            foreach (var button in buttonPool)
            {
                if (button != null)
                    Object.Destroy(button);
            }
            buttonPool.Clear();

            foreach (var header in activeHeaders)
            {
                if (header != null)
                    Object.Destroy(header);
            }
            activeHeaders.Clear();

            foreach (var header in headerPool)
            {
                if (header != null)
                    Object.Destroy(header);
            }
            headerPool.Clear();
        }

        /// <summary>
        /// Get first interactable button from active buttons
        /// </summary>
        public DynamicLevelButton FindFirstInteractableButton()
        {
            foreach (var button in activeLevelButtons)
            {
                if (button == null || button.button == null)
                    continue;

                if (button.button.interactable)
                    return button;
            }

            return null;
        }

        /// <summary>
        /// Refresh states of all active buttons
        /// </summary>
        public void RefreshButtonStates()
        {
            foreach (var button in activeLevelButtons)
            {
                if (button != null)
                {
                    button.UpdateLockState();
                }
            }
        }
    }
}
