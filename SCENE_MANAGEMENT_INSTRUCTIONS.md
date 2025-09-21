# Scene Management System Documentation

## Overview
This document explains the new scene management system that replaces hardcoded build index numbers with a more maintainable approach using `SceneInfo` components and `SceneType` enums.

## Problem with Previous Approach
The old system used hardcoded build index numbers (0, 1, 2-7) throughout the code:
```csharp
// Old approach - brittle and hard to maintain
switch (activeScene.buildIndex)
{
    case >= 2 and <= 7: // Tutorial and level scenes
        backgroundClone.SetActive(false);
        break;
    case 1: // Main Menu scene
        backgroundClone.SetActive(true);
        break;
}
```

## New Scene Management System

### SceneType Enum
Defines the types of scenes in the game:
- `Game` (0) - Initial game scene with GameInitializer
- `MainMenu` (1) - Main menu scene
- `TutorialLevel` (2) - Tutorial level scenes
- `GameplayLevel` (3) - Regular gameplay level scenes

### SceneInfo Component
A MonoBehaviour component that should be added to each scene to define:
- **Scene Type** - What type of scene this is
- **Show Background** - Whether the background should be visible in this scene
- **Level Number** - For level progression tracking (tutorial/gameplay levels only)

## Unity Editor Setup Instructions

### 1. Adding SceneInfo to Existing Scenes

For each scene in your project:

1. **Open the scene** in Unity Editor
2. **Create an empty GameObject** (or use an existing manager object)
3. **Add the SceneInfo component** to the GameObject
4. **Configure the settings**:
   - **Main Menu Scene**: SceneType = MainMenu, Show Background = true
   - **Tutorial Levels**: SceneType = TutorialLevel, Show Background = false, Level Number = 1, 2, 3...
   - **Gameplay Levels**: SceneType = GameplayLevel, Show Background = false, Level Number = 1, 2, 3...
   - **Game Scene**: SceneType = Game, Show Background = true

### 2. Recommended Scene Setup

**Main Menu Scene:**
```
- Main Menu Scene
  └── SceneManager (GameObject)
      └── SceneInfo (Component)
          - Scene Type: MainMenu
          - Show Background: ✓ true
```

**Tutorial/Level Scenes:**
```
- Level 1 Scene
  └── LevelManager (GameObject)
      └── SceneInfo (Component)
          - Scene Type: TutorialLevel (or GameplayLevel)
          - Show Background: ✗ false
          - Level Number: 1
```

### 3. Background Visibility Rules

The system automatically handles background visibility:
- **Show Background = true**: Background visible (typically menus)
- **Show Background = false**: Background hidden (typically gameplay levels)
- **Fallback**: If no SceneInfo found, uses old build index logic

## Benefits of New System

1. **Self-Documenting**: Scene types are clearly defined
2. **Maintainable**: No hardcoded magic numbers
3. **Flexible**: Easy to change scene behavior without code changes
4. **Extensible**: Easy to add new scene types
5. **Visual**: Configuration visible in Unity Inspector
6. **Backward Compatible**: Falls back to build index if SceneInfo missing

## Code Usage

### Checking Scene Type
```csharp
SceneType currentType = SceneInfo.GetActiveSceneType();
if (currentType == SceneType.TutorialLevel)
{
    // Handle tutorial-specific logic
}
```

### Checking Background Visibility
```csharp
bool shouldShow = SceneInfo.ShouldShowBackground();
backgroundClone.SetActive(shouldShow);
```

### Getting Scene Information
```csharp
SceneInfo info = SceneInfo.GetActiveSceneInfo();
if (info != null)
{
    Debug.Log($"Current scene: {info.sceneType}, Level: {info.levelNumber}");
}
```

## Migration Checklist

- [x] Created SceneType enum
- [x] Created SceneInfo component
- [x] Updated GameInitializer to use new system
- [x] Updated CompleteUI to use new system
- [ ] Add SceneInfo components to all scenes
- [ ] Test background visibility in all scenes
- [ ] Remove old build index constants (if any)