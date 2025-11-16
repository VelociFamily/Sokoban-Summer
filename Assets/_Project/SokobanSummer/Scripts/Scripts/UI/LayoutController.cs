using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Handles layout calculations, pagination, and grid/list formatting.
    /// Single responsibility: Layout and pagination logic.
    /// </summary>
    public class LayoutController
    {
        public enum LayoutMode
        {
            VerticalList,
            Grid
        }

        // Configuration
        private readonly Transform container;
        private LayoutMode layoutMode;
        private float buttonSpacing;
        private float buttonHeight;
        private float headerHeight;
        private bool stretchButtonsToContainerWidth;

        // Grid settings
        private int gridColumns;
        private float gridCellHeight;
        private float gridHorizontalSpacing;
        private float gridVerticalSpacing;
        private int gridPaddingLeft, gridPaddingRight, gridPaddingTop, gridPaddingBottom;
        private bool responsiveGridCellWidth;
        private bool enableResponsiveColumns;
        private float oneColumnMaxWidth;
        private float twoColumnMaxWidth;

        // Pagination
        private bool enablePagination;
        private int gridRowsPerPage;
        private int currentPage;
        private int effectiveColumns;

        // Change detection
        private bool needsPaginationRefresh;
        private bool isRefreshingForResize;

        public int CurrentPage => currentPage;
        public int EffectiveColumns => effectiveColumns > 0 ? effectiveColumns : gridColumns;
        public bool NeedsPaginationRefresh => needsPaginationRefresh;

        public LayoutController(Transform container)
        {
            this.container = container;
            this.effectiveColumns = 0;
            this.currentPage = 0;
        }

        /// <summary>
        /// Configure layout mode and settings
        /// </summary>
        public void Configure(
            LayoutMode mode,
            float buttonSpacing,
            float buttonHeight,
            float headerHeight,
            bool stretchButtons,
            int gridColumns,
            float gridCellHeight,
            float gridHSpacing,
            float gridVSpacing,
            int padLeft, int padRight, int padTop, int padBottom,
            bool responsiveCellWidth,
            bool enableResponsive,
            float oneColMax,
            float twoColMax,
            bool enablePagination,
            int rowsPerPage)
        {
            this.layoutMode = mode;
            this.buttonSpacing = buttonSpacing;
            this.buttonHeight = buttonHeight;
            this.headerHeight = headerHeight;
            this.stretchButtonsToContainerWidth = stretchButtons;
            this.gridColumns = gridColumns;
            this.gridCellHeight = gridCellHeight;
            this.gridHorizontalSpacing = gridHSpacing;
            this.gridVerticalSpacing = gridVSpacing;
            this.gridPaddingLeft = padLeft;
            this.gridPaddingRight = padRight;
            this.gridPaddingTop = padTop;
            this.gridPaddingBottom = padBottom;
            this.responsiveGridCellWidth = responsiveCellWidth;
            this.enableResponsiveColumns = enableResponsive;
            this.oneColumnMaxWidth = oneColMax;
            this.twoColumnMaxWidth = twoColMax;
            this.enablePagination = enablePagination;
            this.gridRowsPerPage = rowsPerPage;
        }

        /// <summary>
        /// Get items per page based on current layout
        /// </summary>
        public int GetItemsPerPage()
        {
            return Mathf.Max(1, EffectiveColumns * Mathf.Max(1, gridRowsPerPage));
        }

        /// <summary>
        /// Calculate total pages for a given item count
        /// </summary>
        public int GetTotalPages(int itemCount)
        {
            if (!enablePagination || layoutMode != LayoutMode.Grid || itemCount == 0)
                return 1;

            int perPage = GetItemsPerPage();
            if (perPage <= 0) return 1;
            return Mathf.Max(1, Mathf.CeilToInt(itemCount / (float)perPage));
        }

        /// <summary>
        /// Set current page with clamping
        /// </summary>
        public void SetCurrentPage(int page, int totalItems)
        {
            int maxPage = Mathf.Max(0, GetTotalPages(totalItems) - 1);
            currentPage = Mathf.Clamp(page, 0, maxPage);
        }

        /// <summary>
        /// Navigate to previous page
        /// </summary>
        public bool GoToPreviousPage()
        {
            if (currentPage <= 0)
                return false;

            currentPage--;
            return true;
        }

        /// <summary>
        /// Navigate to next page
        /// </summary>
        public bool GoToNextPage(int totalItems)
        {
            int totalPages = GetTotalPages(totalItems);
            if (currentPage >= totalPages - 1)
                return false;

            currentPage++;
            return true;
        }

        /// <summary>
        /// Get page range for current page
        /// </summary>
        public (int startIndex, int endIndex) GetCurrentPageRange(int totalItems)
        {
            if (!enablePagination || layoutMode != LayoutMode.Grid)
                return (0, totalItems);

            int perPage = GetItemsPerPage();
            int startIndex = currentPage * perPage;
            int endIndex = Mathf.Min(totalItems, startIndex + perPage);
            startIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, totalItems - 1));

            return (startIndex, endIndex);
        }

        /// <summary>
        /// Calculate which page contains a specific index
        /// </summary>
        public int GetPageForIndex(int index)
        {
            if (!enablePagination || layoutMode != LayoutMode.Grid)
                return 0;

            int perPage = GetItemsPerPage();
            if (perPage <= 0)
                return 0;

            return Mathf.Max(0, index / perPage);
        }

        /// <summary>
        /// Configure child element for layout
        /// </summary>
        public void ConfigureChildForLayout(GameObject go, bool isHeader)
        {
            if (go == null) return;

            var rt = go.GetComponent<RectTransform>();
            if (rt != null && stretchButtonsToContainerWidth && layoutMode == LayoutMode.VerticalList)
            {
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
        /// Apply layout rebuild and grid recalculation
        /// </summary>
        public void ApplyLayout(int itemCount)
        {
            var layoutGroup = container.GetComponent<LayoutGroup>();
            if (layoutGroup != null)
            {
                var rt = container.GetComponent<RectTransform>();
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

                var grid = container.GetComponent<GridLayoutGroup>();
                if (grid != null)
                {
                    RecalculateGrid(grid, itemCount);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

                    // Check if pagination needs refresh after grid recalculation
                    if (needsPaginationRefresh && !isRefreshingForResize)
                    {
                        isRefreshingForResize = true;
                        needsPaginationRefresh = false;
                        return; // Signal to caller that refresh is needed
                    }
                }
                return;
            }

            // Fallback: Manual vertical positioning
            ApplyManualVerticalLayout();
        }

        /// <summary>
        /// Manually position items vertically when no layout group exists
        /// </summary>
        private void ApplyManualVerticalLayout()
        {
            float currentY = 0f;
            for (int i = 0; i < container.childCount; i++)
            {
                var go = container.GetChild(i).gameObject;
                if (!go.activeSelf) continue;

                var rt = go.GetComponent<RectTransform>();
                if (rt == null) continue;

                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);

                float h = buttonHeight;
                var le = go.GetComponent<LayoutElement>();
                if (le != null && le.preferredHeight > 0)
                    h = le.preferredHeight;

                rt.anchoredPosition = new Vector2(0f, -currentY);

                if (stretchButtonsToContainerWidth)
                {
                    rt.sizeDelta = new Vector2(0f, h);
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
        /// Recalculate grid layout based on responsive settings
        /// </summary>
        private void RecalculateGrid(GridLayoutGroup grid, int itemCount)
        {
            if (grid == null) return;
            var rt = container as RectTransform;
            if (rt == null) return;

            int previousColumns = effectiveColumns > 0 ? effectiveColumns : gridColumns;
            int previousPerPage = GetItemsPerPage();

            // Calculate responsive columns
            int cols = Mathf.Max(1, gridColumns);
            if (enableResponsiveColumns)
            {
                float w = rt.rect.width;
                if (w <= oneColumnMaxWidth) 
                    cols = 1;
                else if (w <= twoColumnMaxWidth) 
                    cols = 2;
                else 
                    cols = Mathf.Max(cols, 3);
            }

            // Calculate cell width
            float containerWidth = rt.rect.width;
            float totalPadding = gridPaddingLeft + gridPaddingRight;
            float totalSpacing = gridHorizontalSpacing * (cols - 1);
            float cellWidth = responsiveGridCellWidth && containerWidth > 0
                ? Mathf.Max(1f, (containerWidth - totalPadding - totalSpacing) / cols)
                : rt.rect.width / cols;

            grid.cellSize = new Vector2(cellWidth, gridCellHeight);
            grid.constraintCount = cols;
            effectiveColumns = cols;

            // Calculate content height
            int rows = Mathf.CeilToInt(itemCount / (float)cols);
            float contentHeight = gridPaddingTop + gridPaddingBottom + 
                                  rows * gridCellHeight + 
                                  Mathf.Max(0, rows - 1) * gridVerticalSpacing;

            var size = rt.sizeDelta;
            size.y = contentHeight;
            rt.sizeDelta = size;

            // Detect pagination capacity change
            if (layoutMode == LayoutMode.Grid && enablePagination)
            {
                int newPerPage = GetItemsPerPage();
                if (newPerPage != previousPerPage || cols != previousColumns)
                {
                    if (!isRefreshingForResize)
                    {
                        needsPaginationRefresh = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handle dimension changes (called from MonoBehaviour)
        /// </summary>
        public void OnDimensionsChanged(int itemCount)
        {
            if (layoutMode == LayoutMode.Grid)
            {
                var grid = container?.GetComponent<GridLayoutGroup>();
                if (grid != null)
                {
                    RecalculateGrid(grid, itemCount);
                }
            }
        }

        /// <summary>
        /// Clear refresh flags
        /// </summary>
        public void ClearRefreshFlags()
        {
            needsPaginationRefresh = false;
            isRefreshingForResize = false;
        }

        /// <summary>
        /// Set refresh state
        /// </summary>
        public void SetRefreshingForResize(bool value)
        {
            isRefreshingForResize = value;
        }
    }
}
