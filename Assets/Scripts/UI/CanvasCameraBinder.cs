using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasCameraBinder : MonoBehaviour
    {
        [Header("Camera Binding Options")]
        [Tooltip("Override camera to use instead of finding one automatically")]
        public Camera overrideCamera;

        void Start()
        {
            var canvas = GetComponent<Canvas>();
            
            if (canvas.renderMode != RenderMode.ScreenSpaceCamera)
            {
                // No camera needed for overlay mode
                return;
            }

            if (canvas.worldCamera != null)
            {
                // Camera already assigned
                return;
            }

            // Find appropriate camera
            Camera targetCamera = FindAppropriateCamera();
            
            if (targetCamera != null)
            {
                canvas.worldCamera = targetCamera;
                Debug.Log($"[CanvasCameraBinder]: Assigned camera '{targetCamera.name}' to canvas '{gameObject.name}'");
            }
            else
            {
                Debug.LogWarning($"[CanvasCameraBinder]: No suitable camera found for canvas '{gameObject.name}'. Consider switching to Overlay mode or ensuring a camera is present.");
            }
        }

        private Camera FindAppropriateCamera()
        {
            // 1. Use override camera if set
            if (overrideCamera != null)
                return overrideCamera;

            // 2. Try main camera first
            if (Camera.main != null)
                return Camera.main;

            // 3. Fall back to any available camera
            Camera fallbackCamera = FindAnyObjectByType<Camera>();
            if (fallbackCamera != null)
            {
                Debug.LogWarning("[CanvasCameraBinder]: Main camera not found, using fallback camera. Consider tagging the main camera.");
                return fallbackCamera;
            }

            return null;
        }
    }
}