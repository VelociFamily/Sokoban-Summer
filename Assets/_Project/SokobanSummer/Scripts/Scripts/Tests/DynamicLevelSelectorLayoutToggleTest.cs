using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using System.Collections;
using Core;
using UI;

namespace Tests
{
    /// <summary>
    /// Edit-mode and play-mode tests for DynamicLevelSelector layout toggle stability.
    /// Validates that toggling between Grid and VerticalList layout modes produces no errors/warnings.
    /// </summary>
    [TestFixture]
    public class DynamicLevelSelectorLayoutToggleTest
    {
        private GameObject testRoot;
        private GameObject levelManagerObj;
        private GameObject canvasObj;
        private GameObject selectorObj;
        private DynamicLevelSelector selector;

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
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Create EventSystem if not present
            if (GameObject.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.transform.SetParent(testRoot.transform);
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Create ScrollRect with container
            var scrollRectObj = new GameObject("ScrollRect");
            scrollRectObj.transform.SetParent(canvasObj.transform);
            var rt = scrollRectObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            
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
            contentRT.sizeDelta = new Vector2(0, 1000);
            scrollRect.content = contentRT;

            // Create level button prefab
            var buttonPrefab = CreateLevelButtonPrefab();

            // Create and configure DynamicLevelSelector
            selectorObj = scrollRectObj;
            selector = selectorObj.AddComponent<DynamicLevelSelector>();
            selector.levelButtonContainer = contentObj.transform;
            selector.levelButtonPrefab = buttonPrefab;
            selector.showTutorials = true;
            selector.showGameplayLevels = true;
            selector.addSectionHeaders = false; // Keep simple for testing
            selector.layoutMode = DynamicLevelSelector.LayoutMode.VerticalList;
            selector.autoAddVerticalLayoutGroup = true;
            selector.buttonHeight = 96f;
            selector.buttonSpacing = 8f;
            selector.enablePagination = false; // Start without pagination
        }

        [TearDown]
        public void TearDown()
        {
            if (testRoot != null)
            {
                Object.DestroyImmediate(testRoot);
            }
        }

        private GameObject CreateLevelButtonPrefab()
        {
            var prefab = new GameObject("LevelButtonPrefab");
            
            var buttonRT = prefab.AddComponent<RectTransform>();
            buttonRT.sizeDelta = new Vector2(200, 96);
            
            var button = prefab.AddComponent<Button>();
            var image = prefab.AddComponent<Image>();
            image.color = Color.white;
            button.targetGraphic = image;

            // Add DynamicLevelButton component
            var dynamicButton = prefab.AddComponent<DynamicLevelButton>();

            // Add required child elements
            var nameTextObj = new GameObject("NameText");
            nameTextObj.transform.SetParent(prefab.transform);
            var nameText = nameTextObj.AddComponent<Text>();
            nameText.text = "Level Name";
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.alignment = TextAnchor.MiddleCenter;

            var goalTextObj = new GameObject("GoalText");
            goalTextObj.transform.SetParent(prefab.transform);
            var goalText = goalTextObj.AddComponent<Text>();
            goalText.text = "Goals";
            goalText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            goalText.alignment = TextAnchor.MiddleCenter;

            // Assign to DynamicLevelButton
            dynamicButton.levelNameText = nameText;
            dynamicButton.goalText = goalText;

            return prefab;
        }

        [Test]
        public void LayoutToggle_VerticalToGrid_NoErrors()
        {
            LogAssert.NoUnexpectedReceived();
            
            // Start in VerticalList mode (default from SetUp)
            Assert.AreEqual(DynamicLevelSelector.LayoutMode.VerticalList, selector.layoutMode);

            // Toggle to Grid mode
            selector.layoutMode = DynamicLevelSelector.LayoutMode.Grid;
            selector.enablePagination = true;
            selector.gridColumns = 3;
            selector.gridRowsPerPage = 2;
            
            // This should not produce errors
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void LayoutToggle_GridToVertical_NoErrors()
        {
            LogAssert.NoUnexpectedReceived();

            // Start in Grid mode
            selector.layoutMode = DynamicLevelSelector.LayoutMode.Grid;
            selector.enablePagination = true;
            selector.gridColumns = 3;
            
            // Toggle to VerticalList mode
            selector.layoutMode = DynamicLevelSelector.LayoutMode.VerticalList;
            selector.autoAddVerticalLayoutGroup = true;
            
            // This should not produce errors
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void LayoutToggle_MultipleToggles_NoErrors()
        {
            LogAssert.NoUnexpectedReceived();

            // Perform multiple toggles
            for (int i = 0; i < 5; i++)
            {
                selector.layoutMode = DynamicLevelSelector.LayoutMode.Grid;
                selector.layoutMode = DynamicLevelSelector.LayoutMode.VerticalList;
            }
            
            // This should not produce errors
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator LayoutToggle_WithPopulation_NoErrors()
        {
            LogAssert.NoUnexpectedReceived();

            // Wait for LevelManager to initialize
            yield return null;

            // Populate in VerticalList mode
            selector.layoutMode = DynamicLevelSelector.LayoutMode.VerticalList;
            selector.PopulateLevelButtons();
            
            yield return null;

            // Toggle to Grid and repopulate
            selector.layoutMode = DynamicLevelSelector.LayoutMode.Grid;
            selector.enablePagination = true;
            selector.gridColumns = 3;
            selector.PopulateLevelButtons();
            
            yield return null;

            // Toggle back to VerticalList and repopulate
            selector.layoutMode = DynamicLevelSelector.LayoutMode.VerticalList;
            selector.PopulateLevelButtons();
            
            yield return null;

            // This should not produce errors
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator GridMode_PaginationEnabled_NoErrors()
        {
            LogAssert.NoUnexpectedReceived();

            // Configure Grid mode with pagination
            selector.layoutMode = DynamicLevelSelector.LayoutMode.Grid;
            selector.enablePagination = true;
            selector.gridColumns = 3;
            selector.gridRowsPerPage = 2;
            selector.gridCellHeight = 180f;
            
            yield return null;

            selector.PopulateLevelButtons();
            
            yield return null;

            // This should not produce errors
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator VerticalMode_WithHeaders_NoErrors()
        {
            LogAssert.NoUnexpectedReceived();

            // Configure VerticalList mode with section headers
            selector.layoutMode = DynamicLevelSelector.LayoutMode.VerticalList;
            selector.addSectionHeaders = true;
            
            // Create a simple section header prefab
            var headerPrefab = new GameObject("HeaderPrefab");
            var headerText = headerPrefab.AddComponent<Text>();
            headerText.text = "Section";
            headerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            selector.sectionHeaderPrefab = headerPrefab;
            
            yield return null;

            selector.PopulateLevelButtons();
            
            yield return null;

            // Cleanup
            Object.DestroyImmediate(headerPrefab);

            // This should not produce errors
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void LayoutToggle_WithResponsiveColumns_NoErrors()
        {
            LogAssert.NoUnexpectedReceived();

            // Configure Grid mode with responsive columns
            selector.layoutMode = DynamicLevelSelector.LayoutMode.Grid;
            selector.enableResponsiveColumns = true;
            selector.responsiveGridCellWidth = true;
            selector.oneColumnMaxWidth = 680f;
            selector.twoColumnMaxWidth = 1080f;
            
            // This should not produce errors
            LogAssert.NoUnexpectedReceived();

            // Toggle back to vertical
            selector.layoutMode = DynamicLevelSelector.LayoutMode.VerticalList;
            
            // This should not produce errors
            LogAssert.NoUnexpectedReceived();
        }
    }
}
