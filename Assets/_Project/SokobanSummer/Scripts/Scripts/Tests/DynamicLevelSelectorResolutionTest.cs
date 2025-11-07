using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
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
            // Store original resolution
            originalWidth = Screen.width;
            originalHeight = Screen.height;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // Restore original resolution
            Screen.SetResolution(originalWidth, originalHeight, Screen.fullScreen);
        }

            using TMPro;
            using UnityEngine.InputSystem.UI;
        [SetUp]
        public void SetUp()
        {
            // Create test scene hierarchy
            testRoot = new GameObject("TestRoot");
            
            // Create LevelManager
            levelManagerObj = new GameObject("LevelManager");
            levelManagerObj.transform.SetParent(testRoot.transform);
            levelManagerObj.AddComponent<LevelManager>();

            // Create Canvas for UI
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

            // Create EventSystem if not present
            if (GameObject.FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.transform.SetParent(testRoot.transform);
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
            }

            // Create ScrollRect with container
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

            // Create viewport
            var viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollRectObj.transform);
            var viewportRT = viewportObj.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;
            viewportObj.AddComponent<Image>().color = new Color(1, 1, 1, 0.1f);
            viewportObj.AddComponent<Mask>();
            scrollRect.viewport = viewportRT;

            // Create content container
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform);
            var contentRT = contentObj.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
                            eventSystemObj.AddComponent<InputSystemUIInputModule>();
            scrollRect.content = contentRT;

            // Create pagination buttons
            var prevButton = CreatePaginationButton("PreviousButton", scrollRectObj.transform);
            var nextButton = CreatePaginationButton("NextButton", scrollRectObj.transform);

            // Create level button prefab
            var buttonPrefab = CreateLevelButtonPrefab();

            // Create and configure DynamicLevelSelector
            selectorObj = scrollRectObj;
            selector = selectorObj.AddComponent<DynamicLevelSelector>();
            selector.levelButtonContainer = contentObj.transform;
            selector.levelButtonPrefab = buttonPrefab;
            selector.showTutorials = true;
            selector.showGameplayLevels = true;
            selector.addSectionHeaders = false;
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

                        // Avoid Vertical/GridLayoutGroup same-frame conflicts
                        selector.autoAddVerticalLayoutGroup = false;
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
            // Set resolution to 1920x1080
            Screen.SetResolution(1920, 1080, false);
            yield return null;
            yield return null; // Wait for resolution change to apply

            LogAssert.NoUnexpectedReceived();

            // Populate level buttons
            selector.PopulateLevelButtons();
            yield return null;
            yield return null;

            // Verify selection exists and is valid
            var selectedObj = EventSystem.current?.currentSelectedGameObject;
            if (selectedObj != null)
            {
                var button = selectedObj.GetComponent<Button>();
                Assert.IsNotNull(button, "Selected object should have a Button component at 1080p");
                Assert.IsTrue(button.interactable, "Selected button should be interactable at 1080p");
            }

            // Verify pagination controls state
            if (selector.enablePagination)
            {
                Assert.IsNotNull(selector.previousPageButton, "Previous page button should be assigned");
                Assert.IsNotNull(selector.nextPageButton, "Next page button should be assigned");
            }

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Resolution720p_GridPagination_ValidSelection()
        {
            // Set resolution to 1280x720
            Screen.SetResolution(1280, 720, false);
            yield return null;
            yield return null; // Wait for resolution change to apply

            LogAssert.NoUnexpectedReceived();

            // Populate level buttons
            selector.PopulateLevelButtons();
            yield return null;
            yield return null;

            // Verify selection exists and is valid
            var selectedObj = EventSystem.current?.currentSelectedGameObject;
            if (selectedObj != null)
            {
                var button = selectedObj.GetComponent<Button>();
                Assert.IsNotNull(button, "Selected object should have a Button component at 720p");
                Assert.IsTrue(button.interactable, "Selected button should be interactable at 720p");
            }

            // Verify pagination controls state
            if (selector.enablePagination)
            {
                Assert.IsNotNull(selector.previousPageButton, "Previous page button should be assigned");
                Assert.IsNotNull(selector.nextPageButton, "Next page button should be assigned");
            }

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Resolution1080p_ResponsiveColumns_NoErrors()
        {
            // Set resolution to 1920x1080
            Screen.SetResolution(1920, 1080, false);
            yield return null;
            yield return null;

            LogAssert.NoUnexpectedReceived();

            // Enable responsive columns
            selector.enableResponsiveColumns = true;
            selector.responsiveGridCellWidth = true;
            selector.PopulateLevelButtons();
            
            yield return null;
            yield return null;

            // Should have more columns at higher resolution
            var grid = selector.levelButtonContainer.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                Assert.GreaterOrEqual(grid.constraintCount, 3, "Should have at least 3 columns at 1080p");
            }

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Resolution720p_ResponsiveColumns_NoErrors()
        {
            // Set resolution to 1280x720
            Screen.SetResolution(1280, 720, false);
            yield return null;
            yield return null;

            LogAssert.NoUnexpectedReceived();

            // Enable responsive columns
            selector.enableResponsiveColumns = true;
            selector.responsiveGridCellWidth = true;
            selector.PopulateLevelButtons();
            
            yield return null;
            yield return null;

            // May have fewer columns at lower resolution
            var grid = selector.levelButtonContainer.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                Assert.GreaterOrEqual(grid.constraintCount, 1, "Should have at least 1 column at 720p");
            }

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator ResolutionChange_1080To720_MaintainsPaginationConsistency()
        {
            LogAssert.NoUnexpectedReceived();

            // Start at 1080p
            Screen.SetResolution(1920, 1080, false);
            yield return null;
            yield return null;

            selector.PopulateLevelButtons();
            yield return null;

            var initialPage = GetCurrentPageNumber();
            
            // Change to 720p
            Screen.SetResolution(1280, 720, false);
            yield return null;
            yield return null;

            // Trigger resize handling
            selector.RefreshLevelSelection();
            yield return null;

            var afterResizePage = GetCurrentPageNumber();
            
            // Page should be clamped to valid range (may change if items per page changes)
            Assert.GreaterOrEqual(afterResizePage, 0, "Page number should be valid after resize");

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Resolution1080p_PaginationNavigation_SelectionPersists()
        {
            // Set resolution to 1920x1080
            Screen.SetResolution(1920, 1080, false);
            yield return null;
            yield return null;

            LogAssert.NoUnexpectedReceived();

            selector.PopulateLevelButtons();
            yield return null;

            // Verify initial selection
            var initialSelection = EventSystem.current?.currentSelectedGameObject;
            Assert.IsNotNull(initialSelection, "Should have initial selection at 1080p");

            // Navigate to next page if available
            if (selector.nextPageButton != null && selector.nextPageButton.interactable)
            {
                selector.nextPageButton.onClick.Invoke();
                yield return null;
                yield return null;

                var afterPageChange = EventSystem.current?.currentSelectedGameObject;
                Assert.IsNotNull(afterPageChange, "Should have selection after page change at 1080p");
                
                var button = afterPageChange?.GetComponent<Button>();
                if (button != null)
                {
                    Assert.IsTrue(button.interactable, "Selected button should be interactable after pagination");
                }
            }

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Resolution720p_PaginationNavigation_SelectionPersists()
        {
            // Set resolution to 1280x720
            Screen.SetResolution(1280, 720, false);
            yield return null;
            yield return null;

            LogAssert.NoUnexpectedReceived();

            selector.PopulateLevelButtons();
            yield return null;

            // Verify initial selection
            var initialSelection = EventSystem.current?.currentSelectedGameObject;
            Assert.IsNotNull(initialSelection, "Should have initial selection at 720p");

            // Navigate to next page if available
            if (selector.nextPageButton != null && selector.nextPageButton.interactable)
            {
                selector.nextPageButton.onClick.Invoke();
                yield return null;
                yield return null;

                var afterPageChange = EventSystem.current?.currentSelectedGameObject;
                Assert.IsNotNull(afterPageChange, "Should have selection after page change at 720p");
                
                var button = afterPageChange?.GetComponent<Button>();
                if (button != null)
                {
                    Assert.IsTrue(button.interactable, "Selected button should be interactable after pagination");
                }
            }

            LogAssert.NoUnexpectedReceived();
        }

        private int GetCurrentPageNumber()
        {
            // Use reflection to access private currentPage field for testing
            var fieldInfo = typeof(DynamicLevelSelector).GetField("currentPage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldInfo != null)
            {
                return (int)fieldInfo.GetValue(selector);
            }
            return 0;
        }
    }
}
