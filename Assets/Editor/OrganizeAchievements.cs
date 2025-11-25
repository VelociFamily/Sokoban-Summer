using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class OrganizeAchievements
{
    public static void Execute()
    {
        string rootPath = "Menu Canvas/Achieverment Text";
        GameObject root = GameObject.Find(rootPath);
        if (root == null)
        {
            Debug.LogError("Root not found");
            return;
        }

        Transform containerTr = root.transform.Find("Badges Container");
        GameObject badgesContainer;
        if (containerTr == null)
        {
            badgesContainer = new GameObject("Badges Container");
            badgesContainer.transform.SetParent(root.transform, false);
            badgesContainer.AddComponent<RectTransform>();
        }
        else
        {
            badgesContainer = containerTr.gameObject;
        }
        
        string[] badgeNames = { "Tutorial", "SpeedAndConfuse", "CompleteLevelTwo" };
        foreach (string name in badgeNames)
        {
            Transform t = root.transform.Find(name);
            if (t == null) t = badgesContainer.transform.Find(name);

            if (t != null)
            {
                GameObject go = t.gameObject;
                
                Sprite sprite = null;
                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sprite = sr.sprite;
                    Object.DestroyImmediate(sr);
                }
                else
                {
                    Image img = go.GetComponent<Image>();
                    if (img != null) sprite = img.sprite;
                }

                RectTransform rt = go.GetComponent<RectTransform>();
                if (rt == null)
                {
                    rt = go.AddComponent<RectTransform>();
                }

                if (go.GetComponent<Image>() == null)
                {
                    Image img = go.AddComponent<Image>();
                    if (sprite != null) img.sprite = sprite;
                }

                // Re-fetch transform just in case
                go.transform.SetParent(badgesContainer.transform, false);
                
                if (rt != null) rt.sizeDelta = new Vector2(100, 100);
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
        
        Transform shower = root.transform.Find("Achievement shower");
        if (shower != null)
        {
            LayoutElement le = shower.GetComponent<LayoutElement>();
            if (le == null) le = shower.gameObject.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
        }

        Debug.Log("Achievements organized.");
    }
}
