# Persistent UI Menu Structure Guide

## Overview

This guide explains the proper structure for organizing menus (Main Menu, Settings, Level Selection, Credits, etc.) in the new **PersistentUI scene** architecture, replacing the old `MenuPersistence` SetActive-based approach.

## The Problem with Your Current Setup

Based on your scene hierarchy, you've moved entire Canvas objects from Main Menu to PersistentUI, but this creates confusion because:

1. **Multiple Canvases**: Each menu (Main Menu, Settings, etc.) is a separate Canvas in PersistentUI
2. **No Clear Navigation**: There's no script managing which menu should be visible
3. **Duplicate Content**: Both Main Menu scene and PersistentUI scene have similar elements
4. **SetActive Still Used**: Scripts like `ObjectShower` and `Startgame` still use `SetActive(true/false)` on menu GameObjects

## The Correct Architecture

### Option A: Single Canvas with Multiple Menu Panels (Recommended)

This is the **cleanest and most scalable** approach:

```
PersistentUI (Scene)
├── PersistentUIManager (GameObject)
│   ├── PersistentUIManager (Component)
│   ├── EventSystem (Component)
│   └── StandaloneInputModule (Component)
└── MenuCanvas (Single Canvas - Screen Space Overlay)
    ├── CanvasGroup (Component) ← Controls fade for entire UI
    ├── MenuNavigator (Component) ← NEW: Manages which panel is visible
    │
    ├── MainMenuPanel (GameObject with CanvasGroup)
    │   ├── Title
    │   ├── PlayButton
    │   ├── SettingsButton
    │   ├── CreditsButton
    │   ├── AchievementsButton
    │   └── ExitButton
    │
    ├── SettingsMenuPanel (GameObject with CanvasGroup)
    │   ├── MusicSlider
    │   ├── SFXSlider
    │   ├── BackButton
    │   └── ...
    │
    ├── LevelSelectionPanel (GameObject with CanvasGroup)
    │   ├── DynamicLevelSelector
    │   ├── BackButton
    │   └── ...
    │
    ├── CreditsPanel (GameObject with CanvasGroup)
    │   ├── CreditsText
    │   ├── BackButton
    │   └── ...
    │
    └── AchievementsPanel (GameObject with CanvasGroup)
        ├── AchievementShower
        ├── BackButton
        └── ...
```

### Key Principles

1. **Single Canvas**: All menus share one Canvas (reduces draw calls, easier to manage)
2. **Panels, Not Canvases**: Each "menu" is a panel (GameObject) with its own CanvasGroup
3. **CanvasGroup Per Panel**: Each panel has a CanvasGroup to control visibility via alpha (not SetActive)
4. **MenuNavigator Script**: A single script manages which panel is visible at any time
5. **No SetActive on Panels**: Use CanvasGroup alpha to show/hide panels smoothly

## Implementation Steps

### Step 1: Create MenuNavigator Script

Create a new script to manage menu navigation:

