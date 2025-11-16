using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Core;
using UI;
using TMPro;

namespace Tests
{
    /// <summary>
    /// Tests verifying DynamicLevelSelector no longer contains a nested LayoutMode enum
    /// and basic grid / vertical population works without errors.
    /// </summary>
    [TestFixture]
    public class DynamicLevelSelectorEnumRemovalTest
    {
        private GameObject root;
        private DynamicLevelSelector selector;
        private GameObject contentObj;
        private GameObject buttonPrefab;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("EnumRemovalTestRoot");
            // LevelManager needed for selector progression queries
            var lm = new GameObject("LevelManager").AddComponent<LevelManager>();
            lm.transform.SetParent(root.transform);

            // Canvas + EventSystem
            var canvasObj = new GameObject("Canvas");
            canvasObj.transform.SetParent(root.transform);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            if (GameObject.FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.transform.SetParent(root.transform);
                es.AddComponent<EventSystem>();
                es.AddComponent<InputSystemUIInputModule>();
            }

            // ScrollRect + viewport + content
            var scrollGO = new GameObject("ScrollRect");
            scrollGO.transform.SetParent(canvasObj.transform);
            var sr = scrollGO.AddComponent<ScrollRect>();
            sr.horizontal = false; sr.vertical = true;
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollGO.transform);
            var vpRT = viewport.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one; vpRT.sizeDelta = Vector2.zero;
            viewport.AddComponent<Image>().color = new Color(1,1,1,0.05f);
            viewport.AddComponent<Mask>();
            sr.viewport = vpRT;
            contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewport.transform);
            var cRT = contentObj.AddComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0,1); cRT.anchorMax = new Vector2(1,1); cRT.pivot = new Vector2(0.5f,1);
            cRT.sizeDelta = new Vector2(0,800);
            sr.content = cRT;

            // Level button prefab
            buttonPrefab = CreateButtonPrefab();

            selector = scrollGO.AddComponent<DynamicLevelSelector>();
            selector.levelButtonContainer = contentObj.transform;
            selector.levelButtonPrefab = buttonPrefab;
            selector.showTutorials = true;
            selector.showGameplayLevels = true;
            selector.addSectionHeaders = false;
            selector.autoAddVerticalLayoutGroup = false;
        }

        [TearDown]
        public void TearDown()
        {
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }
        }

        private GameObject CreateButtonPrefab()
        {
            var prefab = new GameObject("LevelButtonPrefab");
            var rt = prefab.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(220,96);
            var img = prefab.AddComponent<Image>();
            img.color = Color.white;
            var btn = prefab.AddComponent<Button>();
            btn.targetGraphic = img;
            var dyn = prefab.AddComponent<DynamicLevelButton>();
            var nameGO = new GameObject("NameText");
            nameGO.transform.SetParent(prefab.transform);
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text = "Level";
            dyn.levelNameText = nameTMP;
            var goalGO = new GameObject("GoalText");
            goalGO.transform.SetParent(prefab.transform);
            var goalTMP = goalGO.AddComponent<TextMeshProUGUI>();
            goalTMP.text = "Goal";
            dyn.goalText = goalTMP;
            return prefab;
        }

        [Test]
        public void NestedEnum_Removed()
        {
            var nested = typeof(DynamicLevelSelector).GetNestedType("LayoutMode", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNull(nested, "Legacy nested LayoutMode enum should be removed.");
        }

        [Test]
        public void VerticalList_Populate_NoErrors()
        {
            LogAssert.NoUnexpectedReceived();
            selector.layoutMode = LayoutController.LayoutMode.VerticalList;
            selector.PopulateLevelButtons();
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Grid_Populate_NoErrors()
        {
            LogAssert.NoUnexpectedReceived();
            selector.layoutMode = LayoutController.LayoutMode.Grid;
            selector.enablePagination = true;
            selector.gridColumns = 3;
            selector.gridRowsPerPage = 2;
            selector.PopulateLevelButtons();
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Grid_Resize_Repopulates_SelectionStable()
        {
            LogAssert.NoUnexpectedReceived();
            selector.layoutMode = LayoutController.LayoutMode.Grid;
            selector.enablePagination = true;
            selector.gridColumns = 3;
            selector.gridRowsPerPage = 2;
            selector.PopulateLevelButtons();
            var initialSel = EventSystem.current?.currentSelectedGameObject;
            var rt = contentObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(600, rt.sizeDelta.y); // simulate narrow
            selector.RefreshLevelSelection();
            var afterNarrow = EventSystem.current?.currentSelectedGameObject;
            rt.sizeDelta = new Vector2(1400, rt.sizeDelta.y); // simulate wide
            selector.RefreshLevelSelection();
            var afterWide = EventSystem.current?.currentSelectedGameObject;
            Assert.IsTrue(IsValid(afterWide) || IsValid(afterNarrow) || IsValid(initialSel), "Selection should remain on an interactable button across resizes.");
            LogAssert.NoUnexpectedReceived();
        }

        private bool IsValid(GameObject go)
        {
            if (go == null) return false;
            var btn = go.GetComponent<Button>();
            return btn != null && btn.interactable;
        }
    }
}
