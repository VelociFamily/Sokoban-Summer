using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class Startgame : MonoBehaviour
{
    [Tooltip("List of possible active objects that can be dismissed by pressing B or Escape")]
    public List<GameObject> startObjects;

    [Tooltip("The main menu object to show when B or Escape is pressed")]
    public GameObject menu;

    private InputSystem_Actions _inputActions;

    private void Awake()
    {
        if (_inputActions == null)
            _inputActions = new InputSystem_Actions();

        _inputActions.UI.Cancel.performed += OnCancelPerformed;
    }

    private void OnEnable()
    {
        if (_inputActions == null)
            _inputActions = new InputSystem_Actions();

        _inputActions.UI.Enable();
    }

    private void OnDisable()
    {
        _inputActions?.UI.Disable();
    }

    private void OnDestroy()
    {
        if (_inputActions == null) return;
        _inputActions.UI.Cancel.performed -= OnCancelPerformed;
        _inputActions.UI.Disable();
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        var anyActive = false;

        foreach (var obj in startObjects.Where(o => o != null && o.activeSelf))
        {
            obj.SetActive(false);
            anyActive = true;
        }

        if (anyActive && menu != null) menu.SetActive(true);
    }

    public void LoadNextScene()
    {
        foreach (var obj in startObjects.Where(obj => obj != null)) obj.SetActive(false);
        if (menu != null)
            menu.SetActive(true);
    }
}