using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core;

namespace UI
{
    /// <summary>
    /// Dynamically populates level selection UI based on available levels
    /// Replaces hardcoded level buttons with dynamic generation
    /// </summary>
    public class DynamicLevelSelector : MonoBehaviour
    {
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
        public int gridColumns = 2;

        [Tooltip("Cell height for Grid layout")]
        public float gridCellHeight = 212f;

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

        private List<GameObject> generatedButtons = new List<GameObject>();

        private void Start()
        {
            AutoBindScrollRectAndContainer();
            EnsureOrConfigureLayoutGroup();
            PopulateLevelButtons();
        }

        private void OnValidate()
        {
            // Apply layout changes live in editor
            AutoBindScrollRectAndContainer();
            EnsureOrConfigureLayoutGroup();
            AssignDefaultPrefabsInEditor();
            UpdateLayout();
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

            // Add tutorial levels
            if (showTutorials)
            {
                var tutorials = LevelManager.Instance.GetLevels(SceneType.TutorialLevel);
                if (tutorials.Count > 0)
                {
                    if (addSectionHeaders && layoutMode == LayoutMode.VerticalList)
                        CreateSectionHeader("Tutorials");

                    foreach (var tutorial in tutorials)
                    {
                        CreateLevelButton(tutorial);
                    }
                }
            }

            // Add gameplay levels
            if (showGameplayLevels)
            {
                var levels = LevelManager.Instance.GetLevels(SceneType.GameplayLevel);
                if (levels.Count > 0)
                {
                    if (addSectionHeaders && layoutMode == LayoutMode.VerticalList)
                        CreateSectionHeader("Levels");

                    foreach (var level in levels)
                    {
                        CreateLevelButton(level);
                    }
                }
            }

            // Update layout
            UpdateLayout();
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

            dynamicButton.SetupLevel(levelInfo);

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
                    DestroyImmediate(button);
            }
            generatedButtons.Clear();
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
                        DestroyImmediate(existingGrid);
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
                    DestroyImmediate(existingVertical);
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

            // Calculate rows and set a preferred height via sizeDelta
            int itemCount = generatedButtons.Count;
            int rows = Mathf.CeilToInt(itemCount / (float)cols);
            float contentHeight = gridPaddingTop + gridPaddingBottom + rows * gridCellHeight + Mathf.Max(0, rows - 1) * gridVerticalSpacing;

            var size = rt.sizeDelta;
            // Keep width delta, only adjust height to enable scrolling
            size.y = contentHeight;
            rt.sizeDelta = size;
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
