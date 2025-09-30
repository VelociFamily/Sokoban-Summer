using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Cainos.Pixel_Art_Top_Down___Basic.Script
{
    public class MenuFirstSelect : MonoBehaviour
    {
        public GameObject firstButton; // Assign in Inspector
        public InputSystem_Actions InputSystemActions;


        private void Awake()
        {
            InputSystemActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            StartCoroutine(SelectFirst());
            InputSystemActions.UI.Enable();

            InputSystemActions.UI.Submit.performed += SelectTheFirstOne;
            InputSystemActions.UI.Navigate.performed += OnNavigatePerformed;
        }

        private System.Collections.IEnumerator SelectFirst()
        {
            yield return null;

            if (EventSystem.current != null && firstButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstButton);
            }
            else
            {
                if (EventSystem.current == null)
                    Debug.LogWarning("No EventSystem found in the scene.");
                if (firstButton == null)
                    Debug.LogWarning("No firstButton assigned to MenuFirstSelect.");
            }
        }

        private void SelectTheFirstOne(InputAction.CallbackContext context)
        {
            if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject != null) return;
            EventSystem.current.SetSelectedGameObject(firstButton);
        }

        private void OnNavigatePerformed(InputAction.CallbackContext context)
        {
            // When the left stick is moved and no button is selected, reselect the first button
            if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject != null) return;
            EventSystem.current.SetSelectedGameObject(firstButton);
        }

        private void OnDisable()
        {
            if (InputSystemActions == null) return;
            InputSystemActions.UI.Submit.performed -= SelectTheFirstOne;
            InputSystemActions.UI.Navigate.performed -= OnNavigatePerformed;
            InputSystemActions.UI.Disable();
        }
    }
}