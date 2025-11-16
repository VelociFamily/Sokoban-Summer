using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Ensures gameplay HUD has two separate canvases: StaticHUD (unchanging visuals) and DynamicHUD (frequently updated counters).
    /// Also disables Raycast Target on non-interactive graphics to reduce event system overhead.
    /// Attach this to a root GameObject in gameplay scenes (or add via prefab in bootstrap scene).
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class HudStructureInitializer : MonoBehaviour
    {
        [Header("Canvas Names")] public string staticHudName = "StaticHUD"; public string dynamicHudName = "DynamicHUD";
        [Header("Auto Create")] public bool createIfMissing = true;
        [Header("Raycast Target Optimization")] public bool disableRaycastOnNonInteractive = true;

        public Canvas StaticHudCanvas { get; private set; }
        public Canvas DynamicHudCanvas { get; private set; }

        private void Awake()
        {
            EnsureHudCanvases();
            if (disableRaycastOnNonInteractive)
            {
                OptimizeRaycastTargets(StaticHudCanvas);
                OptimizeRaycastTargets(DynamicHudCanvas);
            }
        }

        private void EnsureHudCanvases()
        {
            // Look for existing canvases by name first
            StaticHudCanvas = FindCanvasByName(staticHudName);
            DynamicHudCanvas = FindCanvasByName(dynamicHudName);

            if (!createIfMissing) return;

            if (StaticHudCanvas == null)
            {
                StaticHudCanvas = CreateHudCanvas(staticHudName, 100);
            }
            if (DynamicHudCanvas == null)
            {
                DynamicHudCanvas = CreateHudCanvas(dynamicHudName, 101);
            }
        }

        private Canvas FindCanvasByName(string name)
        {
            return Resources.FindObjectsOfTypeAll<Canvas>().FirstOrDefault(c => c != null && c.gameObject.scene.IsValid() && c.name == name);
        }

        private Canvas CreateHudCanvas(string name, int sortOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private void OptimizeRaycastTargets(Canvas canvas)
        {
            if (canvas == null) return;
            var graphics = canvas.GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics)
            {
                if (g == null) continue;
                // Skip if part of an interactive control (Selectable or has a Button/Toggle/etc.)
                bool hasSelectable = g.GetComponent<Selectable>() != null || g.GetComponentInParent<Selectable>() != null;
                if (hasSelectable) continue;
                // Skip if explicitly overridden (add a marker component if needed in future)
                g.raycastTarget = false;
            }
            // Also TMP specific (inherits Graphic so already handled) left intentionally.
        }
    }
}
