using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Centralized UI service to manage canvas camera assignments and UI-related operations.
    /// Eliminates runtime Camera.main lookups and ensures deterministic canvas rendering from the first frame.
    /// Access via ServiceLocator.Get&lt;UIService&gt;()
    /// </summary>
    public class UIService
    {
        /// <summary>
        /// The primary camera used for canvas rendering. Automatically resolved during initialization.
        /// </summary>
        public Camera PrimaryCamera { get; private set; }

        /// <summary>
        /// Event fired when the primary camera is assigned or changed.
        /// Subscribe to this to receive notifications when camera binding occurs.
        /// </summary>
        public event Action<Camera> OnPrimaryCameraChanged;

        private readonly List<Canvas> _trackedCanvases = new List<Canvas>();
        private bool _isInitialized;

        public UIService() { }

        /// <summary>
        /// Initialize the UIService asynchronously and resolve the primary camera.
        /// </summary>
        /// <param name="timeoutSeconds">Maximum time to wait for a camera to become available.</param>
        public async Task InitializeAsync(float timeoutSeconds = 5f)
        {
            Debug.Log("[UIService]: Initializing UI systems...");

            await ResolvePrimaryCameraAsync(timeoutSeconds);

            _isInitialized = true;
            Debug.Log("[UIService]: UI systems initialized successfully");
        }

        /// <summary>
        /// Wait for a primary camera to exist and assign it.
        /// </summary>
        private async Task ResolvePrimaryCameraAsync(float timeoutSeconds)
        {
            var startTime = Time.realtimeSinceStartup;

            while (Time.realtimeSinceStartup - startTime < timeoutSeconds)
            {
                var camera = Camera.main ?? UnityEngine.Object.FindFirstObjectByType<Camera>();
                if (camera != null)
                {
                    SetPrimaryCamera(camera);
                    Debug.Log($"[UIService]: Primary camera '{camera.name}' resolved successfully");
                    return;
                }

                await Task.Yield();
            }

            Debug.LogWarning("[UIService]: Timed out waiting for a primary camera. UI canvases may not render correctly until a camera is assigned.");
        }

        /// <summary>
        /// Set or update the primary camera used for UI rendering.
        /// Automatically rebinds all tracked canvases to the new camera.
        /// </summary>
        /// <param name="camera">The camera to use for UI rendering.</param>
        public void SetPrimaryCamera(Camera camera)
        {
            if (camera == null)
            {
                Debug.LogWarning("[UIService]: Attempted to set null camera as primary. Ignoring.");
                return;
            }

            if (PrimaryCamera == camera)
            {
                return; // No change needed
            }

            PrimaryCamera = camera;
            OnPrimaryCameraChanged?.Invoke(camera);

            // Rebind all tracked canvases to the new camera
            RebindAllCanvases();

            Debug.Log($"[UIService]: Primary camera updated to '{camera.name}'");
        }

        /// <summary>
        /// Register a canvas for automatic camera binding.
        /// If the canvas is in ScreenSpaceCamera mode and has no camera assigned, binds it to the primary camera.
        /// </summary>
        /// <param name="canvas">The canvas to register and bind.</param>
        public void BindCanvas(Canvas canvas)
        {
            if (canvas == null)
            {
                Debug.LogWarning("[UIService]: Attempted to bind null canvas. Ignoring.");
                return;
            }

            if (!_trackedCanvases.Contains(canvas))
            {
                _trackedCanvases.Add(canvas);
            }

            // Bind camera if needed
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
            {
                if (PrimaryCamera != null)
                {
                    canvas.worldCamera = PrimaryCamera;
                    Debug.Log($"[UIService]: Bound canvas '{canvas.name}' to primary camera '{PrimaryCamera.name}'");
                }
                else
                {
                    Debug.LogWarning($"[UIService]: Cannot bind canvas '{canvas.name}' - no primary camera available yet");
                }
            }
        }

        /// <summary>
        /// Unregister a canvas from automatic camera binding tracking.
        /// </summary>
        /// <param name="canvas">The canvas to unregister.</param>
        public void UnbindCanvas(Canvas canvas)
        {
            if (canvas != null && _trackedCanvases.Contains(canvas))
            {
                _trackedCanvases.Remove(canvas);
                Debug.Log($"[UIService]: Unbound canvas '{canvas.name}' from tracking");
            }
        }

        /// <summary>
        /// Rebind all tracked canvases to the current primary camera.
        /// Useful when the primary camera changes.
        /// </summary>
        private void RebindAllCanvases()
        {
            if (PrimaryCamera == null)
            {
                Debug.LogWarning("[UIService]: Cannot rebind canvases - no primary camera available");
                return;
            }

            // Clean up null references (destroyed canvases)
            _trackedCanvases.RemoveAll(c => c == null);

            foreach (var canvas in _trackedCanvases)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    canvas.worldCamera = PrimaryCamera;
                    Debug.Log($"[UIService]: Rebound canvas '{canvas.name}' to primary camera '{PrimaryCamera.name}'");
                }
            }
        }

        /// <summary>
        /// Get the current primary camera. Returns null if no camera has been resolved yet.
        /// </summary>
        public Camera GetPrimaryCamera()
        {
            return PrimaryCamera;
        }

        /// <summary>
        /// Check if the UIService has been initialized and has a primary camera.
        /// </summary>
        public bool IsReady()
        {
            return _isInitialized && PrimaryCamera != null;
        }

        /// <summary>
        /// Clean up and release resources.
        /// </summary>
        public void Shutdown()
        {
            _trackedCanvases.Clear();
            PrimaryCamera = null;
            OnPrimaryCameraChanged = null;
            _isInitialized = false;
            Debug.Log("[UIService]: Shutdown complete");
        }
    }
}
