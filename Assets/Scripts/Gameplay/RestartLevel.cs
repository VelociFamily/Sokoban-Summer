using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Gameplay
{
    public class RestartLevel : MonoBehaviour
    {
        [Tooltip("Optional: Restart on click (UI or 3D object with collider)")]
        public bool restartOnClick = true;

        private InputSystem_Actions inputActions;
        private Camera mainCamera;

        private void Awake()
        {
            if (restartOnClick)
            {
                inputActions = new InputSystem_Actions();
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    mainCamera = FindFirstObjectByType<Camera>();
                    Debug.LogWarning("[RestartLevel]: Main camera not tagged - using first available camera as fallback");
                }
            }
        }

        private void OnEnable()
        {
            if (restartOnClick && inputActions != null)
            {
                inputActions.UI.Enable();
                inputActions.UI.Click.performed += OnClickPerformed;
            }
        }

        private void OnDisable()
        {
            if (inputActions != null)
            {
                inputActions.UI.Click.performed -= OnClickPerformed;
                inputActions.UI.Disable();
            }
        }

        private void OnDestroy()
        {
            if (inputActions != null)
            {
                inputActions.UI.Click.performed -= OnClickPerformed;
                inputActions.UI.Disable();
                inputActions?.Dispose();
            }
        }

        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            if (!restartOnClick || mainCamera == null) return;

            // Get mouse position and check if this restart button was clicked
            var mousePosition = inputActions.UI.Point.ReadValue<Vector2>();
            Vector2 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            var hit = Physics2D.Raycast(worldPosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                Debug.Log("[RestartLevel]: Level restart triggered by user input");
                Restart();
            }
        }

        public void Restart()
        {
            Debug.Log("[RestartLevel]: Reloading current level for new attempt");
            // Reload current active scene using SceneManager helper
            var currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name); // Use scene name instead of buildIndex for better maintainability
        }
    }
}
