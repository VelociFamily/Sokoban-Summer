using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using Core;

namespace UI
{
    /// <summary>
    /// Simple level button component for the Dynamic Level Management System
    /// Replaces the complex SceneButton for cleaner level loading
    /// </summary>
    public class DynamicLevelButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Components")]
        [Tooltip("Main button component")]
        public Button button;

    [Tooltip("TextMeshProUGUI component for level name")]
    public TextMeshProUGUI levelNameText;

    [Tooltip("TextMeshProUGUI component for goals/par info")]
    public TextMeshProUGUI goalText;

        [Tooltip("Image component for level preview")]
        public Image previewImage;

        [Tooltip("Fallback sprite to show when no preview is available")]
        public Sprite fallbackThumbnail;

        [Tooltip("Lock overlay GameObject (shown when level is locked)")]
        public GameObject lockOverlay;

    [Tooltip("Image used to render the lock overlay sprite")]
    public Image lockOverlayImage;

        [Tooltip("GameObject with completion badge (star/check, shown when level completed)")]
        public GameObject completionBadge;

        [Header("Selection Visuals")]
        [Tooltip("Background graphic that tints on selection; falls back to Button target graphic")]
        public Image selectionTarget;
        [Tooltip("Color when not selected/hovered")]
        public Color normalColor = Color.white;
        [Tooltip("Color when selected or hovered")]
        public Color selectedColor = new Color(0.55f, 0.85f, 1f, 1f);
        [Tooltip("Fade time for selection tint")] public float selectionFade = 0.08f;
        [Tooltip("Move the selection graphic just behind text/content while keeping it above the base sprite")]
        public bool bringSelectionTargetToFront = false;

    [Header("Lock Icon Sizing")]
    [Tooltip("Ratio of lock icon size relative to button height (0-1).")]
    [Range(0.1f, 1f)] public float lockIconSizeRatio = 0.45f;

    [Header("Audio")]
    [Tooltip("Optional click sound to play when selecting a level")]
    public AudioClip clickSfx;

        [Header("Level Data")]
        public LevelManager.LevelInfo levelInfo;

        // Prevent double-activation while scenes are loading
        private bool _clicked = false;
        private Sprite _assignedLockSprite;
        private bool _isSelected;
        private bool _isHovered;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();

            EnsureSelectionTarget();
            ConfigureButtonTransitionForCustomTint();
            BringSelectionTargetToFront();

