using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI
{
    /// <summary>
    /// Dynamically populates level selection UI based on available levels
    /// Replaces hardcoded level buttons with dynamic generation
    /// </summary>
    public class DynamicLevelSelector : MonoBehaviour
    {
        // Use deferred destruction to avoid exceptions during OnValidate, render, physics, or animation callbacks.
        private static void SafeDestroy(UnityEngine.Object obj)
        {
            if (obj == null) return;
            // Unity warns: "Destroying components immediately is not permitted during ... or OnValidate. You must use Destroy instead."
            // Always prefer deferred destroy; it's safe in both play mode and edit-time contexts.
            UnityEngine.Object.Destroy(obj);
        }
        public enum LayoutMode
        {
            VerticalList,
            Grid
        }
        [Header("UI References")]
        [Tooltip("Parent container where level buttons will be instantiated")]
        public Transform levelButtonContainer;

        [Tooltip("Prefab for level buttons (should have DynamicLevelButton)")]
        public GameObject levelButtonPrefab;

        [Tooltip("Prefab for section headers (optional)")]
        public GameObject sectionHeaderPrefab;

        [Header("Configuration")]
        [Tooltip("Show tutorial levels")]
        public bool showTutorials = true;

        [Tooltip("Show gameplay levels")]
        public bool showGameplayLevels = true;

        [Tooltip("Add section headers between different level types")]
        public bool addSectionHeaders = true;

        [Header("Layout")]
        [Tooltip("Spacing between level buttons")]
        public float buttonSpacing = 24f;

    [Tooltip("Auto-add a VerticalLayoutGroup to the container if none is present")]
    public bool autoAddVerticalLayoutGroup = true;

    [Tooltip("Preferred height for each level button when using layout groups")]
    public float buttonHeight = 212f;

    [Tooltip("Preferred height for section headers when using layout groups")]
    public float headerHeight = 80f;

    [Tooltip("Stretch buttons to container width when using layout groups")]
    public bool stretchButtonsToContainerWidth = true;

        [Tooltip("Choose how to lay out the items")]
        public LayoutMode layoutMode = LayoutMode.VerticalList;

        [Header("Grid Settings")]
    [Tooltip("Number of columns for Grid layout")]
    public int gridColumns = 5;

        [Tooltip("Cell height for Grid layout")]
    public float gridCellHeight = 180f;

        [Tooltip("Horizontal spacing between cells in Grid layout")]
        public float gridHorizontalSpacing = 24f;

        [Tooltip("Vertical spacing between cells in Grid layout")]
        public float gridVerticalSpacing = 24f;

        [Tooltip("Grid padding: Left, Right, Top, Bottom")]
        public int gridPaddingLeft = 24, gridPaddingRight = 24, gridPaddingTop = 24, gridPaddingBottom = 24;

        [Tooltip("Compute cell width to evenly fill the container for Grid layout")]
        public bool responsiveGridCellWidth = true;

    [Tooltip("Enable simple responsive rules to choose columns based on container width")]
    public bool enableResponsiveColumns = false;

    [Tooltip("Max container width for 1 column (if responsive enabled)")]
    public float oneColumnMaxWidth = 680f;

    [Tooltip("Max container width for 2 columns (if responsive enabled); above uses 3+")]
    public float twoColumnMaxWidth = 1080f;

        [Header("Lock Visuals")]
        [Tooltip("Sprite applied to the lock overlay Image on each level button")]
        public Sprite lockedLevelSprite;

        [Header("Pagination")]
        [Tooltip("Enable pagination when there are more buttons than fit in the grid at once")]
        public bool enablePagination = true;

        [Tooltip("Rows per page when laying out buttons as a grid (combined with columns)")]
        public int gridRowsPerPage = 3;

        [Tooltip("Optional button to move to the previous page of levels")]
        public Button previousPageButton;

        [Tooltip("Optional button to move to the next page of levels")]
        public Button nextPageButton;

    private List<GameObject> generatedButtons = new List<GameObject>();
    private readonly List<DynamicLevelButton> generatedLevelButtons = new List<DynamicLevelButton>();
    private readonly Dictionary<LevelManager.LevelInfo, DynamicLevelButton> levelInfoToButton = new Dictionary<LevelManager.LevelInfo, DynamicLevelButton>();
    private readonly List<LevelManager.LevelInfo> orderedLevelSequence = new List<LevelManager.LevelInfo>();
    private readonly List<LevelManager.LevelInfo> cachedDisplayLevels = new List<LevelManager.LevelInfo>();
    private int currentPage;
    private bool preferLastUnlockedSelection = true;
        // Effective columns after responsive calculation. Falls back to configured gridColumns until first grid pass.
        private int effectiveColumns = 0;
        private bool needsGridPaginationRefresh = false; // set when resize changes columns/page size
        private bool isRefreshingForResize = false; // guard to prevent recursive refresh loops

        private int LevelsPerPage => Mathf.Max(1, Mathf.Max(1, (effectiveColumns > 0 ? effectiveColumns : gridColumns)) * Mathf.Max(1, gridRowsPerPage));

        private void Start()
        {
            AutoBindScrollRectAndContainer();
            EnsureOrConfigureLayoutGroup();
            HookPaginationButtons();
            PopulateLevelButtons();
        }

        private void OnValidate()
        {
            // Apply layout changes live in editor
            AutoBindScrollRectAndContainer();
            EnsureOrConfigureLayoutGroup();
            AssignDefaultPrefabsInEditor();
            HookPaginationButtons();
            UpdateLayout();
            UpdatePaginationControls();
        }

        /// <summary>
        /// Clear and regenerate all level buttons
        /// </summary>
        public void PopulateLevelButtons()
        {
            // Clear existing buttons
            ClearGeneratedButtons();

            if (LevelManager.Instance == null)
            {
                Debug.LogError("[DynamicLevelSelector] LevelManager instance not found!");
                return;
            }

            if (levelButtonContainer == null || levelButtonPrefab == null)
            {
                Debug.LogError("[DynamicLevelSelector] Required UI references not set!");
                return;
            }

            EnsureOrConfigureLayoutGroup();

            GatherDisplayLevels(orderedLevelSequence);
            int defaultSelectionIndex = ResolveDefaultSelectionIndex(orderedLevelSequence);
            bool useLastUnlocked = preferLastUnlockedSelection && defaultSelectionIndex >= 0;

            if (layoutMode == LayoutMode.Grid)
            {
                cachedDisplayLevels.Clear();
                cachedDisplayLevels.AddRange(orderedLevelSequence);

                if (!enablePagination)
                {
                    currentPage = 0;
                }
                else if (useLastUnlocked)
                {
                    currentPage = Mathf.Clamp(defaultSelectionIndex / LevelsPerPage, 0, Mathf.Max(0, GetTotalPages() - 1));
                }
                else
                {
                    currentPage = Mathf.Clamp(currentPage, 0, Mathf.Max(0, GetTotalPages() - 1));
                }

                RenderCurrentGridPage();
            }
            else
            {
                bool tutorialHeaderAdded = false;
                bool gameplayHeaderAdded = false;

                foreach (var levelInfo in orderedLevelSequence)
                {
                    if (addSectionHeaders)
                    {
                        if (levelInfo.sceneType == SceneType.TutorialLevel && !tutorialHeaderAdded)
                        {
                            CreateSectionHeader("Tutorials");
                            tutorialHeaderAdded = true;
                        }
                        else if (levelInfo.sceneType == SceneType.GameplayLevel && !gameplayHeaderAdded)
                        {
                            CreateSectionHeader("Levels");
                            gameplayHeaderAdded = true;
                        }
                    }

                    CreateLevelButton(levelInfo);
                }

                UpdateLayout();
            }

            UpdatePaginationControls();
            SelectDefaultLevelButton(defaultSelectionIndex, useLastUnlocked);
            preferLastUnlockedSelection = true;
        }

        /// <summary>
        /// Create a section header
        /// </summary>
        private void CreateSectionHeader(string title)
        {
            if (sectionHeaderPrefab == null) return;

            var headerObj = Instantiate(sectionHeaderPrefab, levelButtonContainer);
            var headerText = headerObj.GetComponentInChildren<Text>();
            if (headerText != null)
                headerText.text = title;

            ConfigureChildForLayout(headerObj, isHeader: true);
            generatedButtons.Add(headerObj);
        }

        /// <summary>
        /// Create a button for a specific level
        /// </summary>
        private void CreateLevelButton(LevelManager.LevelInfo levelInfo)
        {
            var buttonObj = Instantiate(levelButtonPrefab, levelButtonContainer);
            generatedButtons.Add(buttonObj);

            // Require the new DynamicLevelButton component
            var dynamicButton = buttonObj.GetComponent<DynamicLevelButton>();
            if (dynamicButton == null)
            {
                Debug.LogError("[DynamicLevelSelector] Level button prefab must have DynamicLevelButton component. See LEVEL_BUTTON_SETUP_GUIDE.md.");
                return;
            }

            if (lockedLevelSprite != null)
            {
                dynamicButton.SetLockSprite(lockedLevelSprite);
            }

            dynamicButton.SetupLevel(levelInfo);
            generatedLevelButtons.Add(dynamicButton);
            levelInfoToButton[levelInfo] = dynamicButton;

            ConfigureChildForLayout(buttonObj, isHeader: false);
        }

        /// <summary>
        /// Clear all generated buttons
        /// </summary>
        private void ClearGeneratedButtons()
        {
            foreach (var button in generatedButtons)
            {
                if (button != null)
                    SafeDestroy(button);
            }
            generatedButtons.Clear();
            generatedLevelButtons.Clear();
            levelInfoToButton.Clear();
        }

        /// <summary>
        /// Update the layout of the level buttons
        /// </summary>
        private void UpdateLayout()
        {
            // If using a layout group, force rebuild
            var layoutGroup = levelButtonContainer.GetComponent<LayoutGroup>();
            if (layoutGroup != null)
            {
                var rt = levelButtonContainer.GetComponent<RectTransform>();
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                // If grid, recalc cell width and content height
                var grid = levelButtonContainer.GetComponent<GridLayoutGroup>();
                if (grid != null)
                {
                    RecalculateGrid(grid);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                    // If responsive columns changed the page capacity, rebuild current page & selection.
                    if (needsGridPaginationRefresh && !isRefreshingForResize)
                    {
                        isRefreshingForResize = true;
                        needsGridPaginationRefresh = false;
                        ClearGeneratedButtons();
                        // Re-render page with updated LevelsPerPage
                        if (layoutMode == LayoutMode.Grid)
                        {
                            RenderCurrentGridPage();
                            UpdatePaginationControls();
                            int defaultSelectionIndex = ResolveDefaultSelectionIndex(orderedLevelSequence);
                            SelectDefaultLevelButton(defaultSelectionIndex, preferLastUnlockedSelection);
                        }
                        isRefreshingForResize = false;
                        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                    }
                }
                return;
            }

            // Fallback: Manual vertical positioning if no layout group
            float currentY = 0f;
            foreach (var go in generatedButtons)
            {
                if (go == null) continue;
                var rt = go.GetComponent<RectTransform>();
                if (rt == null) continue;

                // Ensure anchored to top-center for predictable layout
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);

                // Determine element height
                float h = buttonHeight;
                var le = go.GetComponent<LayoutElement>();
                if (le != null && le.preferredHeight > 0)
                    h = le.preferredHeight;

                // Position element
                rt.anchoredPosition = new Vector2(0f, -currentY);

                // Stretch width if requested
                if (stretchButtonsToContainerWidth)
                {
                    rt.sizeDelta = new Vector2(0f, h);
                    rt.offsetMin = new Vector2(rt.offsetMin.x, rt.offsetMin.y); // no-op, keep
                }
                else
                {
                    var size = rt.sizeDelta;
                    size.y = h;
                    rt.sizeDelta = size;
                }

                currentY += h + buttonSpacing;
            }
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

        /// <summary>
        /// Refresh the level selection (useful when progression changes)
        /// </summary>
        public void RefreshLevelSelection()
        {
            if (layoutMode == LayoutMode.Grid)
            {
                currentPage = Mathf.Clamp(currentPage, 0, GetTotalPages() - 1);
            }
            preferLastUnlockedSelection = true;
            PopulateLevelButtons();
        }

        /// <summary>
        /// Called when a level is completed to update UI
        /// </summary>
        public void OnLevelCompleted()
        {
            // Refresh lock states without fully regenerating
            if (generatedButtons.Count > 0)
            {
                RefreshButtonStates();
            }
            UpdatePaginationControls();

            var defaultIndex = ResolveDefaultSelectionIndex(orderedLevelSequence);
            SelectDefaultLevelButton(defaultIndex, true);
        }

        /// <summary>
        /// Refresh just the button states without regenerating
        /// </summary>
        private void RefreshButtonStates()
        {
            foreach (var buttonObj in generatedButtons)
            {
                var dynamicButton = buttonObj.GetComponent<DynamicLevelButton>();
                if (dynamicButton != null)
                {
                    dynamicButton.UpdateLockState();
                }
            }
        }

        private void GatherDisplayLevels(List<LevelManager.LevelInfo> targetList)
        {
            targetList.Clear();

            if (LevelManager.Instance == null)
                return;

            if (showTutorials)
            {
                var tutorials = LevelManager.Instance.GetLevels(SceneType.TutorialLevel);
                if (tutorials != null && tutorials.Count > 0)
                {
                    targetList.AddRange(tutorials);
                }
            }

            if (showGameplayLevels)
            {
                var levels = LevelManager.Instance.GetLevels(SceneType.GameplayLevel);
                if (levels != null && levels.Count > 0)
                {
                    targetList.AddRange(levels);
                }
            }
        }

        private int ResolveDefaultSelectionIndex(List<LevelManager.LevelInfo> orderedLevels)
        {
            if (orderedLevels == null || orderedLevels.Count == 0 || LevelManager.Instance == null)
                return -1;

            int lastUnlockedIndex = -1;
            for (int i = 0; i < orderedLevels.Count; i++)
            {
                var info = orderedLevels[i];
                if (info != null && LevelManager.Instance.CanLoadLevel(info))
                {
                    lastUnlockedIndex = i;
                }
            }

            if (lastUnlockedIndex >= 0)
                return lastUnlockedIndex;

            return orderedLevels.Count > 0 ? 0 : -1;
        }

        private void SelectDefaultLevelButton(int defaultSelectionIndex, bool useLastUnlocked)
        {
            DynamicLevelButton targetButton = null;

            if (useLastUnlocked && defaultSelectionIndex >= 0 && defaultSelectionIndex < orderedLevelSequence.Count)
            {
                var targetLevel = orderedLevelSequence[defaultSelectionIndex];
                if (targetLevel != null && levelInfoToButton.TryGetValue(targetLevel, out var mappedButton))
                {
                    if (mappedButton != null && mappedButton.button != null && mappedButton.button.interactable)
                    {
                        targetButton = mappedButton;
                    }
                }
            }

            if (targetButton == null)
            {
                targetButton = FindFirstInteractableButtonOnPage();
            }

            if (targetButton == null && generatedLevelButtons.Count > 0)
            {
                targetButton = generatedLevelButtons[0];
            }

            if (targetButton != null)
            {
                var buttonComponent = targetButton.button;
                if (buttonComponent != null)
                {
                    buttonComponent.Select();
                    if (EventSystem.current != null)
                    {
                        EventSystem.current.SetSelectedGameObject(buttonComponent.gameObject);
                    }
                }
                else if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(targetButton.gameObject);
                }
            }
        }

        private DynamicLevelButton FindFirstInteractableButtonOnPage()
        {
            foreach (var button in generatedLevelButtons)
            {
                if (button == null || button.button == null)
                    continue;

                if (button.button.interactable)
                    return button;
            }

            return null;
        }

        private void RenderCurrentGridPage()
        {
            if (cachedDisplayLevels.Count == 0)
            {
                UpdateLayout();
                return;
            }

            int startIndex = enablePagination ? currentPage * LevelsPerPage : 0;
            int endIndex = enablePagination ? Mathf.Min(cachedDisplayLevels.Count, startIndex + LevelsPerPage) : cachedDisplayLevels.Count;

            startIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, cachedDisplayLevels.Count - 1));
            for (int i = startIndex; i < endIndex; i++)
            {
                CreateLevelButton(cachedDisplayLevels[i]);
            }

            UpdateLayout();
        }

        private void HookPaginationButtons()
        {
            if (previousPageButton != null)
            {
                previousPageButton.onClick.RemoveListener(GoToPreviousPage);
                previousPageButton.onClick.AddListener(GoToPreviousPage);
            }

            if (nextPageButton != null)
            {
                nextPageButton.onClick.RemoveListener(GoToNextPage);
                nextPageButton.onClick.AddListener(GoToNextPage);
            }
        }

        private void GoToPreviousPage()
        {
            if (currentPage <= 0) return;
            currentPage--;
            preferLastUnlockedSelection = false;
            PopulateLevelButtons();
        }

        private void GoToNextPage()
        {
            var totalPages = GetTotalPages();
            if (currentPage >= totalPages - 1) return;
            currentPage++;
            preferLastUnlockedSelection = false;
            PopulateLevelButtons();
        }

        private int GetTotalPages()
        {
            if (!enablePagination || layoutMode != LayoutMode.Grid)
                return Mathf.Max(1, cachedDisplayLevels.Count > 0 ? 1 : 0);

            int perPage = LevelsPerPage;
            if (perPage <= 0) return 1;
            int count = Mathf.Max(0, cachedDisplayLevels.Count);
            return Mathf.Max(1, Mathf.CeilToInt(count / (float)perPage));
        }

        private void UpdatePaginationControls()
        {
            // Always clamp current page to valid bounds before updating controls
            if (layoutMode == LayoutMode.Grid)
            {
                currentPage = Mathf.Clamp(currentPage, 0, Mathf.Max(0, GetTotalPages() - 1));
            }
            bool shouldShow = enablePagination && layoutMode == LayoutMode.Grid && cachedDisplayLevels.Count > LevelsPerPage;
            int totalPages = Mathf.Max(1, GetTotalPages());

            if (previousPageButton != null)
            {
                previousPageButton.gameObject.SetActive(shouldShow);
                previousPageButton.interactable = shouldShow && currentPage > 0;
            }

            if (nextPageButton != null)
            {
                nextPageButton.gameObject.SetActive(shouldShow);
                nextPageButton.interactable = shouldShow && currentPage < totalPages - 1;
            }
        }

        // Development helper
        [ContextMenu("Refresh Level Selection")]
        private void RefreshLevelSelectionContext()
        {
            RefreshLevelSelection();
        }

        /// <summary>
        /// Ensure a layout group exists on the container for automatic spacing.
        /// Adds VerticalLayoutGroup + ContentSizeFitter when enabled and missing.
        /// </summary>
        private void EnsureLayoutGroup()
        {
            if (levelButtonContainer == null) return;
            var existing = levelButtonContainer.GetComponent<LayoutGroup>();
            if (existing != null || !autoAddVerticalLayoutGroup) return;

            var v = levelButtonContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            v.childAlignment = TextAnchor.UpperCenter;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            v.spacing = buttonSpacing;

            var fitter = levelButtonContainer.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = levelButtonContainer.gameObject.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        /// <summary>
        /// Configure a child (button/header) for layout groups: stretch width and set preferred height.
        /// </summary>
        private void ConfigureChildForLayout(GameObject go, bool isHeader)
        {
            if (go == null) return;
            var rt = go.GetComponent<RectTransform>();
            if (rt != null && stretchButtonsToContainerWidth && layoutMode == LayoutMode.VerticalList)
            {
                // Stretch horizontally within container
                rt.anchorMin = new Vector2(0f, rt.anchorMin.y);
                rt.anchorMax = new Vector2(1f, rt.anchorMax.y);
                rt.offsetMin = new Vector2(0f, rt.offsetMin.y);
                rt.offsetMax = new Vector2(0f, rt.offsetMax.y);
            }

            if (layoutMode == LayoutMode.VerticalList)
            {
                var le = go.GetComponent<LayoutElement>();
                if (le == null) le = go.AddComponent<LayoutElement>();
                le.preferredHeight = isHeader ? headerHeight : buttonHeight;
                le.flexibleHeight = 0f;
                le.flexibleWidth = 0f;
            }
        }

        /// <summary>
        /// Ensure or configure the container to use the selected layout mode.
        /// </summary>
        private void EnsureOrConfigureLayoutGroup()
        {
            // During batch editor automation (validation / autofix) OnValidate and other editor
            // callbacks can run in a context where adding/removing components causes
            // DestroyImmediate / AddComponent to throw or trigger unexpected editor state.
            // Skip layout modifications when running in batch mode to keep automation stable.
#if UNITY_EDITOR
            // EditorApplication.isBatchMode is not available on all Editor versions; detect
            // batch mode by checking command line args instead.
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-batchmode") >= 0)
            {
                return;
            }
#endif

            if (levelButtonContainer == null) return;

            var existingVertical = levelButtonContainer.GetComponent<VerticalLayoutGroup>();
            var existingGrid = levelButtonContainer.GetComponent<GridLayoutGroup>();
            var fitter = levelButtonContainer.GetComponent<ContentSizeFitter>();

            if (layoutMode == LayoutMode.VerticalList)
            {
                // Switch to vertical
                if (existingGrid != null)
                {
                    if (autoAddVerticalLayoutGroup)
                        SafeDestroy(existingGrid);
                }

                var v = existingVertical;
                if (v == null && autoAddVerticalLayoutGroup)
                {
                    v = levelButtonContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                }
                if (v != null)
                {
                    v.childAlignment = TextAnchor.UpperCenter;
                    v.childControlWidth = true;
                    v.childControlHeight = true;
                    v.childForceExpandWidth = true;
                    v.childForceExpandHeight = false;
                    v.spacing = buttonSpacing;
                }

                if (fitter == null && autoAddVerticalLayoutGroup)
                {
                    fitter = levelButtonContainer.gameObject.AddComponent<ContentSizeFitter>();
                }
                if (fitter != null)
                {
                    fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                }
            }
            else // Grid
            {
                // Remove vertical if present
                if (existingVertical != null)
                {
                    SafeDestroy(existingVertical);
                }

                // Grid
                var grid = existingGrid;
                if (grid == null)
                {
                    grid = levelButtonContainer.gameObject.AddComponent<GridLayoutGroup>();
                }
                grid.childAlignment = TextAnchor.UpperCenter;
                grid.spacing = new Vector2(gridHorizontalSpacing, gridVerticalSpacing);
                grid.padding = new RectOffset(gridPaddingLeft, gridPaddingRight, gridPaddingTop, gridPaddingBottom);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = Mathf.Max(1, gridColumns);

                // Content size fitter can create loops with grids – disable vertical fit
                if (fitter == null)
                {
                    fitter = levelButtonContainer.gameObject.AddComponent<ContentSizeFitter>();
                }
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

                RecalculateGrid(grid);
            }
        }

        /// <summary>
        /// Compute responsive grid cell width and content height.
        /// </summary>
        private void RecalculateGrid(GridLayoutGroup grid)
        {
            if (grid == null) return;
            var rt = levelButtonContainer as RectTransform;
            if (rt == null) return;

            // Responsive columns
            int previousColumns = effectiveColumns > 0 ? effectiveColumns : gridColumns;
            int previousPerPage = LevelsPerPage;
            int cols = Mathf.Max(1, gridColumns);
            if (enableResponsiveColumns)
            {
                float w = rt.rect.width;
                if (w <= oneColumnMaxWidth) cols = 1;
                else if (w <= twoColumnMaxWidth) cols = 2;
                else cols = Mathf.Max(cols, 3);
            }
            float containerWidth = rt.rect.width;
            float totalPadding = gridPaddingLeft + gridPaddingRight;
            float totalSpacing = gridHorizontalSpacing * (cols - 1);
            float cellWidth = responsiveGridCellWidth && containerWidth > 0
                ? Mathf.Max(1f, (containerWidth - totalPadding - totalSpacing) / cols)
                : rt.rect.width / cols;

            grid.cellSize = new Vector2(cellWidth, gridCellHeight);
            grid.constraintCount = cols; // ensure constraint count matches effective columns
            effectiveColumns = cols;

            // Calculate rows and set a preferred height via sizeDelta
            int itemCount = generatedButtons.Count;
            int rows = Mathf.CeilToInt(itemCount / (float)cols);
            float contentHeight = gridPaddingTop + gridPaddingBottom + rows * gridCellHeight + Mathf.Max(0, rows - 1) * gridVerticalSpacing;

            var size = rt.sizeDelta;
            // Keep width delta, only adjust height to enable scrolling
            size.y = contentHeight;
            rt.sizeDelta = size;

            // Detect pagination capacity change and schedule refresh.
            if (layoutMode == LayoutMode.Grid && enablePagination)
            {
                int newPerPage = LevelsPerPage;
                if (newPerPage != previousPerPage || cols != previousColumns)
                {
                    // Clamp current page to new total pages.
                    currentPage = Mathf.Clamp(currentPage, 0, Mathf.Max(0, GetTotalPages() - 1));
                    // Avoid jumping pages to last unlocked after a resize; keep current page stable.
                    preferLastUnlockedSelection = false;
                    // Flag for rebuild after current layout pass.
                    if (!isRefreshingForResize)
                    {
                        needsGridPaginationRefresh = true;
                    }
                }
            }
        }

        /// <summary>
        /// Auto-detect ScrollRect and content/viewport under this object and wire them up.
        /// Also assigns levelButtonContainer from ScrollRect.content or a child named "Content".
        /// </summary>
        private void AutoBindScrollRectAndContainer()
        {
            var sr = GetComponent<ScrollRect>();
            if (sr != null)
            {
                // Ensure viewport
                if (sr.viewport == null)
                {
                    var vp = transform.Find("Viewport") as RectTransform;
                    if (vp == null && transform.childCount > 0)
                    {
                        // Try first child as viewport
                        vp = transform.GetChild(0) as RectTransform;
                    }
                    if (vp != null) sr.viewport = vp;
                }

                // Ensure content
                if (sr.content == null && sr.viewport != null)
                {
                    // Look for Content under viewport
                    var content = sr.viewport.Find("Content") as RectTransform;
                    if (content == null && sr.viewport.childCount > 0)
                    {
                        // Try first child
                        content = sr.viewport.GetChild(0) as RectTransform;
                    }
                    if (content != null) sr.content = content;
                }

                // Prefer vertical scroll-only for level list
                sr.horizontal = false;
                sr.vertical = true;
            }

            // Assign levelButtonContainer if not set
            if (levelButtonContainer == null)
            {
                if (sr != null && sr.content != null)
                {
                    levelButtonContainer = sr.content;
                }
                else
                {
                    // Fallback to a child named Content
                    var t = transform.Find("Viewport/Content") ?? transform.Find("Content");
                    if (t != null) levelButtonContainer = t;
                    else levelButtonContainer = transform; // last resort
                }
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (layoutMode == LayoutMode.Grid)
            {
                var grid = levelButtonContainer != null ? levelButtonContainer.GetComponent<GridLayoutGroup>() : null;
                if (grid != null)
                {
                    RecalculateGrid(grid);
                    // Trigger a layout refresh so pagination/selection stays in sync after resize
                    UpdateLayout();
                    UpdatePaginationControls();
                }
            }
        }

#if UNITY_EDITOR
        private void AssignDefaultPrefabsInEditor()
        {
            // Auto-assign default level button prefab if missing
            if (levelButtonPrefab == null)
            {
                var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Level Button.prefab");
                if (go != null)
                {
                    levelButtonPrefab = go;
                }
            }
        }
#endif
    }
}
