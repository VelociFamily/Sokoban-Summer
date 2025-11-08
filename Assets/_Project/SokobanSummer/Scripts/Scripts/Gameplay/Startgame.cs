using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay
{
    public class Startgame : MonoBehaviour
    {
        [Tooltip("List of possible active objects that can be dismissed by pressing B or Escape")]
        public List<GameObject> startObjects;

        [Tooltip("The main menu object to show when B or Escape is pressed")]
        public GameObject menu;

        private InputSystem_Actions _inputActions;

        private void Awake()
        {
            // Use InputService instead of creating our own InputSystem_Actions
            InitializeInput();
        }

        private void InitializeInput()
        {
            // Use centralized input service
            var inputService = ServiceLocator.Get<InputService>();
            if (inputService.InputActions != null)
            {
                _inputActions = inputService.InputActions;
                _inputActions.UI.Cancel.performed += OnCancelPerformed;
            }
        }

        private void OnEnable()
        {
            var inputService = ServiceLocator.Get<InputService>();
            inputService?.EnableUIInput();
        }

        private void OnDisable()
        {
            var inputService = ServiceLocator.Get<InputService>();
            inputService?.DisableUIInput();
        }

        private void OnDestroy()
        {
            if (_inputActions != null)
            {
                _inputActions.UI.Cancel.performed -= OnCancelPerformed;
            }
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
}
