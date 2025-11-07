using UnityEngine;
using UnityEngine.EventSystems;
using UI;

namespace Testing
{
    /// <summary>
    /// Auto runner for DynamicLevelSelectorPaginationTest. Attach to a GameObject in the test scene.
    /// Ensures a DynamicLevelSelector exists and invokes the pagination fallback test on Start.
    /// </summary>
    public class DynamicLevelSelectorAutoTestRunner : MonoBehaviour
    {
        [Tooltip("Optional explicit reference to pagination test component")] public DynamicLevelSelectorPaginationTest paginationTest;
        [Tooltip("Populate buttons on Start before running the test")] public bool populateBeforeRun = true;
        [Tooltip("Run test automatically on Start")] public bool runOnStart = true;

        private void Awake()
        {
            if (paginationTest == null)
            {
                paginationTest = FindFirstObjectByType<DynamicLevelSelectorPaginationTest>();
            }
        }

        private void Start()
        {
            if (!runOnStart || paginationTest == null) return;

            var selector = paginationTest.selector ?? FindFirstObjectByType<DynamicLevelSelector>();
            if (selector != null && populateBeforeRun)
            {
                selector.PopulateLevelButtons();
            }

            // Ensure EventSystem exists
            if (EventSystem.current == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            paginationTest.RunPaginationSelectionFallbackTest();
        }
    }
}
