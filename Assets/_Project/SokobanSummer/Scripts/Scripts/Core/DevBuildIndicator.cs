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
        public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f); // transparent grey
        public Vector2 padding = new Vector2(10, 6);

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
            var content = new GUIContent(text);
            var size = _style.CalcSize(content);
            float w = size.x + padding.x * 2f;
            float h = size.y + padding.y * 2f;
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
            var textRect = new Rect(x + padding.x, y + padding.y, size.x, size.y);
            var oldColor = GUI.color;
            GUI.color = backgroundColor;
            GUI.Box(rect, GUIContent.none);
            GUI.color = oldColor;
            GUI.Label(textRect, content, _style);
        }
    }
}
#endif
