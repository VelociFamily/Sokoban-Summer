using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;
using UI;

namespace Editor
{
    /// <summary>
    /// Editor utility to create and configure the LevelButton prefab with proper sprite settings
    /// </summary>
    public static class LevelButtonPrefabCreator
    {
        private const string SPRITES_PATH = "Assets/_Project/SokobanSummer/Sprites/UI/LevelButton";
        private const string PREFAB_OUTPUT_PATH = "Assets/_Project/SokobanSummer/Prefabs/UI/LevelButton.prefab";
        
        [MenuItem("Tools/Sokoban Summer/Create Level Button Prefab")]
        public static void CreateLevelButtonPrefab()
        {
            // Step 1: Configure sprite import settings
            ConfigureSpriteImportSettings();
            
            // Step 2: Create prefab hierarchy
            GameObject prefabRoot = CreatePrefabHierarchy();
            
            // Step 3: Save as prefab
            SavePrefab(prefabRoot);
            
            Debug.Log($"[LevelButtonPrefabCreator] Successfully created LevelButton prefab at {PREFAB_OUTPUT_PATH}");
        }
        
        private static void ConfigureSpriteImportSettings()
        {
            Debug.Log("[LevelButtonPrefabCreator] Configuring sprite import settings...");
            
            string[] spriteFiles = new string[]
            {
                "LevelButton_Base.png",
                "LockIcon.png",
                "CompletionBadge.png",
                "FallbackThumbnail.png",
                "LockOverlay.png"
            };
            
            foreach (string fileName in spriteFiles)
            {
                string spritePath = $"{SPRITES_PATH}/{fileName}";
                
                if (!File.Exists(Path.Combine(Application.dataPath, "..", spritePath)))
                {
                    Debug.LogWarning($"[LevelButtonPrefabCreator] Sprite not found: {spritePath}");
                    continue;
                }
                
                TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
                if (importer == null)
                {
                    AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
                    importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
                }
                
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 100f;
                    importer.filterMode = FilterMode.Point; // Pixel-perfect look
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.mipmapEnabled = false;
                    
                    // Configure 9-slicing for background
                    if (fileName == "LevelButton_Base.png")
                    {
                        importer.spriteBorder = new Vector4(16, 16, 16, 16); // 16px borders for 9-slice
                    }
                    
                    importer.SaveAndReimport();
                    Debug.Log($"[LevelButtonPrefabCreator] Configured: {fileName}");
                }
            }
            
            AssetDatabase.Refresh();
        }
        
        private static GameObject CreatePrefabHierarchy()
        {
            Debug.Log("[LevelButtonPrefabCreator] Creating prefab hierarchy...");
            
            // Root GameObject with Button, Image, and DynamicLevelButton
            GameObject root = new GameObject("LevelButton");
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(400, 96); // reduced height for denser list
            
            Image rootImage = root.AddComponent<Image>();
            rootImage.color = Color.white;
            
            Button button = root.AddComponent<Button>();
            ConfigureButtonTransition(button);
            
            DynamicLevelButton dynamicButton = root.AddComponent<DynamicLevelButton>();
            
            // 1. Background Image (9-sliced)
            GameObject background = CreateChild(root, "Background");
            Image bgImage = background.AddComponent<Image>();
            Sprite bgSprite = LoadSprite("LevelButton_Base.png");
            if (bgSprite != null)
            {
                bgImage.sprite = bgSprite;
                bgImage.type = Image.Type.Sliced;
            }
            StretchToFill(background.GetComponent<RectTransform>());
            
            // 2. Content Wrapper (Horizontal Layout)
            GameObject contentWrapper = CreateChild(root, "ContentWrapper");
            RectTransform contentRect = contentWrapper.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(16, 12); // tighter padding
            contentRect.offsetMax = new Vector2(-16, -12);
            
            HorizontalLayoutGroup layout = contentWrapper.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.spacing = 16;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            
            // 2a. Preview Image (Thumbnail)
            GameObject preview = CreateChild(contentWrapper, "PreviewImage");
            RectTransform previewRect = preview.GetComponent<RectTransform>();
            previewRect.sizeDelta = new Vector2(72, 72);
            Image previewImage = preview.AddComponent<Image>();
            Sprite fallbackSprite = LoadSprite("FallbackThumbnail.png");
            if (fallbackSprite != null)
            {
                previewImage.sprite = fallbackSprite;
            }
            previewImage.preserveAspect = true;
            
            // 2b. Text Column (Vertical Layout)
            GameObject textColumn = CreateChild(contentWrapper, "TextColumn");
            RectTransform textRect = textColumn.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(250, 72);
            
            VerticalLayoutGroup textLayout = textColumn.AddComponent<VerticalLayoutGroup>();
            textLayout.childAlignment = TextAnchor.UpperLeft;
            textLayout.spacing = 4;
            textLayout.childControlWidth = true;
            textLayout.childControlHeight = false;
            textLayout.childForceExpandWidth = true;
            textLayout.childForceExpandHeight = false;
            
            // 2b-i. Level Name Text
            GameObject levelNameObj = CreateChild(textColumn, "LevelNameText");
            TextMeshProUGUI levelName = levelNameObj.AddComponent<TextMeshProUGUI>();
            levelName.text = "Level Name";
            levelName.fontSize = 24;
            levelName.fontStyle = FontStyles.Bold;
            levelName.color = new Color(0.047f, 0.122f, 0.208f); // #0C1F35
            levelName.alignment = TextAlignmentOptions.Left;
            
            // 2b-ii. Goal Text
            GameObject goalTextObj = CreateChild(textColumn, "GoalText");
            TextMeshProUGUI goalText = goalTextObj.AddComponent<TextMeshProUGUI>();
            goalText.text = "Par: 10 moves";
            goalText.fontSize = 16;
            goalText.color = new Color(0.063f, 0.227f, 0.388f); // #103A63
            goalText.alignment = TextAlignmentOptions.Left;
            
            // 3. Completion Badge
            GameObject badge = CreateChild(root, "CompletionBadge");
            RectTransform badgeRect = badge.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1, 1);
            badgeRect.anchorMax = new Vector2(1, 1);
            badgeRect.anchoredPosition = new Vector2(-20, -20);
            badgeRect.sizeDelta = new Vector2(40, 40);
            
