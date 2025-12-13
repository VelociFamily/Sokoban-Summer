# Dynamic Level Management System - Implementation Guide

## Overview
This system provides dynamic level discovery, management, and UI population for the Sokoban Summer game. It allows designers to easily add new levels by dropping scenes into folders without code changes.

## Key Components

### 1. LevelData (Scriptable Object)
Stores metadata for each level:
- **Level Title**: Display name
- **Description**: Brief description  
- **Sort Order**: Ordering priority (lower = earlier)
- **Par Moves/Time**: Target goals for completion
- **Preview Image**: Image for level selection
- **Requires Unlock**: Whether level needs progression

### 2. LevelManager (Singleton)
Core system that:
- Scans build settings for levels in Levels/Tutorials folders
- Provides API for level discovery and management
- Handles level progression and unlocking
- Integrates with existing systems (LevelLogger, SceneSelector)

### 3. DynamicLevelSelector (UI Component)
UI component that:
- Automatically populates level selection screens
- Shows level information and goals
- Handles lock/unlock states
- Updates when progression changes

## Setup Instructions

### For New Projects:
1. Add a LevelManager to your Game scene:
   ```
   GameObject > Create Empty > Name: "Level Manager"
   Add Component > LevelManager
   Configure folder paths if needed (default: "Levels" and "Tutorials")
   Enable Debug Mode for development
   ```

2. Create your level button prefab:
   ```
   Create UI > Button
   Add DynamicLevelButton component
   Add child Text objects for level name and goals
   Add child Image for preview thumbnail
   Add lock overlay GameObject (optional)
   Save as prefab
   ```

3. Set up level selection UI:
   ```
   Add DynamicLevelSelector component to your UI
   Assign Level Button Container (ScrollView content area)
   Assign Level Button Prefab (created above)
   Configure display options (tutorials, levels, headers)
   ```

### For Each Level Scene:
1. Create or open your level scene
2. Add SceneInfo component to a GameObject:
   ```
   Create Empty GameObject > Name: "Level Info"
   Add SceneInfo component
   Set Scene Type: TutorialLevel or GameplayLevel
   Set Level Number for progression
   ```

3. Create LevelData asset (optional but recommended):
   ```
   Right-click in Project > Create > Sokoban > Level Data
   Configure title, description, goals, and thumbnail
   Assign to SceneInfo's Level Data field
   ```

4. Add scene to Build Settings and ensure proper ordering

### For New Levels:
1. Create scene in `Assets/Scenes/Levels/` folder
2. Follow naming pattern: `01_Level Name.unity` (number helps with ordering)
3. Add essential GameObjects:
   - Level Manager with SceneInfo component
   - Player
   - Level geometry and interactive objects
   - UI Canvas with required elements
4. Configure SceneInfo:
   - Scene Type: GameplayLevel (or TutorialLevel)
   - Show Background: false (for gameplay levels)
   - Level Number: Sequential number
   - Level Data: Optional - create via Right-click > Create > Sokoban > Level Data
5. Add scene to Build Settings
6. Test the level

### Creating LevelData Assets (Optional):
1. In Project window, Right-click in a folder
2. Select Create > Sokoban > Level Data
3. Name the asset appropriately (e.g., "Level_01_Data")
4. Configure the fields in the Inspector:
   - Level Title: Display name
   - Description: Brief description
   - Sort Order: Numeric order (lower = appears first)
   - Par Moves/Time: Target goals (0 = no goal)
   - Preview Image: Optional sprite for level selection
   - Requires Unlock: Whether level needs progression
5. Reference this asset in your level's SceneInfo component

## API Reference

