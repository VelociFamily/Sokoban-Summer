using UnityEditor;
using UnityEngine;
using TMPro;

namespace SokobanSummer.Editor.FontTools
{
    public static class AssignBangersFont
    {
        [MenuItem("Tools/Fonts/Assign Bangers To All TextMeshPro")]
        public static void Assign()
        {
            // Load TMP Font Asset from Resources. Path is relative inside any Resources folder.
            var fontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/Bangers SDF");
            if (fontAsset == null)
            {
                Debug.LogError("Bangers SDF TMP_FontAsset not found at Resources/Fonts & Materials/Bangers SDF. Make sure the asset and meta file are restored.");
                return;
            }

            int updated = 0;
            var texts = Object.FindObjectsOfType<TMP_Text>(true);
            foreach (var tmp in texts)
            {
                if (tmp.font != fontAsset)
                {
                    Undo.RecordObject(tmp, "Assign Bangers Font");
                    tmp.font = fontAsset;
                    EditorUtility.SetDirty(tmp);
                    updated++;
                }
            }

            Debug.Log($"Assigned Bangers SDF to {updated} TextMeshPro objects across loaded scenes.");
        }
    }
}