```csharp
// Assets/_Project/SokobanSummer/Scripts/Scripts/UI/MenuNavigator.cs
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Manages navigation between different menu panels using CanvasGroup-based visibility.
    /// Replaces SetActive toggling with smooth alpha transitions.
    /// </summary>
    public class MenuNavigator : MonoBehaviour
    {
        [System.Serializable]
        public class MenuPanel
        {
            public string panelName;
            public CanvasGroup canvasGroup;
            public GameObject firstSelectedButton; // For gamepad/keyboard navigation
        }

        [Header("Menu Panels")]
        [Tooltip("All menu panels managed by this navigator")]
        public List<MenuPanel> menuPanels = new List<MenuPanel>();

        [Header("Settings")]
        [Tooltip("Default panel to show when starting")]
        public string defaultPanelName = "MainMenu";

        [Tooltip("Transition duration for fading panels")]
        public float transitionDuration = 0.2f;

        private MenuPanel currentPanel;
        private Dictionary<string, MenuPanel> panelLookup;

        private void Awake()
        {
            // Build lookup dictionary
            panelLookup = new Dictionary<string, MenuPanel>();
            foreach (var panel in menuPanels)
            {
                if (panel.canvasGroup != null)
                {
                    panelLookup[panel.panelName] = panel;
                    // Initialize all panels as hidden
                    SetPanelVisibility(panel, false, instant: true);
                }
            }
        }

        private void Start()
        {
            // Show default panel
            ShowPanel(defaultPanelName, instant: true);
        }

        /// <summary>
        /// Show a specific panel by name, hiding all others
        /// </summary>
        public void ShowPanel(string panelName, bool instant = false)
        {
            if (!panelLookup.TryGetValue(panelName, out var panel))
            {
                Debug.LogError($"[MenuNavigator]: Panel '{panelName}' not found!");
                return;
            }

            // Hide current panel
            if (currentPanel != null && currentPanel != panel)
            {
                SetPanelVisibility(currentPanel, false, instant);
            }

            // Show new panel
            SetPanelVisibility(panel, true, instant);
            currentPanel = panel;

            // Set first selected button for input navigation
            if (panel.firstSelectedButton != null)
            {
                UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(panel.firstSelectedButton);
            }

            Debug.Log($"[MenuNavigator]: Switched to panel '{panelName}'");
        }

        /// <summary>
        /// Show main menu panel
        /// </summary>
        public void ShowMainMenu() => ShowPanel("MainMenu");

        /// <summary>
        /// Show settings panel
        /// </summary>
        public void ShowSettings() => ShowPanel("Settings");

        /// <summary>
        /// Show level selection panel
        /// </summary>
        public void ShowLevelSelection() => ShowPanel("LevelSelection");

        /// <summary>
        /// Show credits panel
        /// </summary>
        public void ShowCredits() => ShowPanel("Credits");

        /// <summary>
        /// Show achievements panel
        /// </summary>
        public void ShowAchievements() => ShowPanel("Achievements");

        /// <summary>
        /// Go back to main menu (common back button action)
        /// </summary>
        public void BackToMainMenu() => ShowMainMenu();

        /// <summary>
        /// Sets panel visibility using CanvasGroup
        /// </summary>
        private void SetPanelVisibility(MenuPanel panel, bool visible, bool instant = false)
        {
            if (panel.canvasGroup == null) return;

            float targetAlpha = visible ? 1f : 0f;

            if (instant || transitionDuration <= 0f)
            {
                panel.canvasGroup.alpha = targetAlpha;
                panel.canvasGroup.interactable = visible;
                panel.canvasGroup.blocksRaycasts = visible;
            }
            else
            {
                // Stop any existing transition
                StopAllCoroutines();
                StartCoroutine(FadePanel(panel.canvasGroup, targetAlpha, visible));
            }
        }

        /// <summary>
        /// Smoothly fades a panel to target alpha
        /// </summary>
        private System.Collections.IEnumerator FadePanel(CanvasGroup canvasGroup, float targetAlpha, bool visible)
        {
            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;

            // Enable interaction immediately if showing
            if (visible)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;

            // Disable interaction after hiding
            if (!visible)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Get the currently active panel name
        /// </summary>
        public string GetCurrentPanelName()
        {
            return currentPanel?.panelName ?? "None";
        }
    }
}
```

### Step 2: Restructure PersistentUI Scene

In Unity Editor:

1. **Consolidate Canvases**:
   - Keep only ONE Canvas in PersistentUI scene
   - Delete duplicate Canvases (Settings Canvas, Level Selection Canvas, etc.)

2. **Create Panel Structure**:
   - Under the single Canvas, create child GameObjects for each menu:
     - `MainMenuPanel`
     - `SettingsPanel`
     - `LevelSelectionPanel`
     - `CreditsPanel`
     - `AchievementsPanel`

