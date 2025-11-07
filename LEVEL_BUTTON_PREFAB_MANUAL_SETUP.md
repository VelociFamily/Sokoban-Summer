# LevelButton Prefab Manual Setup Instructions

This guide walks through creating the polished `LevelButton` prefab manually in Unity Editor.

## Prerequisites
- Sprites generated in `Assets/_Project/SokobanSummer/Sprites/UI/LevelButton/`:
  - `LevelButton_Base.png` - 9-sliced background
  - `LockIcon.png` - Lock icon for locked states
  - `CompletionBadge.png` - Star/badge for completed levels
  - `FallbackThumbnail.png` - Placeholder when no preview available
  - `LockOverlay.png` - Semi-transparent overlay

## Step 1: Configure Sprite Import Settings

For each sprite in the LevelButton folder:

1. Select sprite in Project window
2. In Inspector, set:
   - **Texture Type**: `Sprite (2D and UI)`
   - **Sprite Mode**: `Single`
   - **Pixels Per Unit**: `100`
   - **Filter Mode**: `Point` (for pixel-perfect)
   - **Compression**: `None`
   - **Generate Mip Maps**: Disabled

3. **For `LevelButton_Base.png` only**: Click "Sprite Editor"
   - Set Border values: L: 16, R: 16, T: 16, B: 16
   - Click "Apply"

4. Click "Apply" for each sprite

## Step 2: Create Prefab Hierarchy

### Root GameObject
1. In Hierarchy: Right-click → UI → Button (creates Canvas if needed)
2. Rename to `LevelButton`
3. Set RectTransform Size: Width: `400`, Height: `120`
4. Add Component → `DynamicLevelButton` script
5. On Button component, configure ColorBlock:
   - Normal Color: `#FFFFFF`
   - Highlighted Color: `#E4FFEF`
   - Pressed Color: `#C7EED6`
   - Disabled Color: `#8FAAA0`

### Child 1: Background (9-sliced)
1. Right-click LevelButton → Create Empty
2. Rename to `Background`
3. Add Component → Image
4. Assign `LevelButton_Base` sprite
5. Set Image Type to `Sliced`
6. Stretch anchors to fill parent (Anchor Presets → stretch/stretch)

### Child 2: ContentWrapper (Layout Container)
1. Right-click LevelButton → Create Empty
2. Rename to `ContentWrapper`
3. Set anchors to stretch/stretch
4. Set Offsets: Left: `20`, Right: `-20`, Top: `-16`, Bottom: `16`
5. Add Component → Horizontal Layout Group
   - Child Alignment: Middle Left
   - Spacing: `16`
   - Uncheck all Control/Force Expand options

### Child 2a: PreviewImage (under ContentWrapper)
1. Right-click ContentWrapper → UI → Image
2. Rename to `PreviewImage`
3. Set Size: Width: `80`, Height: `80`
4. Assign `FallbackThumbnail` sprite (default)
5. Check "Preserve Aspect"

### Child 2b: TextColumn (under ContentWrapper)
1. Right-click ContentWrapper → Create Empty
2. Rename to `TextColumn`
3. Set Size: Width: `250`, Height: `80`
4. Add Component → Vertical Layout Group
   - Child Alignment: Upper Left
   - Spacing: `4`
   - Check "Control Width", uncheck others

### Child 2b-i: LevelNameText (under TextColumn)
1. Right-click TextColumn → UI → Text - TextMeshPro
2. Rename to `LevelNameText`
3. Set Text: `Level Name`
4. Font Size: `28`
5. Font Style: `Bold`
6. Color: `#0C1F35`
7. Alignment: Left

### Child 2b-ii: GoalText (under TextColumn)
1. Right-click TextColumn → UI → Text - TextMeshPro
2. Rename to `GoalText`
3. Set Text: `Par: 10 moves`
4. Font Size: `18`
5. Color: `#103A63`
6. Alignment: Left

### Child 3: CompletionBadge
1. Right-click LevelButton → UI → Image
2. Rename to `CompletionBadge`
3. Set Anchors: Top-Right (preset)
4. Position: X: `-20`, Y: `-20`
5. Size: Width: `48`, Height: `48`
6. Assign `CompletionBadge` sprite
7. Check "Preserve Aspect"
8. **Disable GameObject** (shown only when completed)

### Child 4: LockOverlay
1. Right-click LevelButton → UI → Image
2. Rename to `LockOverlay`
3. Stretch anchors to fill parent
4. Assign `LockOverlay` sprite (or set Color to `#00102080`)
5. **Disable GameObject** (shown only when locked)

### Child 4a: LockIcon (under LockOverlay)
1. Right-click LockOverlay → UI → Image
2. Rename to `LockIcon`
3. Set Anchors: Center
4. Position: X: `0`, Y: `0`
5. Size: Width: `48`, Height: `48`
6. Assign `LockIcon` sprite
7. Check "Preserve Aspect"

## Step 3: Wire DynamicLevelButton References

Select the root `LevelButton` GameObject. In the `DynamicLevelButton` component:

1. **Button**: Drag the root GameObject (or auto-filled)
2. **Level Name Text**: Drag `LevelNameText` child
3. **Goal Text**: Drag `GoalText` child
4. **Preview Image**: Drag `PreviewImage` child
5. **Fallback Thumbnail**: Assign `FallbackThumbnail` sprite asset
6. **Lock Overlay**: Drag `LockOverlay` GameObject
7. **Lock Overlay Image**: Drag `LockIcon` Image component
8. **Completion Badge**: Drag `CompletionBadge` GameObject

## Step 4: Save as Prefab

1. Create folder structure if needed: `Assets/_Project/SokobanSummer/Prefabs/UI/`
2. Drag `LevelButton` from Hierarchy to the UI folder
3. Prefab saved at: `Assets/_Project/SokobanSummer/Prefabs/UI/LevelButton.prefab`

## Step 5: Test in Scene

1. Ensure a `DynamicLevelSelector` component exists in your scene
2. Assign the `LevelButton` prefab to its `Button Prefab` field
3. Enter Play Mode
4. Verify:
   - Locked/Unlocked states work
   - Completion badge appears for completed levels
   - Text is readable at different resolutions
   - Fallback thumbnail shows when no preview available

## Alternative: Use Editor Script

If Unity MCP connection is available, you can also run:
- Menu: `Tools → Sokoban Summer → Create Level Button Prefab`
- This automates the entire setup

## Troubleshooting

- **Missing sprites**: Ensure all sprites are imported and located in the correct folder
- **Layout issues**: Check Layout Group settings and RectTransform anchors
- **References not wired**: Double-check Step 3; missing refs cause NullReferenceException
- **Badge not showing**: CompletionBadge GameObject is disabled by default; UpdateCompletionBadge() enables it
- **Lock not showing**: LockOverlay GameObject is disabled by default; UpdateLockState() controls it
