# Step-by-Step Guide: Creating a New Level (Level 4+)

This guide provides detailed instructions for creating a new gameplay level in Sokoban Summer. Follow these steps to create **Level Three**, **Level Four**, or any subsequent level.

## Prerequisites

Before you begin, ensure:
- Unity Editor is open with the Sokoban Summer project
- You have Level One and Level Two as reference examples
- The Dynamic Level Management System is already set up (it is!)

---

## Part 1: Create the Level Scene

### Step 1.1: Duplicate an Existing Level Scene

The easiest way to start is by copying an existing level:

1. Navigate to `Assets/_Project/SokobanSummer/Scenes/Levels/` in the Project window
2. Right-click on **Level Two.unity**
3. Select **Duplicate** (or press `Ctrl+D`)
4. Rename the duplicated scene to **`Level Three.unity`** (or Level Four, Five, etc.)
5. Double-click to open the new scene

> **Naming Convention**: Use "Level Three", "Level Four", etc. The system will automatically order them.

### Step 1.2: Configure the Scene Hierarchy

With your new level scene open:

1. **Locate the Level Manager GameObject**
   - Look for a GameObject named "Level Manager" or "Scene Info" in the hierarchy
   - If it doesn't exist, create one: Right-click in Hierarchy → Create Empty → Name it "Level Manager"

2. **Verify/Add SceneInfo Component**
   - Select the Level Manager GameObject
   - In the Inspector, ensure it has a `SceneInfo` component
   - If not, click **Add Component** → Search for "SceneInfo" → Add it

