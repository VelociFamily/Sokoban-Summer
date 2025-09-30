# Level Button Prefab Setup Guide

## Creating the Perfect Level Button Prefab

This guide will help you create a level button prefab that works perfectly with the Dynamic Level Management System.

## Step-by-Step Prefab Creation

### 1. Create the Base Button
1. In your scene hierarchy: **UI → Button**
2. Rename it to "Level Button"
3. Set the RectTransform size (recommended: 200x100)

### 2. Configure the Main Button
Select the Button GameObject and:
- **Image**: Set your button background sprite
- **Transition**: Use ColorTint or SpriteSwap as preferred
- **Colors**: Set Normal, Highlighted, Pressed, Selected, Disabled colors

### 3. Add DynamicLevelButton Component
1. Select the Button GameObject
2. **Add Component → DynamicLevelButton**
3. The component will automatically find the Button component
4. Leave the Level Info field empty (this gets set automatically)

### 4. Create Child Elements

#### 4.1 Level Name Text
1. Right-click Button → **UI → Text**
2. Rename to "Level Name"
3. Configure:
   - **Text**: "Level Name" (placeholder)
   - **Font Size**: 18-24
   - **Alignment**: Center
   - **Color**: White or contrasting color
   - **Anchor**: Center
   - **Position**: Top portion of button

#### 4.2 Goal Text
1. Right-click Button → **UI → Text**
2. Rename to "Goal Text"
3. Configure:
   - **Text**: "Goals" (placeholder)
   - **Font Size**: 12-16
   - **Alignment**: Center
   - **Color**: Light gray
   - **Anchor**: Center
   - **Position**: Bottom portion of button

#### 4.3 Preview Image
1. Right-click Button → **UI → Image**
2. Rename to "Preview Image"
3. Configure:
   - **Source Image**: Leave empty
   - **Color**: White
   - **Raycast Target**: Unchecked
   - **Anchor**: Center or Left side
   - **Size**: 60x60 (or desired thumbnail size)

#### 4.4 Lock Overlay (Optional)
1. Right-click Button → **UI → Image**
2. Rename to "Lock Overlay"
3. Configure:
   - **Source Image**: Lock icon or dark overlay
   - **Color**: Semi-transparent black (0,0,0,128)
   - **Raycast Target**: Unchecked
   - **Anchor**: Stretch (covers entire button)
   - **Position**: Covers entire button area

### 5. Wire Up DynamicLevelButton Component
Select the Button GameObject and in the DynamicLevelButton component:
1. **Button**: Should auto-populate with the Button component
2. **Level Name Text**: Drag the "Level Name" text object here
3. **Goal Text**: Drag the "Goal Text" text object here
4. **Preview Image**: Drag the "Preview Image" object here
5. **Lock Overlay**: Drag the "Lock Overlay" object here (if created)

### 6. Example Layout Structure
```
Level Button (Button + DynamicLevelButton)
├── Level Name (Text) - "Challenge Level 1"
├── Goal Text (Text) - "Par: 15 moves | Time: 01:00"
├── Preview Image (Image) - Level screenshot
└── Lock Overlay (Image) - Shows when locked
```

### 7. Save as Prefab
1. Drag the Button from Hierarchy to Project window
2. Save in `Assets/Prefabs/UI/LevelButton.prefab`
3. Delete from scene hierarchy

## Example Button Appearance

When working correctly, your button should display:
- **Top**: Level name (e.g., "Moving Tutorial", "Challenge Level 1")
- **Bottom**: Goals (e.g., "Par: 15 moves | Time: 01:00")
- **Side/Background**: Level screenshot thumbnail
- **Overlay**: Lock icon when level is locked

## Common Setup Issues

### Issue: Button doesn't respond to clicks
**Solution**: 
- Ensure Button component is present and enabled
- Check that DynamicLevelButton found the Button component
- Verify Canvas has GraphicRaycaster component

### Issue: Text doesn't update
**Solution**:
- Check that Text components are assigned in DynamicLevelButton
- Ensure Text components are not null in inspector
- Verify scene names match expected patterns

### Issue: Images don't show
**Solution**:
- Confirm Preview Image is assigned in DynamicLevelButton
- Check that LevelData assets have Preview Image set
- Verify image import settings (Sprite, 2D and UI)

### Issue: Lock overlay not working
**Solution**:
- Ensure Lock Overlay GameObject is assigned
- Check that lock overlay covers the button area
- Verify CanLoadLevel() logic in LevelManager

## Testing Your Prefab

1. **In Scene**: Place your prefab in a test scene
2. **Add Test Data**: Temporarily set Level Info to test the display
3. **Play Mode**: Test button clicks and visual updates
4. **With DynamicLevelSelector**: Use the complete system to verify integration

## Advanced Customization

### Animation Support
Add Animator component for:
- Hover animations
- Click feedback
- Unlock animations
- Progress indicators

### Audio Integration
In DynamicLevelButton.LoadLevel(), add:
```csharp
// Play button click sound
AudioManager.Instance.PlayButtonClick();
```

### Visual Effects
Consider adding:
- Particle effects for unlocks
- Glow effects for completed levels
- Progress bars for partial completion

Your level button prefab is now ready to work perfectly with the Dynamic Level Management System!