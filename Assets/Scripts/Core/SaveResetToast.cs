#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Collections;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Displays a small on-screen toast when saves are reset via SaveFacade.
    /// Attach to any persistent GameObject (e.g., GameInitializer).
    /// </summary>
    public class SaveResetToast : MonoBehaviour
    {
        public float duration = 3f;
        public string message = "";
        public bool visible;
        private Coroutine _hideRoutine;

        private void OnEnable()
        {
            SaveFacade.OnReset += HandleReset;
        }

        private void OnDisable()
        {
            SaveFacade.OnReset -= HandleReset;
        }

        private void HandleReset(string scope)
        {
            message = scope == "All" ? "All saves reset" : $"{scope} reset";
            visible = true;
            if (_hideRoutine != null) StopCoroutine(_hideRoutine);
            _hideRoutine = StartCoroutine(HideLater());
        }

        private IEnumerator HideLater()
        {
            yield return new WaitForSeconds(duration);
            visible = false;
            _hideRoutine = null;
        }

        private void OnGUI()
        {
            if (!visible) return;
            var w = 260; var h = 40;
            var x = (Screen.width - w) / 2;
            var y = 20;
            var rect = new Rect(x, y, w, h);
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(rect, message, new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
        }
    }
}
#endif
