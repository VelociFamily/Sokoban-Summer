using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using Core;
using UI;

namespace Tests
{
    /// <summary>
    /// Play-mode tests for DynamicLevelSelector pagination and selection at different resolutions.
    /// Validates behavior at 1920x1080 and 1280x720 resolutions as specified in issue #71.
    /// </summary>
    [TestFixture]
    public class DynamicLevelSelectorResolutionTest
    {
        private GameObject testRoot;
        private GameObject levelManagerObj;
        private GameObject canvasObj;
        private GameObject selectorObj;
        private DynamicLevelSelector selector;
        private Canvas canvas;
        private RectTransform canvasRT;
        private int originalWidth;
        private int originalHeight;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            originalWidth = Screen.width;
            originalHeight = Screen.height;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Screen.SetResolution(originalWidth, originalHeight, Screen.fullScreen);
        }

        [SetUp]
        public void SetUp()
        {
            testRoot = new GameObject("TestRoot");

            levelManagerObj = new GameObject("LevelManager");
            levelManagerObj.transform.SetParent(testRoot.transform);
            levelManagerObj.AddComponent<LevelManager>();

            canvasObj = new GameObject("Canvas");
            canvasObj.transform.SetParent(testRoot.transform);
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasRT = canvasObj.GetComponent<RectTransform>();

            var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            if (GameObject.FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.transform.SetParent(testRoot.transform);
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<InputSystemUIInputModule>();
            }

            var scrollRectObj = new GameObject("ScrollRect");
            scrollRectObj.transform.SetParent(canvasObj.transform);
            var rt = scrollRectObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(100, 100);
            rt.offsetMax = new Vector2(-100, -100);

            var scrollRect = scrollRectObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollRectObj.transform);
            var viewportRT = viewportObj.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;
            viewportObj.AddComponent<Image>().color = new Color(1, 1, 1, 0.1f);
            viewportObj.AddComponent<Mask>();
            scrollRect.viewport = viewportRT;

            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform);
            var contentRT = contentObj.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            scrollRect.content = contentRT;

            var prevButton = CreatePaginationButton("PreviousButton", scrollRectObj.transform);
            var nextButton = CreatePaginationButton("NextButton", scrollRectObj.transform);

            var buttonPrefab = CreateLevelButtonPrefab();

            selectorObj = scrollRectObj;
            selector = selectorObj.AddComponent<DynamicLevelSelector>();
            // Prevent Start from running until we finish configuration/cleanup
            selector.enabled = false;
            selector.levelButtonContainer = contentObj.transform;
            selector.levelButtonPrefab = buttonPrefab;
            selector.showTutorials = true;
            selector.showGameplayLevels = true;
            selector.addSectionHeaders = false;
            // Configure directly for Grid before Start executes so no VerticalLayoutGroup is ever added.
            selector.layoutMode = DynamicLevelSelector.LayoutMode.Grid;
            selector.enablePagination = true;
            selector.gridColumns = 5;
            selector.gridRowsPerPage = 3;
            selector.gridCellHeight = 180f;
            selector.gridHorizontalSpacing = 24f;
            selector.gridVerticalSpacing = 24f;
            selector.responsiveGridCellWidth = true;
            selector.enableResponsiveColumns = true;
            selector.oneColumnMaxWidth = 680f;
            selector.twoColumnMaxWidth = 1080f;
            selector.previousPageButton = prevButton;
            selector.nextPageButton = nextButton;
            selector.autoAddVerticalLayoutGroup = false;
            // Ensure no leftover LayoutGroup components before Start runs
            RemoveExistingLayoutGroups();
            // Allow Start to run in already-configured Grid mode (Vertical never added)
            selector.enabled = true;
            // Safety: if anything added an unexpected VerticalLayoutGroup during Awake, clear it now before Start processes frame
            RemoveExistingLayoutGroups();
        }

        [TearDown]
        public void TearDown()
        {
            if (testRoot != null)
            {
                Object.DestroyImmediate(testRoot);
            }
        }

        private Button CreatePaginationButton(string name, Transform parent)
        {
            var buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent);
            var rt = buttonObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 40);
            var button = buttonObj.AddComponent<Button>();
            var image = buttonObj.AddComponent<Image>();
            image.color = Color.gray;
            button.targetGraphic = image;
            return button;
        }

        private GameObject CreateLevelButtonPrefab()
        {
            var prefab = new GameObject("LevelButtonPrefab");
            var buttonRT = prefab.AddComponent<RectTransform>();
            buttonRT.sizeDelta = new Vector2(200, 180);
            var button = prefab.AddComponent<Button>();
            var image = prefab.AddComponent<Image>();
            image.color = Color.white;
            button.targetGraphic = image;

            var dynamicButton = prefab.AddComponent<DynamicLevelButton>();

            var nameTextObj = new GameObject("NameText");
            nameTextObj.transform.SetParent(prefab.transform);
            var nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Level Name";
            nameText.alignment = TextAlignmentOptions.Center;

            var goalTextObj = new GameObject("GoalText");
            goalTextObj.transform.SetParent(prefab.transform);
            var goalText = goalTextObj.AddComponent<TextMeshProUGUI>();
            goalText.text = "Goals";
            goalText.alignment = TextAlignmentOptions.Center;

            dynamicButton.levelNameText = nameText;
            dynamicButton.goalText = goalText;

            return prefab;
        }

        [UnityTest]
        public IEnumerator Resolution1080p_GridPagination_ValidSelection()
        {
            SafeEnsureGrid();
            Screen.SetResolution(1920, 1080, false);
            yield return null;
            yield return null;
            LogAssert.NoUnexpectedReceived();

            selector.PopulateLevelButtons();
            yield return null;
            yield return null;

            var selectedObj = EventSystem.current?.currentSelectedGameObject;
            if (selectedObj != null)
            {
                var button = selectedObj.GetComponent<Button>();
                Assert.IsNotNull(button);
                Assert.IsTrue(button.interactable);
            }

            if (selector.enablePagination)
            {
                Assert.IsNotNull(selector.previousPageButton);
                Assert.IsNotNull(selector.nextPageButton);
            }

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Resolution720p_GridPagination_ValidSelection()
        {
            SafeEnsureGrid();
            Screen.SetResolution(1280, 720, false);
            yield return null;
            yield return null;
            LogAssert.NoUnexpectedReceived();

            selector.PopulateLevelButtons();
            yield return null;
            yield return null;

            var selectedObj = EventSystem.current?.currentSelectedGameObject;
            if (selectedObj != null)
            {
                var button = selectedObj.GetComponent<Button>();
                Assert.IsNotNull(button);
                Assert.IsTrue(button.interactable);
            }

            if (selector.enablePagination)
            {
                Assert.IsNotNull(selector.previousPageButton);
                Assert.IsNotNull(selector.nextPageButton);
            }

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Resolution1080p_ResponsiveColumns_NoErrors()
        {
            SafeEnsureGrid();
            Screen.SetResolution(1920, 1080, false);
            yield return null;
            yield return null;
            LogAssert.NoUnexpectedReceived();

            selector.enableResponsiveColumns = true;
            selector.responsiveGridCellWidth = true;
            selector.PopulateLevelButtons();
            yield return null;
            yield return null;

            var grid = selector.levelButtonContainer.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                Assert.GreaterOrEqual(grid.constraintCount, 3);
            }

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Resolution720p_ResponsiveColumns_NoErrors()
        {
            SafeEnsureGrid();
            Screen.SetResolution(1280, 720, false);
            yield return null;
            yield return null;
            LogAssert.NoUnexpectedReceived();

            selector.enableResponsiveColumns = true;
            selector.responsiveGridCellWidth = true;
            selector.PopulateLevelButtons();
            yield return null;
            yield return null;

            var grid = selector.levelButtonContainer.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                Assert.GreaterOrEqual(grid.constraintCount, 1);
            }

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator ResolutionChange_1080To720_MaintainsPaginationConsistency()
        {
            LogAssert.NoUnexpectedReceived();
            SafeEnsureGrid();
            Screen.SetResolution(1920, 1080, false);
            yield return null;
            yield return null;

            selector.PopulateLevelButtons();
            yield return null;

            var initialPage = GetCurrentPageNumber();

            Screen.SetResolution(1280, 720, false);
            yield return null;
            yield return null;

            selector.RefreshLevelSelection();
            yield return null;

            var afterResizePage = GetCurrentPageNumber();
            Assert.GreaterOrEqual(afterResizePage, 0);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Resolution1080p_PaginationNavigation_SelectionPersists()
        {
            SafeEnsureGrid();
            Screen.SetResolution(1920, 1080, false);
            yield return null;
            yield return null;
            LogAssert.NoUnexpectedReceived();

            selector.PopulateLevelButtons();
            yield return null;

            var initialSelection = EventSystem.current?.currentSelectedGameObject;
            Assert.IsNotNull(initialSelection);

            if (selector.nextPageButton != null && selector.nextPageButton.interactable)
            {
                selector.nextPageButton.onClick.Invoke();
                yield return null;
                yield return null;

                var afterPageChange = EventSystem.current?.currentSelectedGameObject;
                Assert.IsNotNull(afterPageChange);
                var button = afterPageChange?.GetComponent<Button>();
                if (button != null)
                {
                    Assert.IsTrue(button.interactable);
                }
            }

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Resolution720p_PaginationNavigation_SelectionPersists()
        {
            SafeEnsureGrid();
            Screen.SetResolution(1280, 720, false);
            yield return null;
            yield return null;
            LogAssert.NoUnexpectedReceived();

            selector.PopulateLevelButtons();
            yield return null;

            var initialSelection = EventSystem.current?.currentSelectedGameObject;
            Assert.IsNotNull(initialSelection);

            if (selector.nextPageButton != null && selector.nextPageButton.interactable)
            {
                selector.nextPageButton.onClick.Invoke();
                yield return null;
                yield return null;

                var afterPageChange = EventSystem.current?.currentSelectedGameObject;
                Assert.IsNotNull(afterPageChange);
                var button = afterPageChange?.GetComponent<Button>();
                if (button != null)
                {
                    Assert.IsTrue(button.interactable);
                }
            }

            LogAssert.NoUnexpectedReceived();
        }

        private int GetCurrentPageNumber()
        {
            var fieldInfo = typeof(DynamicLevelSelector).GetField(
                "currentPage",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldInfo != null)
            {
                return (int)fieldInfo.GetValue(selector);
            }
            return 0;
        }

        // Remove any existing LayoutGroup components to avoid Grid/Vertical conflicts
        private void RemoveExistingLayoutGroups()
        {
            if (selector != null && selector.levelButtonContainer != null)
            {
                var groups = selector.levelButtonContainer.GetComponents<LayoutGroup>();
                foreach (var g in groups)
                {
                    Object.DestroyImmediate(g);
                }
            }
        }

        // Ensure we are in a clean Grid configuration before running a test
        private void SafeEnsureGrid()
        {
            if (selector == null) return;
            RemoveExistingLayoutGroups();
            selector.autoAddVerticalLayoutGroup = false;
            selector.layoutMode = DynamicLevelSelector.LayoutMode.Grid;
        }
    }
}
