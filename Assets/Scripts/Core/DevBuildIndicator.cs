#if DEVELOPMENT_BUILD || UNITY_EDITOR
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Simple on-screen indicator for development/editor builds.
    /// </summary>
    public class DevBuildIndicator : MonoBehaviour
    {
        public string prefixText = "Development Build";
        public Color textColor = new Color(1f, 0.9f, 0.2f, 1f);
        public int fontSize = 14;
        public Vector2 margin = new Vector2(10, 10);
        public TextAnchor anchor = TextAnchor.UpperLeft;

        private GUIStyle _style;

        private void EnsureStyle()
        {
            if (_style != null) return;
            _style = new GUIStyle(GUI.skin.label)
            {
                alignment = anchor,
                fontStyle = FontStyle.Bold,
                fontSize = fontSize,
                normal = { textColor = textColor }
            };
        }

        private void OnGUI()
        {
            EnsureStyle();
            var text = string.IsNullOrEmpty(prefixText)
                ? $"{Application.productName} {Application.version}"
                : $"{prefixText} — {Application.productName} {Application.version}";

            float w = 340f; float h = 24f;
            float x = margin.x; float y = margin.y;

            switch (anchor)
            {
                case TextAnchor.UpperCenter: x = (Screen.width - w) / 2f; y = margin.y; break;
                case TextAnchor.UpperRight: x = Screen.width - w - margin.x; y = margin.y; break;
                case TextAnchor.LowerLeft: x = margin.x; y = Screen.height - h - margin.y; break;
                case TextAnchor.LowerCenter: x = (Screen.width - w) / 2f; y = Screen.height - h - margin.y; break;
                case TextAnchor.LowerRight: x = Screen.width - w - margin.x; y = Screen.height - h - margin.y; break;
                default: /* UpperLeft */ break;
            }

            var rect = new Rect(x, y, w, h);
            var oldColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.5f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = oldColor;
            GUI.Label(rect, text, _style);
        }
    }
}
#endif
