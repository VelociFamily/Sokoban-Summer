using System.Collections;
using UnityEngine;
using Core;

namespace UI
{
    /// <summary>
    /// Binds a canvas to the primary camera via UIService, eliminating runtime Camera.main lookups.
    /// Attach this to any Canvas GameObject that needs automatic camera binding.
    /// UIService must be initialized before this component starts (handled by GameInitializer).
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class CanvasCameraBinder : MonoBehaviour
    {
        [Tooltip("Maximum time in seconds to wait for UIService before giving up")]
        [SerializeField] private float timeoutSeconds = 10f;

        private Canvas _canvas;
        private UIService _uiService;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            StartCoroutine(WaitForUIServiceAndBind());
        }

        /// <summary>
        /// Wait for UIService to be registered in ServiceLocator, then bind canvas.
        /// Uses a coroutine to avoid blocking the main thread during initialization.
        /// </summary>
        private IEnumerator WaitForUIServiceAndBind()
        {
            var startTime = Time.realtimeSinceStartup;

            while (Time.realtimeSinceStartup - startTime < timeoutSeconds)
            {
                if (ServiceLocator.TryGet(out _uiService))
                {
                    // UIService found - bind canvas
                    _uiService.BindCanvas(_canvas);
                    Debug.Log($"[CanvasCameraBinder]: Successfully bound canvas '{_canvas.name}' to UIService");
                    yield break;
                }

                // Wait one frame before retrying
                yield return null;
            }

            // Timeout reached
            Debug.LogError($"[CanvasCameraBinder]: Timed out waiting for UIService after {timeoutSeconds}s. Canvas '{_canvas.name}' cannot be bound to camera. Ensure GameInitializer has initialized UIService.");
        }

        private void OnDestroy()
        {
            // Unregister this canvas when destroyed
            if (_uiService != null && _canvas != null)
            {
                _uiService.UnbindCanvas(_canvas);
            }
        }
    }
}
