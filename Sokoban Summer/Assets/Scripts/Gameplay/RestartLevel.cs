using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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
                Debug.LogWarning("RestartLevel: Main camera not found, using first available camera");
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
        Vector2 mousePosition = inputActions.UI.Point.ReadValue<Vector2>();
        Vector2 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        var hit = Physics2D.Raycast(worldPosition, Vector2.zero);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            Debug.Log("RestartLevel: Restart button clicked via InputSystem");
            Restart();
        }
    }

    public void Restart()
    {
        Debug.Log("RestartLevel: Restarting current level");
        // Reload current active scene
        var currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}