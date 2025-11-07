using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Core;
using UI;

namespace Testing
{
    /// <summary>
    /// Runtime test helper for DynamicLevelSelector pagination & selection fallback.
    /// Attach in a test scene containing a GameObject with DynamicLevelSelector and Grid layout setup.
    /// Provides a context menu to simulate a resize changing column count and asserts selection behavior.
    /// </summary>
    public class DynamicLevelSelectorPaginationTest : MonoBehaviour
    {
        [Tooltip("Reference to the DynamicLevelSelector under test")] public DynamicLevelSelector selector;
        [Tooltip("Simulated container width for narrow layout (e.g., 1 column)")] public float narrowWidth = 600f;
        [Tooltip("Simulated container width for medium layout (e.g., 2 columns)")] public float mediumWidth = 900f;
        [Tooltip("Simulated container width for wide layout (>= configured columns)")] public float wideWidth = 1400f;
        [Tooltip("Enable verbose logging")] public bool verboseLogging = true;

        private RectTransform _contentRT;

        private void Awake()
        {
            if (selector == null)
            {
                selector = FindFirstObjectByType<DynamicLevelSelector>();
            }
            if (selector != null && selector.levelButtonContainer != null)
            {
                _contentRT = selector.levelButtonContainer.GetComponent<RectTransform>();
            }
        }

        [ContextMenu("Run Pagination Selection Fallback Test")]
        public void RunPaginationSelectionFallbackTest()
        {
            if (selector == null || _contentRT == null)
            {
                Debug.LogError("[DynamicLevelSelectorPaginationTest] Missing selector or content RectTransform.");
                return;
            }

            if (selector.layoutMode != DynamicLevelSelector.LayoutMode.Grid)
            {
                Debug.LogWarning("[DynamicLevelSelectorPaginationTest] Selector not in Grid mode; switching.");
                selector.layoutMode = DynamicLevelSelector.LayoutMode.Grid;
                selector.PopulateLevelButtons();
            }

            // Step 1: Ensure initial population
            selector.PopulateLevelButtons();
            var initialSelected = EventSystem.current?.currentSelectedGameObject;
            Log($"Initial selected: {initialSelected?.name ?? "<none>"}");

            // Step 2: Simulate narrow resize (force 1 column if responsive enabled)
            SimulateResize(narrowWidth);
            var afterNarrow = EventSystem.current?.currentSelectedGameObject;
            Log($"After narrow resize selected: {afterNarrow?.name ?? "<none>"}");
            bool narrowValid = IsValidInteractableSelection(afterNarrow);

            // Step 3: Simulate medium resize (2 columns)
            SimulateResize(mediumWidth);
            var afterMedium = EventSystem.current?.currentSelectedGameObject;
            Log($"After medium resize selected: {afterMedium?.name ?? "<none>"}");
            bool mediumValid = IsValidInteractableSelection(afterMedium);

            // Step 4: Simulate wide resize (restore full columns)
            SimulateResize(wideWidth);
            var afterWide = EventSystem.current?.currentSelectedGameObject;
            Log($"After wide resize selected: {afterWide?.name ?? "<none>"}");
            bool wideValid = IsValidInteractableSelection(afterWide);

            // Summary
            Debug.Log("=== DynamicLevelSelector Pagination Fallback Test Results ===");
            Debug.Log($"Narrow resize selection valid: {narrowValid}");
            Debug.Log($"Medium resize selection valid: {mediumValid}");
            Debug.Log($"Wide resize selection valid: {wideValid}");
            bool allPassed = narrowValid && mediumValid && wideValid;
            Debug.Log($"Overall: {(allPassed ? "PASS" : "FAIL")}");
        }

        private void SimulateResize(float targetWidth)
        {
            if (_contentRT == null) return;
            var size = _contentRT.sizeDelta;
            size.x = targetWidth; // mimic width change; OnRectTransformDimensionsChange will fire
            _contentRT.sizeDelta = size;
            // Force a layout update cycle
            selector.RefreshLevelSelection();
        }

        private bool IsValidInteractableSelection(GameObject selected)
        {
            if (selected == null) return false;
            var btn = selected.GetComponent<Button>();
            return btn != null && btn.interactable;
        }

        private void Log(string msg)
        {
            if (verboseLogging) Debug.Log(msg);
        }
    }
}
