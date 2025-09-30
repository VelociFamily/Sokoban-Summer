# Level Conversion Guide - Dynamic Level Management System

## Converting Existing Levels

This guide will help you convert existing levels to work with the new Dynamic Level Management System and create new levels properly.

## Step 1: Remove Old System Dependencies

### 1.1 Clean Up SceneSelector (Optional - for complete removal)
If you want to completely remove the old system:

1. Delete or rename `Assets/Scripts/UI/SceneSelector.cs`
2. Delete or rename `Assets/Scripts/UI/SceneButton.cs` 
3. Find any hardcoded level buttons in your UI scenes and replace them

**Note**: The new system can work alongside the old system, so this step is optional.

### 1.2 Update Build Settings
Make sure all your level scenes are added to the build settings:
1. File → Build Settings
2. Add all scenes from `Assets/Scenes/Levels/` and `Assets/Scenes/Tutorials/`
3. Ensure proper order (tutorials first, then levels)

## Step 2: Convert Existing Level Scenes

For each existing level scene, follow these steps:

### 2.1 Open the Level Scene
1. Open your level scene in Unity
2. Find or create a GameObject to hold the SceneInfo component

### 2.2 Add/Configure SceneInfo Component
1. Select the GameObject (or create a new empty GameObject named "Level Manager")
2. Add the `SceneInfo` component if not present
3. Configure the SceneInfo:
   - **Scene Type**: Set to `TutorialLevel` or `GameplayLevel`
   - **Show Background**: Usually `false` for gameplay levels
   - **Level Number**: Sequential number for progression tracking
   - **Level Data**: Leave empty for now (we'll create this next)

### 2.3 Create LevelData Asset (Optional but Recommended)
1. In the Project window, navigate to `Assets/Data/` (create this folder if it doesn't exist)
2. Right-click → Create → Sokoban → Level Data
3. Name it matching your scene name (e.g., "Level One Data", "Moving Tutorial Data")
4. Configure the LevelData asset:
   - **Level Title**: Display name (e.g., "Moving Tutorial", "Challenge Level 1")
   - **Description**: Brief description of the level
   - **Sort Order**: Numeric order (0 = first, 1 = second, etc.)
   - **Par Moves**: Target number of moves (0 = no goal)
   - **Par Time**: Target completion time in seconds (0 = no goal)
   - **Preview Image**: Drag a sprite for the level thumbnail
   - **Requires Unlock**: Usually `false` for tutorials, `true` for levels
   - **Scene Path**: Leave empty (automatically filled)

### 2.4 Link LevelData to Scene
1. Go back to your level scene
2. Select the GameObject with SceneInfo
3. Drag your LevelData asset to the "Level Data" field in SceneInfo

## Step 3: Set Up Level Selection UI

### 3.1 Create Level Button Prefab
1. Create a new GameObject in your scene hierarchy
2. Add these components:
   - `Button` component
   - `DynamicLevelButton` component
   - `Image` component (for background)
3. Create child objects:
   - **Text** for level name (assign to `levelNameText` in DynamicLevelButton)
   - **Text** for goals/par info (assign to `goalText` in DynamicLevelButton)
   - **Image** for preview (assign to `previewImage` in DynamicLevelButton)
   - **GameObject** for lock overlay (assign to `lockOverlay` in DynamicLevelButton)
4. Save as prefab in `Assets/Prefabs/UI/`

### 3.2 Set Up Level Selection Screen
1. Open your Main Menu scene (or level selection scene)
2. Find or create a container for level buttons (e.g., ScrollView content)
3. Add the `DynamicLevelSelector` component to a GameObject
4. Configure DynamicLevelSelector:
   - **Level Button Container**: Drag your button container
   - **Level Button Prefab**: Drag your level button prefab
   - **Section Header Prefab**: Optional header prefab
   - **Show Tutorials**: `true` to show tutorial levels
   - **Show Gameplay Levels**: `true` to show gameplay levels
   - **Add Section Headers**: `true` to add "Tutorials" and "Levels" headers

### 3.3 Add LevelManager to Game Scene
1. Open your Game scene (the main game scene that loads first)
2. Create an empty GameObject named "Level Manager"
3. Add the `LevelManager` component
4. Configure LevelManager:
   - **Levels Folder**: "Levels" (default)
   - **Tutorials Folder**: "Tutorials" (default)
   - **Debug Mode**: `true` for development, `false` for release

## Step 4: Adding Thumbnails

### 4.1 Create Level Screenshots
1. Play your level scenes
2. Take screenshots (Unity Editor: Game view → right-click → Save Image)
3. Import screenshots into Unity (`Assets/Textures/Levels/`)
4. Set Texture Type to "Sprite (2D and UI)"

### 4.2 Assign Thumbnails to LevelData
1. Open your LevelData assets
2. Drag the appropriate screenshot to the "Preview Image" field
3. The thumbnail will automatically appear in level selection

## Step 5: Testing Your Setup

### 5.1 Test Level Discovery
1. Play the Game scene
2. Check the Console for LevelManager debug messages
3. Verify all levels are discovered correctly

### 5.2 Test Level Selection
1. Go to your level selection screen
2. Verify buttons appear for all levels
3. Check that thumbnails and goal text display correctly
4. Test clicking buttons to load levels

### 5.3 Use Test Component
1. Add the `LevelSystemTest` component to a GameObject in your Game scene
2. Play the scene
3. Check Console output for system validation
4. Use the context menu "Run Level System Test" for manual testing

## Step 6: Level Naming Conventions

For automatic ordering, use these naming patterns:

### Tutorials:
- `Moving Tutorial.unity`
- `Button Tutorial.unity`
- `Speed Tutorial.unity`

### Levels:
- `01_First Level.unity`
- `02_Second Level.unity`
- `Level One.unity` (will be ordered as 1)
- `Level Two.unity` (will be ordered as 2)

## Common Issues and Solutions

### Issue: Levels appear locked
**Solution**: 
- Check LevelData "Requires Unlock" setting
- For testing, LevelManager is set to unlock all levels by default
- Verify LevelManager.CanLoadLevel() logic

### Issue: Thumbnails not showing
**Solution**:
- Ensure Preview Image is assigned in LevelData
- Check sprite import settings (Texture Type: Sprite 2D and UI)
- Verify DynamicLevelButton has previewImage assigned

### Issue: Wrong level order
**Solution**:
- Use numeric prefixes in scene names (01_, 02_, etc.)
- Or set Sort Order in LevelData assets
- Check LevelManager debug output for discovered order

### Issue: Buttons not working
**Solution**:
- Ensure DynamicLevelButton component is on button prefab
- Check that Button component is assigned in DynamicLevelButton
- Verify scenes are added to Build Settings

### Issue: Old buttons conflicting
**Solution**:
- Remove old hardcoded level buttons from UI
- Or disable SceneButton components on old buttons
- Use only DynamicLevelSelector for new system

## Example Complete Setup

Here's a complete example for "Level One":

1. **Scene**: `Assets/Scenes/Levels/Level One.unity`
2. **LevelData**: `Assets/Data/Level One Data.asset`
   - Level Title: "First Challenge"
   - Sort Order: 1
   - Par Moves: 15
   - Par Time: 60
   - Preview Image: LevelOneScreenshot.png
3. **Build Settings**: Scene at index 4
4. **Result**: Button shows "First Challenge", "Par: 15 moves | Time: 01:00", with screenshot thumbnail

This setup ensures your level integrates perfectly with the Dynamic Level Management System!