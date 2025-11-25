using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class FinishOrganizingAchievements
{
    public static void Execute()
    {
        string rootPath = "Menu Canvas/Achieverment Text";
        GameObject root = GameObject.Find(rootPath);
        if (root == null) { Debug.LogError("Root not found"); return; }

        Transform containerTr = root.transform.Find("Badges Container");
        GameObject badgesContainer = containerTr.gameObject;

        // Helper to process badge
        void ProcessBadge(GameObject go, string spritePath)
        {
            // Remove SpriteRenderer if exists
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) Object.DestroyImmediate(sr);

            // Add RectTransform if missing
            if (go.GetComponent<RectTransform>() == null)
            {
                Undo.AddComponent<RectTransform>(go);
            }

            // Add Image if missing
            Image img = go.GetComponent<Image>();
            if (img == null)
            {
                img = Undo.AddComponent<Image>(go);
            }
            
            // Set Sprite
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (s != null) img.sprite = s;
            
            // Set size
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        }

        // 1. Tutorial
        Transform t1 = badgesContainer.transform.Find("Tutorial");
        if (t1 != null) ProcessBadge(t1.gameObject, "Assets/_Project/SokobanSummer/Textures/Achievement badges/ChatGPT Image Jul 28, 2025, 01_14_57 PM.png");

        // 2. SpeedAndConfuse
        Transform t2 = badgesContainer.transform.Find("SpeedAndConfuse");
        if (t2 != null) ProcessBadge(t2.gameObject, "Assets/_Project/SokobanSummer/Textures/Achievement badges/ChatGPT Image Jul 28, 2025, 01_17_53 PM.png");

        // 3. CompleteLevelTwo (might be in root)
        Transform t3 = root.transform.Find("CompleteLevelTwo");
        if (t3 == null) t3 = badgesContainer.transform.Find("CompleteLevelTwo");
        
        if (t3 != null)
        {
            GameObject go3 = t3.gameObject;
            ProcessBadge(go3, "Assets/_Project/SokobanSummer/Textures/Achievement badges/ChatGPT Image Jul 28, 2025, 01_21_32 PM (1).png");
            
            // Use go3.transform instead of t3, because t3 might be destroyed
            if (go3.transform.parent != badgesContainer.transform)
            {
                go3.transform.SetParent(badgesContainer.transform, false);
            }
        }

        // Layouts
        HorizontalLayoutGroup hlg = badgesContainer.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null) hlg = badgesContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 20;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        VerticalLayoutGroup vlg = root.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = root.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = 20;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        Transform logs = root.transform.Find("Logs");
        if (logs != null) logs.SetSiblingIndex(root.transform.childCount - 1);

        Debug.Log("Finished organizing.");
    }
}
