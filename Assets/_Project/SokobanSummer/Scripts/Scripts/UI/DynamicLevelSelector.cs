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
    /// Orchestrates level selection UI by coordinating data, button pooling, and layout.
    /// Refactored to use single-responsibility components for better maintainability.
    /// </summary>
    public class DynamicLevelSelector : MonoBehaviour
    {
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
        public float buttonSpacing = 8f;

        [Tooltip("Auto-add a VerticalLayoutGroup to the container if none is present")]
        public bool autoAddVerticalLayoutGroup = true;

        [Tooltip("Preferred height for each level button when using layout groups")]
        public float buttonHeight = 96f;

        [Tooltip("Preferred height for section headers when using layout groups")]
        public float headerHeight = 80f;

        [Tooltip("Stretch buttons to container width when using layout groups")]
        public bool stretchButtonsToContainerWidth = true;

        [Tooltip("Choose how to lay out the items")]
        public LayoutController.LayoutMode layoutMode = LayoutController.LayoutMode.VerticalList;

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

        // Component instances
        private LevelDataProvider dataProvider;
        private ButtonPoolManager buttonPool;
        private LayoutController layoutController;

        private bool preferLastUnlockedSelection = true;

        private void Start()
        {
            InitializeComponents();
            AutoBindScrollRectAndContainer();
            AutoBindPaginationButtonsIfMissing();
            EnsureOrConfigureLayoutGroup();
            HookPaginationButtons();
            PopulateLevelButtons();
        }

        private void OnValidate()
        {
            // Apply layout changes live in editor
            AutoBindScrollRectAndContainer();
            AutoBindPaginationButtonsIfMissing();
            
#if UNITY_EDITOR
            // Defer component modification to avoid "DestroyImmediate during OnValidate" errors
            EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                EnsureOrConfigureLayoutGroup();
                AssignDefaultPrefabsInEditor();
                HookPaginationButtons();
                UpdateLayout();
                UpdatePaginationControls();
            };
#endif
        }

        /// <summary>
        /// Initialize component instances
        /// </summary>
        private void InitializeComponents()
        {
            dataProvider = new LevelDataProvider();
            buttonPool = new ButtonPoolManager(levelButtonContainer, levelButtonPrefab, sectionHeaderPrefab, lockedLevelSprite);
            layoutController = new LayoutController(levelButtonContainer);

            // Configure layout controller
            layoutController.Configure(
                layoutMode,
                buttonSpacing,
                buttonHeight,
                headerHeight,
                stretchButtonsToContainerWidth,
                gridColumns,
                gridCellHeight,
                gridHorizontalSpacing,
                gridVerticalSpacing,
                gridPaddingLeft, gridPaddingRight, gridPaddingTop, gridPaddingBottom,
                responsiveGridCellWidth,
                enableResponsiveColumns,
                oneColumnMaxWidth,
                twoColumnMaxWidth,
                enablePagination,
                gridRowsPerPage
            );
        }

        /// <summary>
        /// Clear and regenerate all level buttons
        /// </summary>
        public void PopulateLevelButtons()
        {
            // Ensure components are initialized
            if (dataProvider == null || buttonPool == null || layoutController == null)
            {
                InitializeComponents();
            }

            // Return buttons to pool instead of destroying
            buttonPool.ReturnAllToPool();
            dataProvider.ClearButtonRegistrations();

            var levelManager = ServiceLocator.Get<LevelManager>();
            if (levelManager == null)
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

            // Gather levels using data provider
            dataProvider.GatherLevels(showTutorials, showGameplayLevels);
            int defaultSelectionIndex = dataProvider.GetDefaultSelectionIndex();
            bool useLastUnlocked = preferLastUnlockedSelection && defaultSelectionIndex >= 0;

            if (layoutMode == LayoutController.LayoutMode.Grid)
            {
                // Grid layout with pagination
                if (!enablePagination)
                {
                    layoutController.SetCurrentPage(0, dataProvider.GetLevelCount());
                }
                else if (useLastUnlocked)
                {
                    int targetPage = layoutController.GetPageForIndex(defaultSelectionIndex);
                    layoutController.SetCurrentPage(targetPage, dataProvider.GetLevelCount());
                }
                else
                {
                    layoutController.SetCurrentPage(layoutController.CurrentPage, dataProvider.GetLevelCount());
                }

                RenderCurrentGridPage();
            }
            else
            {
                // Vertical list layout with optional section headers
                RenderVerticalList();
            }

            UpdateLayout();
            UpdatePaginationControls();
            SelectDefaultLevelButton(defaultSelectionIndex, useLastUnlocked);
            preferLastUnlockedSelection = true;
        }

        /// <summary>
        /// Render vertical list with optional section headers
        /// </summary>
        private void RenderVerticalList()
        {
            bool tutorialHeaderAdded = false;
            bool gameplayHeaderAdded = false;

            var levels = dataProvider.OrderedLevels;
            foreach (var levelInfo in levels)
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
        }

        /// <summary>
        /// Render current page of grid layout
        /// </summary>
        private void RenderCurrentGridPage()
        {
            int levelCount = dataProvider.GetLevelCount();
            if (levelCount == 0)
            {
                UpdateLayout();
                return;
            }

            var (startIndex, endIndex) = layoutController.GetCurrentPageRange(levelCount);

            for (int i = startIndex; i < endIndex; i++)
            {
                var levelInfo = dataProvider.GetLevelAt(i);
                if (levelInfo != null)
                {
                    CreateLevelButton(levelInfo);
                }
            }

            UpdateLayout();
        }

        /// <summary>
        /// Create a section header
        /// </summary>
        private void CreateSectionHeader(string title)
        {
            var headerObj = buttonPool.GetHeader();
            if (headerObj == null) return;

            var headerText = headerObj.GetComponentInChildren<Text>();
            if (headerText != null)
                headerText.text = title;

            layoutController.ConfigureChildForLayout(headerObj, isHeader: true);
        }

        /// <summary>
        /// Create a button for a specific level
        /// </summary>
        private void CreateLevelButton(LevelManager.LevelInfo levelInfo)
        {
            var dynamicButton = buttonPool.CreateLevelButton(levelInfo);
            if (dynamicButton == null)
                return;

            dataProvider.RegisterButton(levelInfo, dynamicButton);
            layoutController.ConfigureChildForLayout(dynamicButton.gameObject, isHeader: false);
        }

        /// <summary>
        /// Update the layout of the level buttons
        /// </summary>
        private void UpdateLayout()
        {
            if (layoutController == null)
                return;

            int itemCount = buttonPool.ActiveButtons.Count;
            layoutController.ApplyLayout(itemCount);

            // Check if pagination refresh is needed after layout
            if (layoutController.NeedsPaginationRefresh)
            {
                layoutController.SetRefreshingForResize(true);
                layoutController.ClearRefreshFlags();
                
                // Re-render page with updated capacity
                buttonPool.ReturnAllToPool();
                dataProvider.ClearButtonRegistrations();

                if (layoutMode == LayoutController.LayoutMode.Grid)
                {
                    RenderCurrentGridPage();
                    UpdatePaginationControls();
                    int defaultSelectionIndex = dataProvider.GetDefaultSelectionIndex();
                    SelectDefaultLevelButton(defaultSelectionIndex, preferLastUnlockedSelection);
                }

                layoutController.SetRefreshingForResize(false);
                
                // Apply layout again after refresh
                var rt = levelButtonContainer.GetComponent<RectTransform>();
                if (rt != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                }
            }
        }

        /// <summary>
        /// Refresh the level selection (useful when progression changes)
        /// </summary>
        public void RefreshLevelSelection()
        {
            if (layoutMode == LayoutController.LayoutMode.Grid && layoutController != null)
            {
                layoutController.SetCurrentPage(layoutController.CurrentPage, dataProvider.GetLevelCount());
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
            if (buttonPool != null && buttonPool.ActiveButtons.Count > 0)
            {
                buttonPool.RefreshButtonStates();
            }
            UpdatePaginationControls();

            var defaultIndex = dataProvider?.GetDefaultSelectionIndex() ?? -1;
            SelectDefaultLevelButton(defaultIndex, true);
        }

        /// <summary>
        /// Select the default level button based on progression
        /// </summary>
        private void SelectDefaultLevelButton(int defaultSelectionIndex, bool useLastUnlocked)
        {
            DynamicLevelButton targetButton = null;

            if (useLastUnlocked && defaultSelectionIndex >= 0 && dataProvider != null)
            {
                var targetLevel = dataProvider.GetLevelAt(defaultSelectionIndex);
                if (targetLevel != null && dataProvider.LevelToButtonMap.TryGetValue(targetLevel, out var mappedButton))
                {
                    if (mappedButton != null && mappedButton.button != null && mappedButton.button.interactable)
                    {
                        targetButton = mappedButton;
                    }
                }
            }

            if (targetButton == null && buttonPool != null)
            {
                targetButton = buttonPool.FindFirstInteractableButton();
            }

            if (targetButton == null && buttonPool != null && buttonPool.ActiveLevelButtons.Count > 0)
            {
                targetButton = buttonPool.ActiveLevelButtons[0];
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

        /// <summary>
        /// Hook up pagination button listeners
        /// </summary>
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

        /// <summary>
        /// Attempt to auto-bind pagination buttons by name if they are not explicitly assigned.
        /// </summary>
        private void AutoBindPaginationButtonsIfMissing()
        {
            if (!enablePagination || layoutMode != LayoutController.LayoutMode.Grid)
                return;

            if (previousPageButton != null && nextPageButton != null)
                return;

            List<Transform> searchRoots = new List<Transform>();
            searchRoots.Add(transform);
            if (transform.parent != null)
            {
                searchRoots.Add(transform.parent);
                if (transform.parent.parent != null)
                    searchRoots.Add(transform.parent.parent);
            }

            foreach (var root in searchRoots)
            {
                if (root == null) continue;
                var buttons = root.GetComponentsInChildren<Button>(true);
                foreach (var btn in buttons)
                {
                    if (btn == null) continue;
                    string n = btn.gameObject.name.ToLower();
                    if (previousPageButton == null && (n.Contains("prev") || n.Contains("previous")))
                    {
                        previousPageButton = btn;
                    }
                    else if (nextPageButton == null && n.Contains("next"))
                    {
                        nextPageButton = btn;
                    }
                    if (previousPageButton != null && nextPageButton != null)
                        break;
                }
                if (previousPageButton != null && nextPageButton != null)
                    break;
            }
        }

        private void GoToPreviousPage()
        {
            if (layoutController == null || !layoutController.GoToPreviousPage())
                return;

            preferLastUnlockedSelection = false;
            PopulateLevelButtons();
        }

        private void GoToNextPage()
        {
            if (layoutController == null || dataProvider == null)
                return;

            if (!layoutController.GoToNextPage(dataProvider.GetLevelCount()))
                return;

            preferLastUnlockedSelection = false;
            PopulateLevelButtons();
        }

        private void UpdatePaginationControls()
        {
            if (layoutController == null || dataProvider == null)
                return;

            // Clamp current page
            layoutController.SetCurrentPage(layoutController.CurrentPage, dataProvider.GetLevelCount());

            int levelCount = dataProvider.GetLevelCount();
            int itemsPerPage = layoutController.GetItemsPerPage();
            bool shouldShow = enablePagination && 
                             layoutMode == LayoutController.LayoutMode.Grid && 
                             levelCount > itemsPerPage;
            
            int totalPages = layoutController.GetTotalPages(levelCount);
            int currentPage = layoutController.CurrentPage;

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
        /// Ensure or configure the container to use the selected layout mode.
        /// </summary>
        private void EnsureOrConfigureLayoutGroup()
        {
#if UNITY_EDITOR
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-batchmode") >= 0)
            {
                return;
            }
#endif

            if (levelButtonContainer == null) return;

            var fitter = levelButtonContainer.GetComponent<ContentSizeFitter>();

            // Remove incorrect layout groups
            var allGroups = levelButtonContainer.GetComponents<LayoutGroup>();
            bool needsCleanup = false;
            if (allGroups != null && allGroups.Length > 0)
            {
                foreach (var g in allGroups)
                {
                    if (g == null) continue;
                    bool isTarget = (layoutMode == LayoutController.LayoutMode.Grid && g is GridLayoutGroup)
                                    || (layoutMode == LayoutController.LayoutMode.VerticalList && g is VerticalLayoutGroup);
                    if (!isTarget)
                    {
#if UNITY_EDITOR
                        if (!Application.isPlaying)
                        {
                            // Use DestroyImmediate only if safe (e.g. via delayCall in OnValidate, or direct call in Editor)
                            // But here we are called from Start() too.
                            // If called from OnValidate via delayCall, it's safe.
                            UnityEngine.Object.DestroyImmediate(g);
                        }
                        else
                        {
                            UnityEngine.Object.Destroy(g);
                            needsCleanup = true;
                        }
#else
                        UnityEngine.Object.Destroy(g);
                        needsCleanup = true;
#endif
                    }
                }
            }

            if (needsCleanup)
            {
                return;
            }

            var existingVertical = levelButtonContainer.GetComponent<VerticalLayoutGroup>();
            var existingGrid = levelButtonContainer.GetComponent<GridLayoutGroup>();

            if (layoutMode == LayoutController.LayoutMode.VerticalList)
            {
                var v = existingVertical;
                if (v == null && autoAddVerticalLayoutGroup)
                {
                    v = levelButtonContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                    if (v == null)
                    {
                        return;
                    }
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

                if (autoAddVerticalLayoutGroup)
                {
                    if (fitter == null)
                    {
                        fitter = levelButtonContainer.gameObject.AddComponent<ContentSizeFitter>();
                    }
                    if (fitter != null)
                    {
                        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    }
                }
            }
            else // Grid
            {
                var lingeringVertical = levelButtonContainer.GetComponent<VerticalLayoutGroup>();
                if (lingeringVertical != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        UnityEngine.Object.DestroyImmediate(lingeringVertical);
                    }
                    else
                    {
                        UnityEngine.Object.Destroy(lingeringVertical);
                        return;
                    }
#else
                    UnityEngine.Object.Destroy(lingeringVertical);
                    return;
#endif
                }

                var grid = existingGrid;
                if (grid == null)
                {
                    grid = levelButtonContainer.gameObject.AddComponent<GridLayoutGroup>();
                    if (grid == null)
                    {
                        return;
                    }
                }
                grid.childAlignment = TextAnchor.UpperCenter;
                grid.spacing = new Vector2(gridHorizontalSpacing, gridVerticalSpacing);
                grid.padding = new RectOffset(gridPaddingLeft, gridPaddingRight, gridPaddingTop, gridPaddingBottom);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = Mathf.Max(1, gridColumns);

                if (fitter == null)
                {
                    fitter = levelButtonContainer.gameObject.AddComponent<ContentSizeFitter>();
                }
                if (fitter != null)
                {
                    fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                }
            }
        }

        /// <summary>
        /// Auto-detect ScrollRect and content/viewport under this object and wire them up.
        /// </summary>
        private void AutoBindScrollRectAndContainer()
        {
            var sr = GetComponent<ScrollRect>();
            if (sr != null)
            {
                if (sr.viewport == null)
                {
                    var vp = transform.Find("Viewport") as RectTransform;
                    if (vp == null && transform.childCount > 0)
                    {
                        vp = transform.GetChild(0) as RectTransform;
                    }
                    if (vp != null) sr.viewport = vp;
                }

                if (sr.content == null && sr.viewport != null)
                {
                    var content = sr.viewport.Find("Content") as RectTransform;
                    if (content == null && sr.viewport.childCount > 0)
                    {
                        content = sr.viewport.GetChild(0) as RectTransform;
                    }
                    if (content != null) sr.content = content;
                }

                sr.horizontal = false;
                sr.vertical = true;
            }

            if (levelButtonContainer == null)
            {
                if (sr != null && sr.content != null)
                {
                    levelButtonContainer = sr.content;
                }
                else
                {
                    var t = transform.Find("Viewport/Content") ?? transform.Find("Content");
                    if (t != null) levelButtonContainer = t;
                    else levelButtonContainer = transform;
                }
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (layoutMode == LayoutController.LayoutMode.Grid && layoutController != null && dataProvider != null)
            {
                layoutController.OnDimensionsChanged(dataProvider.GetLevelCount());
                UpdateLayout();
                UpdatePaginationControls();
            }
        }

        private void OnDestroy()
        {
            // Clean up pooled objects
            buttonPool?.DestroyAll();
        }

#if UNITY_EDITOR
        private void AssignDefaultPrefabsInEditor()
        {
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