3. **Add CanvasGroups**:
   - Add a `CanvasGroup` component to each panel GameObject
   - Set initial alpha to 0 (they'll be hidden until MenuNavigator shows them)

4. **Move UI Elements**:
   - Move each menu's buttons/UI elements into their respective panel
   - Example: Move Settings buttons/sliders into `SettingsPanel`

5. **Add MenuNavigator**:
   - Add `MenuNavigator` component to the Canvas GameObject
   - Assign all panels in the inspector
   - Set panel names: "MainMenu", "Settings", "LevelSelection", "Credits", "Achievements"
   - Assign first selected button for each panel (for keyboard/gamepad)
   - Set default panel to "MainMenu"

### Step 3: Update Button OnClick Events

Change button callbacks from:

```csharp
// OLD WAY (using SetActive)
settingsButton.onClick.AddListener(() => {
    mainMenuCanvas.SetActive(false);
    settingsCanvas.SetActive(true);
});
```

To:

```csharp
// NEW WAY (using MenuNavigator)
settingsButton.onClick.AddListener(() => {
    menuNavigator.ShowSettings();
});
```

In Unity Inspector:
- Settings Button → OnClick → MenuNavigator.ShowSettings()
- Back Button (in Settings) → OnClick → MenuNavigator.ShowMainMenu()
- Level Selection Button → OnClick → MenuNavigator.ShowLevelSelection()
- Credits Button → OnClick → MenuNavigator.ShowCredits()
- etc.

### Step 4: Update PersistentUIManager

`PersistentUIManager` should reference the **root Canvas's CanvasGroup**, NOT individual panel CanvasGroups:

```csharp
[SerializeField] private List<CanvasGroup> persistentUIGroups = new List<CanvasGroup>();
```

Assign only the **Canvas's CanvasGroup** here (the one that fades the entire UI during scene transitions).

### Step 5: Clean Up Main Menu Scene

The Main Menu scene should now be **minimal or empty**:

```
Main Menu (Scene)
├── Main Camera (if needed for transitions)
└── SceneInfo (Component to mark this as MainMenu scene type)
```

**Remove**:
- All UI Canvases (they're now in PersistentUI)
- EventSystem (managed by PersistentUIManager)
- Menu navigation scripts (managed by MenuNavigator)

### Step 6: Update Scripts Using SetActive

Find and update scripts that toggle menus:

**ObjectShower.cs**:
```csharp
// OLD
public GameObject menu;
menu.SetActive(false);

// NEW
private MenuNavigator menuNavigator;

void Awake()
{
    menuNavigator = FindFirstObjectByType<MenuNavigator>();
}

public void show()
{
    if (objectToShow != null)
        objectToShow.SetActive(true);
    
    // No need to hide menu manually - MenuNavigator handles it
    // Or explicitly show a specific panel:
    menuNavigator?.ShowMainMenu();
}
```

**Startgame.cs**:
```csharp
// OLD
public GameObject menu;
menu.SetActive(true);

// NEW
private MenuNavigator menuNavigator;

void Awake()
{
    menuNavigator = FindFirstObjectByType<MenuNavigator>();
}

public void LoadNextScene()
{
    foreach (var obj in startObjects.Where(obj => obj != null))
        obj.SetActive(false);
    
    menuNavigator?.ShowMainMenu();
}
```

## Option B: Multiple Canvases (Not Recommended)

If you have a compelling reason to use separate Canvases (e.g., different sort orders), you can keep them but:

1. Add **CanvasGroup to each Canvas**
2. Assign all Canvas CanvasGroups to **MenuNavigator**
3. MenuNavigator will manage visibility the same way

Structure:
```
PersistentUI (Scene)
├── PersistentUIManager
├── MainMenuCanvas (CanvasGroup)
├── SettingsCanvas (CanvasGroup)
├── LevelSelectionCanvas (CanvasGroup)
└── CreditsCanvas (CanvasGroup)
```

**Downsides**:
- More draw calls (each Canvas is a separate batch)
- Harder to manage layering
- More complex scene hierarchy

## Benefits of This Approach

✅ **No Flickering**: CanvasGroup alpha transitions are smooth
✅ **Centralized Logic**: MenuNavigator manages all navigation
✅ **Scalable**: Easy to add new menu panels
✅ **Persistent State**: UI state preserved across scene transitions
✅ **Performance**: Single Canvas reduces draw calls
✅ **Clean Scene**: Main Menu scene is minimal

## Testing Checklist

- [ ] PersistentUI scene loads on game start
- [ ] Main Menu panel shows by default
- [ ] Settings button opens Settings panel with smooth fade
- [ ] Back buttons return to Main Menu
- [ ] Level Selection button opens Level Selection panel
- [ ] Credits button opens Credits panel
- [ ] Achievements button opens Achievements panel
- [ ] No UI flicker during panel transitions
- [ ] Keyboard/gamepad navigation works (first button auto-selected)
- [ ] Menu UI hides during gameplay
- [ ] Menu UI shows when returning to main menu
- [ ] No SetActive calls remain in button scripts

## Common Pitfalls

❌ **Using SetActive on Panels**: This defeats the purpose of CanvasGroup-based visibility
❌ **Multiple Canvases**: Adds complexity without benefit in most cases
❌ **Forgetting CanvasGroups**: MenuNavigator requires CanvasGroup on each panel
❌ **Wrong CanvasGroup in PersistentUIManager**: Should reference the root Canvas CanvasGroup, not individual panels
❌ **Duplicate UI**: Don't keep menus in both Main Menu and PersistentUI scenes

## Summary

**Old Way (MenuPersistence)**:
- Multiple scenes with duplicate UI
- SetActive toggling causes flicker
- Complex duplicate cleanup logic

**New Way (PersistentUIManager + MenuNavigator)**:
- Single persistent scene with all UI
- CanvasGroup alpha for smooth transitions
- Centralized navigation logic
- No duplicates or cleanup needed

The key insight: **Panels are always active GameObjects, but their visibility is controlled via CanvasGroup alpha**. This is faster, smoother, and more maintainable than SetActive toggling.
