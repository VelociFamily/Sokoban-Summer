using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowClick : MonoBehaviour
{
    public HatSelectionManager manager; // Drag your HatSelectionManager here
    public bool isRightArrow;

    private InputSystem_Actions inputActions;
    private Camera mainCamera;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
            Debug.LogWarning("[ArrowClick]: Main camera not tagged - using first available camera as fallback");
        }
    }

    private void OnEnable()
    {
        inputActions.UI.Enable();
        inputActions.UI.Click.performed += OnClickPerformed;
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
        if (mainCamera == null) return;

        var mousePosition = inputActions.UI.Point.ReadValue<Vector2>();
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
