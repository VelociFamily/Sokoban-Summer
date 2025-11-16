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
        private Canvas _canvas;
        private UIService _uiService;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            // Get UIService from ServiceLocator
            if (!ServiceLocator.TryGet(out _uiService))
            {
                Debug.LogError($"[CanvasCameraBinder]: UIService not found in ServiceLocator. Canvas '{_canvas.name}' cannot be bound to camera. Ensure GameInitializer has initialized UIService.");
                return;
            }

            // Register this canvas with UIService for automatic camera binding
            _uiService.BindCanvas(_canvas);
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
