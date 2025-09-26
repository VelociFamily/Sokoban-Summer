using UnityEngine;
using UnityEngine.InputSystem;
using Core;

namespace UI
{
    public class ArrowClick : MonoBehaviour
    {
        public HatSelectionManager manager; // Drag your HatSelectionManager here
        public bool isRightArrow;

        private Camera mainCamera;
        private bool isUsingInputService = false;

        private void Awake()
        {
            // Try to use InputService if available, otherwise fall back to direct InputSystem usage
            if (InputService.Instance != null)
            {
                isUsingInputService = true;
            }
            
            // Improved camera finding with fallback
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                // Use FindAnyObjectByType instead of FindFirstObjectByType for better compatibility
                mainCamera = FindAnyObjectByType<Camera>();
                if (mainCamera != null)
                {
                    Debug.LogWarning("[ArrowClick]: Main camera not tagged - using first available camera as fallback");
                }
                else
                {
                    Debug.LogError("[ArrowClick]: No camera found in scene - click detection will not work");
                }
            }
        }

        private void OnEnable()
        {
            if (isUsingInputService && InputService.Instance?.InputActions?.UI != null)
            {
                InputService.Instance.InputActions.UI.Enable();
                InputService.Instance.InputActions.UI.Click.performed += OnClickPerformed;
            }
        }

        private void OnDisable()
        {
            if (isUsingInputService && InputService.Instance?.InputActions?.UI != null)
            {
                InputService.Instance.InputActions.UI.Click.performed -= OnClickPerformed;
            }
        }

        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            if (mainCamera == null) return;

            Vector2 mousePosition;
            
            if (isUsingInputService && InputService.Instance?.InputActions?.UI != null)
            {
                mousePosition = InputService.Instance.InputActions.UI.Point.ReadValue<Vector2>();
            }
            else
            {
                // Fallback - shouldn't happen if InputService is properly set up
                mousePosition = Mouse.current.position.ReadValue();
            }

            Vector2 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            var hit = Physics2D.Raycast(worldPosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                Debug.Log($"[ArrowClick]: {(isRightArrow ? "Next" : "Previous")} hat selection triggered");
            
                // This object was clicked!
                if (isRightArrow)
                {
                    manager?.NextHat();
                }
                else
                {
                    manager?.PreviousHat();
                }
            }
        }
    }
}