3. **Configure SceneInfo Settings**
   - **Scene Type**: `GameplayLevel` (dropdown)
   - **Show Background**: `false` (unchecked) - gameplay levels typically don't show the animated background
   - **Level Number**: `3` (for Level Three; use 4 for Level Four, etc.)
   - **Level Data**: Leave empty for now (we'll link this in Part 2)

### Step 1.3: Design Your Level Layout

Now make the level unique:

1. **Modify the level geometry**
   - Move/add/remove walls, floors, and obstacles
   - Adjust box positions and goal locations
   - Ensure the level is solvable!

2. **Required GameObjects** (should already exist from duplication):
   - **Main Camera** - positioned to show the entire playable area
   - **Directional Light** - provides lighting
   - **Player** - starting position for the player
   - **Boxes** - crates the player must push
   - **Goals** - target positions for boxes
   - **Walls/Floor** - level boundaries and structure

3. **Test playability** (optional but recommended):
   - Press Play in the Unity Editor
   - Verify the level can be completed
   - Adjust difficulty by changing box/goal positions

### Step 1.4: Save the Scene

1. **Save your changes**: `Ctrl+S` or File → Save
2. **Add to Build Settings**:
   - Go to **File → Build Settings**
   - Click **Add Open Scenes** (if not already added)
   - Verify the scene appears in the list
   - Note: Scene order doesn't matter much (the system uses Level Number from SceneInfo)

---

## Part 2: Create the LevelData Asset

LevelData is a ScriptableObject that stores metadata about your level (title, goals, thumbnail, etc.)

### Step 2.1: Create the LevelData Asset

1. **Navigate to the Data folder**
   - Project window: `Assets/_Project/SokobanSummer/Data/`
   - If a "Levels" subfolder doesn't exist, create one:
     - Right-click → Create → Folder → Name it "Levels"

2. **Create the LevelData asset**
   - Right-click in `Assets/_Project/SokobanSummer/Data/Levels/`
   - Select **Create → Sokoban → Level Data**
   - Name it **`Level Three Data`** (match your scene name for consistency)

### Step 2.2: Configure LevelData Properties

Select your newly created LevelData asset and fill in the Inspector:

#### Required Fields:

- **Level Title**: `"First Challenge"` or `"Level Three"` or any descriptive name
  - This is what players see in the level selection menu

- **Description**: `"Navigate around obstacles to reach the goal."`
  - Brief description of what makes this level unique

- **Sort Order**: `3` (for Level Three; 4 for Level Four, etc.)
  - Determines the order in the level selection menu
  - Must match or correlate with your Level Number in SceneInfo

#### Optional but Recommended:

- **Par Moves**: `20` (example)
  - Target number of moves for completing the level
  - Set to `0` if you don't want to show a move goal
  - Players earn achievements by beating par

- **Par Time**: `60` (example - means 60 seconds)
  - Target completion time in seconds
  - Set to `0` if you don't want to show a time goal

- **Preview Image**: Drag a sprite/texture here
  - Shows a thumbnail of the level in the selection menu
  - See **Part 3** for creating thumbnails

- **Requires Unlock**: `false` (unchecked for testing)
  - Set to `true` if the level should be locked initially
  - Unlocking is typically based on completing previous levels
  - For testing, leave as `false`

- **Scene Path**: Leave empty
  - This is auto-populated by the system at runtime

### Step 2.3: Link LevelData to Scene

Now connect the LevelData asset to your scene:

1. **Open your level scene** (`Level Three.unity`)
2. **Select the Level Manager GameObject**
3. In the Inspector, find the **SceneInfo** component
4. **Drag your LevelData asset** (`Level Three Data`) into the **Level Data** field
5. **Save the scene** (`Ctrl+S`)

---

## Part 3: Create a Level Thumbnail (Optional but Recommended)

Thumbnails make your level selection menu look professional and help players identify levels.

### Step 3.1: Capture a Screenshot

1. **Open your level scene** in Unity
2. **Enter Play Mode** (press Play button)
3. **Position the camera** to show the entire level clearly
4. **Take a screenshot**:
   - Click on the **Game view** window
   - Right-click anywhere in the Game view
   - Select **Save Image** (or press a screenshot hotkey)
   - Save as `LevelThree_Preview.png` in a memorable location

5. **Exit Play Mode**

### Step 3.2: Import the Screenshot

1. **Import to Unity**:
   - Create/navigate to `Assets/_Project/SokobanSummer/Textures/LevelPreviews/`
   - Drag your screenshot PNG file into this folder

2. **Configure Import Settings**:
   - Select the imported image in the Project window
   - In the Inspector, set:
     - **Texture Type**: `Sprite (2D and UI)`
     - **Max Size**: `512` or `1024` (balances quality and file size)
   - Click **Apply**

### Step 3.3: Assign to LevelData

1. **Open your LevelData asset** (`Level Three Data`)
2. **Drag the sprite** into the **Preview Image** field
3. The thumbnail will now appear in the level selection menu!

---

## Part 4: Testing Your New Level

### Step 4.1: Verify Level Discovery

1. **Open the Game scene** (`Assets/_Project/SokobanSummer/Scenes/Game.unity`)
2. **Enter Play Mode**
3. **Check the Console** for LevelManager messages:
   - Look for: `"Discovered level: Level Three"` or similar
   - If you see this, the system found your level!
4. **Exit Play Mode**

### Step 4.2: Test from Level Selection

1. **Enter Play Mode** in the Game scene
2. **Navigate to the level selection screen**:
   - Click through Main Menu to Level Select
3. **Verify your level button appears**:
   - Should show your Level Title
   - Should show Par Moves/Time if you set them
   - Should show thumbnail if you created one
4. **Click the level button** to load your level
5. **Play through and test**:
   - Verify level is completable
   - Check that Move Counter and Timer work
   - Ensure the completion screen appears when you win

### Step 4.3: Troubleshooting

If your level doesn't appear:

- ✅ **Check Build Settings**: Scene must be added (File → Build Settings)
- ✅ **Check SceneInfo**: Level Number and Scene Type must be set
- ✅ **Check Console**: Look for errors or warnings from LevelManager
- ✅ **Check Scene Name**: Must end with "Three", "Four", or use numeric prefix (03_, 04_)
- ✅ **Restart Play Mode**: Sometimes the LevelManager needs a fresh start

If the level button looks wrong:

- ✅ **Check LevelData fields**: Ensure Title, Sort Order, and other fields are filled
- ✅ **Re-link LevelData**: Make sure SceneInfo → Level Data field is assigned
- ✅ **Check thumbnail**: Preview Image should reference a valid sprite

---

## Part 5: Quick Reference Checklist

Use this checklist when creating future levels:

### Scene Setup:
- [ ] Duplicate existing level scene or create new scene
- [ ] Rename to "Level [Number].unity" (e.g., "Level Four.unity")
- [ ] Add/configure SceneInfo component on a Level Manager GameObject
  - [ ] Scene Type = `GameplayLevel`
  - [ ] Show Background = `false`
  - [ ] Level Number = correct sequential number
- [ ] Design level layout (walls, boxes, goals, player start)
- [ ] Add scene to Build Settings
- [ ] Save scene

### LevelData Asset:
- [ ] Create LevelData asset in `Data/Levels/`
- [ ] Name it "[Level Name] Data" (e.g., "Level Four Data")
- [ ] Configure properties:
  - [ ] Level Title (display name)
  - [ ] Description (brief text)
  - [ ] Sort Order (sequential number)
  - [ ] Par Moves (optional)
  - [ ] Par Time (optional)
  - [ ] Preview Image (optional)
  - [ ] Requires Unlock (false for testing)
- [ ] Link LevelData to Scene (SceneInfo → Level Data field)

### Thumbnail (Optional):
- [ ] Play scene and capture screenshot
- [ ] Import to `Textures/LevelPreviews/`
- [ ] Set Texture Type to "Sprite (2D and UI)"
- [ ] Assign to LevelData Preview Image field

### Testing:
- [ ] Play Game scene and check Console for level discovery
- [ ] Navigate to level selection and verify button appears
- [ ] Click button to load level
- [ ] Complete level and verify win condition

---

## Example: Creating Level Four

Following this guide to create Level Four:

1. **Scene**: Duplicate `Level Three.unity` → Rename to `Level Four.unity`
2. **SceneInfo**: Level Number = `4`, Scene Type = `GameplayLevel`
3. **LevelData**: Create `Level Four Data.asset` in `Data/Levels/`
   - Level Title: `"Underground Maze"`
   - Description: `"Find your way through the twisting passages."`
   - Sort Order: `4`
   - Par Moves: `35`
   - Par Time: `90`
4. **Thumbnail**: Capture screenshot → Import → Assign to Preview Image
5. **Test**: Play Game scene → Navigate to Level Select → Click "Underground Maze"

---

## Additional Resources

- **LEVEL_CONVERSION_GUIDE.md** - Original system documentation
- **DYNAMIC_LEVEL_SYSTEM_GUIDE.md** - Detailed technical information
- **Level One.unity** and **Level Two.unity** - Reference examples

---

## Tips for Level Design

### Difficulty Progression
- **Level 1-2**: Simple tutorials (few boxes, obvious solutions)
- **Level 3-5**: Introduce complexity (multiple boxes, timing)
- **Level 6+**: Advanced challenges (tight spaces, multi-step solutions)

### Par Goals
- **Par Moves**: Count the optimal solution + 2-5 extra moves for flexibility
- **Par Time**: Test yourself playing at a moderate pace, add 10-20 seconds

### Testing
- Always test your levels yourself before releasing
- Ensure there's no way to make the level unwinnable (box stuck in corner)
- Verify the level can be completed within Par goals

---

**You're ready to create amazing levels! Start with Level Three and expand from there.**
