# Level Creation Guide

## How to Create New Levels

### 1. Copy the Level Template
1. Navigate to `Assets/Scenes/Levels/` in Unity
2. Right-click in the folder and select "Create > Scene"
3. Name your scene following the pattern: `01_Level Name.unity` (number prefix helps with ordering)

### 2. Set Up Scene Structure
Your level should include these essential components:

#### Required GameObjects:
- **Level Manager** - Empty GameObject with:
  - `SceneInfo` component configured as `GameplayLevel`
  - `LevelData` scriptable object (optional but recommended)
- **Player** - Player character
- **Level Geometry** - Walls, floors, etc.
- **Interactive Objects** - Boxes, buttons, goals, etc.
- **Audio Manager** - For sound effects
- **Canvas** - UI elements

#### Level Data Configuration:
To create rich level metadata:
1. In Unity's Project window, right-click in a folder (e.g., create a "LevelData" folder)
2. Select Create > Sokoban > Level Data
3. Name your asset (e.g., "Level_01_Data")
4. Configure the fields in Inspector:
   - **Level Title**: Display name for the level
   - **Description**: Brief description  
   - **Sort Order**: Number for level ordering (lower = earlier)
   - **Par Moves**: Target number of moves (0 = no goal)
   - **Par Time**: Target completion time in seconds (0 = no goal)
   - **Preview Image**: Optional preview sprite for level selection
   - **Requires Unlock**: Whether level needs to be unlocked
5. In your level scene's SceneInfo component, drag this asset to the "Level Data" field

### 3. Design Your Level
- Use existing prefabs from `Assets/Prefabs/` for consistency
- Follow the same patterns as existing levels
- Test player movement and interactions
- Ensure there's a clear solution path

### 4. Configure Scene Info
Select the Level Manager GameObject and configure the `SceneInfo` component:
- **Scene Type**: Set to `GameplayLevel` (or `TutorialLevel` for tutorials)
- **Show Background**: Usually `false` for gameplay levels
- **Level Number**: Sequential number for progression tracking
- **Level Data**: Drag your `LevelData` asset here (optional)

### 5. Add to Build Settings
1. Go to File > Build Settings
2. Click "Add Open Scenes" to add your new scene
3. Make sure it's positioned correctly in the build order

### 6. Test Your Level
- Play test the scene directly
- Test from the main menu to ensure proper loading
- Verify level completion triggers work correctly
- Check that the next level unlocks properly

## Level Template Structure

```
New Level Scene
├── Level Manager (GameObject)
│   ├── SceneInfo (Component)
│   └── LevelData (ScriptableObject reference)
├── Player
├── Level Geometry
│   ├── Walls
│   ├── Floor
│   └── Decorations
├── Interactive Objects
│   ├── Boxes
│   ├── Buttons
│   └── Goals
├── Audio Manager
└── UI Canvas
    ├── Move Counter
    ├── Timer
    └── Pause Menu
```

## Naming Conventions

### Scene Naming:
- **Tutorials**: `Moving Tutorial.unity`, `Button Tutorial.unity`
- **Levels**: `01_Level Name.unity`, `02_Another Level.unity`
- Use descriptive names that indicate difficulty or mechanics

### LevelData Asset Naming:
- Match the scene name: `01_Level Name_Data.asset`
- Keep in the same folder or a dedicated LevelData folder

## Integration with Dynamic System

The new dynamic level system will:
- Automatically discover your levels in the Levels folder
- Sort them by the numeric prefix or Sort Order
- Display them in the level selection screen
- Handle unlocking progression automatically
- Track completion and best scores

No additional code changes are needed - just follow this structure and the system will handle the rest!

## Tips for Level Design

1. **Start Simple**: Begin with basic mechanics and gradually add complexity
2. **Clear Goals**: Make it obvious what the player needs to accomplish  
3. **Fair Difficulty**: Ensure the level is solvable with reasonable effort
4. **Test Thoroughly**: Play through multiple times to catch issues
5. **Consider Flow**: Think about how this level fits in the overall progression
6. **Use Par Goals**: Set realistic move/time targets to encourage replayability