### LevelManager
```csharp
// Get all levels
List<LevelInfo> GetAllLevels()
List<LevelInfo> GetLevels(SceneType sceneType)

// Find specific levels
LevelInfo GetLevelByBuildIndex(int buildIndex)
LevelInfo GetLevelByName(string sceneName)

// Level loading and progression
bool CanLoadLevel(LevelInfo levelInfo)
void LoadLevel(LevelInfo levelInfo)
void MarkLevelCompleted(int buildIndex)
LevelInfo GetNextLevel(LevelInfo currentLevel)

// Utilities
void RescanLevels() // Force rescan (development)
```

### LevelInfo Structure
```csharp
public class LevelInfo
{
    public string sceneName;
    public string scenePath;
    public int buildIndex;
    public SceneType sceneType;
    public LevelData levelData;
    public int sortOrder;
    public string displayName;
    public Sprite previewImage;
    public int parMoves;
    public float parTime;
    public bool requiresUnlock;
}
```

### DynamicLevelSelector
```csharp
// UI Management
void PopulateLevelButtons()
void RefreshLevelSelection()
void OnLevelCompleted()

// Configuration
public bool showTutorials = true;
public bool showGameplayLevels = true;
public bool addSectionHeaders = true;
```

## Integration with Existing Systems

### LevelLogger Integration
- Automatically notifies LevelManager when levels are completed
- Uses LevelManager for level name display
- Maintains backward compatibility with existing score tracking

### SceneSelector Integration
- Uses existing unlock logic for level progression
- LevelManager delegates CanLoadLevel checks to SceneSelector
- Maintains existing unlock states and progression

### SceneInfo Integration
- Extends existing SceneInfo with optional LevelData reference
- Maintains backward compatibility with build index fallbacks
- Uses existing SceneType system

## Testing and Validation

### Use LevelSystemTest Component
1. Add LevelSystemTest to a GameObject in any scene
2. Run the test from context menu or automatically on Start
3. Check Console for discovered levels and system status

### Expected Output Example:
```
=== Level System Test Started ===
✓ LevelManager instance found
✓ Found 6 total levels
✓ Found 4 tutorial levels
✓ Found 2 gameplay levels
--- Discovered Levels ---
  Moving Tutorial [TutorialLevel] - Build Index: 2 - UNLOCKED
  Button Tutorial [TutorialLevel] - Build Index: 3 - LOCKED
  Level One [GameplayLevel] - Build Index: 5 - LOCKED
=== Level System Test Completed ===
```

## Best Practices

### Scene Organization
```
Assets/Scenes/
├── Levels/                 # Gameplay levels
│   ├── 01_Tutorial_End.unity
│   ├── 02_Easy_Start.unity
│   └── 03_Medium_Challenge.unity
├── Tutorials/              # Tutorial levels  
│   ├── Moving Tutorial.unity
│   ├── Button Tutorial.unity
│   └── Advanced Tutorial.unity
└── Main Menu.unity
```

### Level Naming
- **With sorting**: `01_Level Name.unity`, `02_Another Level.unity`
- **Descriptive**: `Easy Warmup.unity`, `Box Maze Challenge.unity`
- **Consistent**: Use same pattern throughout project

### LevelData Usage
- Create LevelData assets for levels with specific goals
- Store in `Assets/Data/LevelData/` for organization
- Name consistently: `01_Level_Name_Data.asset`

## Troubleshooting

### Common Issues:
1. **Levels not discovered**: Check folder paths in LevelManager configuration
2. **Wrong order**: Check scene names for numeric prefixes or LevelData sort orders  
3. **Unlock issues**: Verify SceneSelector has proper progression state
4. **UI not updating**: Call RefreshLevelSelection() after progression changes

### Debug Features:
- Enable Debug Mode in LevelManager for detailed logging
- Use RescanLevels() context menu to force refresh
- Run LevelSystemTest to validate system state

## Future Enhancements

Potential improvements:
- Asset bundle support for downloadable levels
- Level rating and difficulty system
- Custom unlock conditions beyond linear progression
- Level editor integration
- Community level sharing

This system provides a solid foundation that can be extended as needed while maintaining compatibility with existing code.