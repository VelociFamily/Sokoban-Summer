#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.IO;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Lightweight in-game debug UI to reset saves and inspect state.
    /// Attach to any always-on object (e.g., GameInitializer) or a small empty GameObject in Game scene.
    /// Toggle with F10 by default.
    /// </summary>
    public class SaveDebugMenu : MonoBehaviour
    {
        [Header("Hotkey")]
        public KeyCode toggleKey = KeyCode.F10;

        [Header("UI")]
        public bool visible;
        public Rect windowRect = new Rect(20, 20, 380, 320);

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                visible = !visible;
            }
        }

        private void OnGUI()
        {
            if (!visible) return;

            windowRect = GUI.ModalWindow(0xED51, windowRect, DrawWindow, "Save Debug Menu");
        }

        private void DrawWindow(int id)
        {
            var savesPath = Path.Combine(Application.persistentDataPath, "Saves");
            GUILayout.Label($"persistentDataPath: {Application.persistentDataPath}");
            GUILayout.Label("Saves folder: " + savesPath);
            if (GUILayout.Button("Show Saves Folder"))
            {
                var uri = new System.Uri(savesPath);
                Application.OpenURL(uri.AbsoluteUri);
            }

            GUILayout.Space(8);
            if (GUILayout.Button("Reset Settings"))
            {
                SaveFacade.Instance.ResetSettings();
                Debug.Log("[SaveDebugMenu] Settings reset");
            }
            if (GUILayout.Button("Reset Achievements"))
            {
                SaveFacade.Instance.ResetAchievements();
                Debug.Log("[SaveDebugMenu] Achievements reset");
            }
            if (GUILayout.Button("Reset Progress"))
            {
                SaveFacade.Instance.ResetProgress();
                Debug.Log("[SaveDebugMenu] Progress reset");
            }
            if (GUILayout.Button("Reset ALL (incl. migration flag)"))
            {
                SaveFacade.Instance.ResetAll();
                Debug.Log("[SaveDebugMenu] All saves reset");
            }

            GUILayout.Space(8);
            if (GUILayout.Button("Close")) visible = false;

            GUI.DragWindow(new Rect(0,0,10000,20));
        }
    }
}
#endif
