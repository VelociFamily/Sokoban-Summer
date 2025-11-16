using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Tracks UI dirty callbacks (layout, vertices, material) under a canvas to approximate rebuild cause frequency.
    /// Attach to StaticHUD and DynamicHUD canvases to compare activity. Logs summary every logInterval seconds when changes occur.
    /// </summary>
    public class CanvasRebuildProfiler : MonoBehaviour
    {
        [Tooltip("Seconds between summary logs when there is activity")] public float logInterval = 5f;
        [Tooltip("Enable automatic logging; otherwise query counters manually")] public bool autoLog = true;

        private readonly List<Graphic> trackedGraphics = new List<Graphic>();
        private int layoutDirtyCount;
        private int verticesDirtyCount;
        private int materialDirtyCount;
        private float lastLogTime;
        private Canvas canvas;

        public int LayoutDirtyCount => layoutDirtyCount;
        public int VerticesDirtyCount => verticesDirtyCount;
        public int MaterialDirtyCount => materialDirtyCount;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            RegisterGraphics();
        }

        private void RegisterGraphics()
        {
            trackedGraphics.Clear();
            if (canvas == null) return;
            var graphics = canvas.GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics)
            {
                if (g == null) continue;
                trackedGraphics.Add(g);
                g.RegisterDirtyLayoutCallback(OnLayoutDirty);
                g.RegisterDirtyVerticesCallback(OnVerticesDirty);
                g.RegisterDirtyMaterialCallback(OnMaterialDirty);
            }
        }

        private void OnLayoutDirty() => layoutDirtyCount++;
        private void OnVerticesDirty() => verticesDirtyCount++;
        private void OnMaterialDirty() => materialDirtyCount++;

        private void LateUpdate()
        {
            if (!autoLog) return;
            if (Time.unscaledTime - lastLogTime >= logInterval && (layoutDirtyCount + verticesDirtyCount + materialDirtyCount) > 0)
            {
                Debug.Log($"[CanvasRebuildProfiler] Canvas '{name}' activity in last {logInterval:F1}s => Layout:{layoutDirtyCount} Vertices:{verticesDirtyCount} Material:{materialDirtyCount}");
                layoutDirtyCount = 0; verticesDirtyCount = 0; materialDirtyCount = 0; lastLogTime = Time.unscaledTime;
            }
        }

        /// <summary>
        /// Manual snapshot log without resetting counters.
        /// </summary>
        [ContextMenu("Log Snapshot")] public void LogSnapshot()
        {
            Debug.Log($"[CanvasRebuildProfiler] Snapshot '{name}' => Layout:{layoutDirtyCount} Vertices:{verticesDirtyCount} Material:{materialDirtyCount}");
        }
    }
}