            if (button != null)
                button.onClick.AddListener(LoadLevel);
        }

        private void OnEnable()
        {
            if (selectionTarget != null)
            {
                selectionTarget.color = normalColor;
            }
            ApplySelectionVisual(false);
        }

        /// <summary>
        /// Configure this button with level information
        /// </summary>
        public void SetupLevel(LevelManager.LevelInfo info)
        {
            levelInfo = info;

            // Update UI elements
            if (levelNameText != null)
                levelNameText.text = info.displayName;

            if (goalText != null)
                SetupGoalText(info);

            if (previewImage != null)
            {
                previewImage.sprite = info.previewImage != null ? info.previewImage : fallbackThumbnail;
            }

            EnsureLockOverlayImage();
            ApplyAssignedLockSprite();
            ApplySelectionVisual(false);

            // Update lock state and completion badge
            UpdateLockState();
            UpdateCompletionBadge();
        }

        /// <summary>
        /// Setup the goal text with par moves and time
        /// </summary>
        private void SetupGoalText(LevelManager.LevelInfo info)
        {
            var goals = new System.Collections.Generic.List<string>();

            if (info.parMoves > 0)
                goals.Add($"Par: {info.parMoves} moves");
            if (info.parTime > 0f)
                goals.Add($"Time: {FormatTime(info.parTime)}");

            goalText.text = goals.Count > 0 ? string.Join(" | ", goals) : "";
        }

        /// <summary>
        /// Update button lock state and interactability
        /// </summary>
        public void UpdateLockState()
        {
            if (levelInfo == null) return;

            var levelManager = ServiceLocator.Get<LevelManager>();
            bool canLoad = levelManager.CanLoadLevel(levelInfo);

            // Update button interactability
            if (button != null)
                button.interactable = canLoad;

            // Update lock overlay
            if (lockOverlay != null)
                lockOverlay.SetActive(!canLoad);

            if (lockOverlayImage != null)
            {
                lockOverlayImage.enabled = !canLoad;
                if (!canLoad)
                {
                    AdjustLockIconSize();
                }
            }
        }

        /// <summary>
        /// Update completion badge visibility based on level completion status
        /// </summary>
        public void UpdateCompletionBadge()
        {
            if (completionBadge == null || levelInfo == null) return;

            var levelManager = ServiceLocator.Get<LevelManager>();
            bool isCompleted = levelManager.IsLevelCompleted(levelInfo);
            completionBadge.SetActive(isCompleted);
        }

        /// <summary>
        /// Assign a sprite to the lock overlay image.
        /// </summary>
        public void SetLockSprite(Sprite sprite)
        {
            _assignedLockSprite = sprite;
            EnsureLockOverlayImage();
            ApplyAssignedLockSprite();
        }

        private void EnsureLockOverlayImage()
        {
            if (lockOverlayImage != null) return;

            if (lockOverlay != null)
            {
                lockOverlayImage = lockOverlay.GetComponent<Image>() ?? lockOverlay.GetComponentInChildren<Image>();
            }
        }

        private void ApplyAssignedLockSprite()
        {
            if (lockOverlayImage != null && _assignedLockSprite != null)
            {
                lockOverlayImage.sprite = _assignedLockSprite;
                lockOverlayImage.preserveAspect = true;
            }
        }

        private void AdjustLockIconSize()
        {
            if (lockOverlayImage == null) return;
            var rootRect = GetComponent<RectTransform>();
            if (rootRect == null) return;
            float target = Mathf.Clamp(lockIconSizeRatio, 0.1f, 1f) * rootRect.rect.height;
            var rt = lockOverlayImage.rectTransform;
            rt.sizeDelta = new Vector2(target, target);
        }

        /// <summary>
        /// Load the associated level
        /// </summary>
        public void LoadLevel()
        {
            if (levelInfo == null)
            {
                Debug.LogError("[DynamicLevelButton] No level info assigned!");
                return;
            }

            var levelManager = ServiceLocator.Get<LevelManager>();
            if (!levelManager.CanLoadLevel(levelInfo))
            {
                Debug.Log($"[DynamicLevelButton] Level {levelInfo.displayName} is locked");
                return;
            }

            // Prevent double clicks
            if (_clicked) return;
            _clicked = true;

            Debug.Log($"[DynamicLevelButton] Loading level: {levelInfo.displayName}");

            if (clickSfx)
            {
                var audioService = ServiceLocator.Get<ModernAudioService>();
                audioService?.PlaySFX(clickSfx);
            }

            // Delegate to LevelManager so loading respects additive/persistence rules.
            levelManager?.LoadLevel(levelInfo);
            
            // Reset the clicked flag after a delay to allow re-clicking if level load fails or is cancelled
            StartCoroutine(ResetClickedFlagAfterDelay(2f));
        }

        private System.Collections.IEnumerator ResetClickedFlagAfterDelay(float delaySeconds)
        {
            yield return new WaitForSecondsRealtime(delaySeconds);
            _clicked = false;
            Debug.Log("[DynamicLevelButton] Click flag reset - button ready for next click");
        }

        public void OnSelect(BaseEventData eventData)
        {
            _isSelected = true;
            ApplySelectionVisual(true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _isSelected = false;
            ApplySelectionVisual(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            ApplySelectionVisual(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            ApplySelectionVisual(_isSelected);
        }

        private void ApplySelectionVisual(bool highlight)
        {
            EnsureSelectionTarget();
            var target = selectionTarget != null ? selectionTarget : button != null ? button.targetGraphic as Image : GetComponent<Image>();
            if (target == null)
                return;

            var desired = highlight || _isHovered || _isSelected ? selectedColor : normalColor;
            target.CrossFadeColor(desired, selectionFade, true, true);
        }

        private void EnsureSelectionTarget()
        {
            if (selectionTarget != null)
                return;

            if (button != null && button.targetGraphic is Image image)
            {
                selectionTarget = image;
            }
            else
            {
                selectionTarget = GetComponent<Image>();
            }
        }

        private void BringSelectionTargetToFront()
        {
            if (!bringSelectionTargetToFront || selectionTarget == null)
                return;

            var rt = selectionTarget.transform as RectTransform;
            if (rt != null)
            {
                // Keep the tint behind text/content so labels stay visible.
                rt.SetAsFirstSibling();
            }
        }

        private void ConfigureButtonTransitionForCustomTint()
        {
            if (button == null)
                return;

            // Avoid Unity's built-in color tint overriding our manual tint; keep other states neutral.
            button.transition = Selectable.Transition.None;
        }

        /// <summary>
        /// Format time in MM:SS format
        /// </summary>
        private string FormatTime(float time)
        {
            var minutes = Mathf.FloorToInt(time / 60f);
            var seconds = Mathf.FloorToInt(time % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
    }
}