            Image badgeImage = badge.AddComponent<Image>();
            Sprite badgeSprite = LoadSprite("CompletionBadge.png");
            if (badgeSprite != null)
            {
                badgeImage.sprite = badgeSprite;
            }
            badgeImage.preserveAspect = true;
            badge.SetActive(false); // Hidden by default, shown when level completed
            
            // 4. Lock Overlay (semi-transparent with lock icon)
            GameObject lockOverlay = CreateChild(root, "LockOverlay");
            RectTransform lockRect = lockOverlay.GetComponent<RectTransform>();
            StretchToFill(lockRect);
            
            Image lockBg = lockOverlay.AddComponent<Image>();
            Sprite lockOverlaySprite = LoadSprite("LockOverlay.png");
            if (lockOverlaySprite != null)
            {
                lockBg.sprite = lockOverlaySprite;
            }
            else
            {
                lockBg.color = new Color(0, 0.063f, 0.125f, 0.7f); // Fallback semi-transparent dark
            }
            
            // 4a. Lock Icon (centered)
            GameObject lockIcon = CreateChild(lockOverlay, "LockIcon");
            RectTransform lockIconRect = lockIcon.GetComponent<RectTransform>();
            lockIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            lockIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            lockIconRect.anchoredPosition = Vector2.zero;
            lockIconRect.sizeDelta = new Vector2(40, 40);
            
            Image lockIconImage = lockIcon.AddComponent<Image>();
            Sprite lockSprite = LoadSprite("LockIcon.png");
            if (lockSprite != null)
            {
                lockIconImage.sprite = lockSprite;
            }
            lockIconImage.preserveAspect = true;
            
            lockOverlay.SetActive(false); // Hidden by default, shown when locked
            
            // Wire up DynamicLevelButton references
            dynamicButton.button = button;
            dynamicButton.levelNameText = levelName;
            dynamicButton.goalText = goalText;
            dynamicButton.previewImage = previewImage;
            dynamicButton.lockOverlay = lockOverlay;
            dynamicButton.lockOverlayImage = lockIconImage;
            
            return root;
        }
        
        private static void ConfigureButtonTransition(Button button)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.894f, 1f, 0.937f); // #E4FFEF
            colors.pressedColor = new Color(0.780f, 0.933f, 0.839f); // #C7EED6
            colors.disabledColor = new Color(0.561f, 0.667f, 0.627f); // #8FAAA0
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
        }
        
        private static GameObject CreateChild(GameObject parent, string name)
        {
            GameObject child = new GameObject(name);
            child.AddComponent<RectTransform>();
            child.transform.SetParent(parent.transform, false);
            return child;
        }
        
        private static void StretchToFill(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        
        private static Sprite LoadSprite(string fileName)
        {
            string path = $"{SPRITES_PATH}/{fileName}";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"[LevelButtonPrefabCreator] Could not load sprite: {path}");
            }
            return sprite;
        }
        
        private static void SavePrefab(GameObject prefabRoot)
        {
            string directory = Path.GetDirectoryName(PREFAB_OUTPUT_PATH);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Delete existing prefab if present
            if (File.Exists(PREFAB_OUTPUT_PATH))
            {
                AssetDatabase.DeleteAsset(PREFAB_OUTPUT_PATH);
            }
            
            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PREFAB_OUTPUT_PATH);
            
            // Clean up GameObject from scene
            Object.DestroyImmediate(prefabRoot);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select the created prefab
            Object prefab = AssetDatabase.LoadAssetAtPath<Object>(PREFAB_OUTPUT_PATH);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }
    }
}
