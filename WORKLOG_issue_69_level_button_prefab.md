# Worklog Issue #69: Level Button Prefab Polish

Resolves #69 (will be referenced in PR once opened).

## Goal
Create a polished `LevelButton` prefab at `Assets/_Project/SokobanSummer/Prefabs/UI/LevelButton.prefab` with visual states (Locked / Unlocked / Completed) and proper layout, thumbnails, and badges per guides.

## Planned Implementation Steps
1. Import / prepare 9-slice background sprite (check LEVEL_BUTTON_VISUAL_POLISH_GUIDE.md for slicing) and ensure import settings (Sprite 2D, no mipmaps, Filter Mode Point, correct Pixels Per Unit).
2. Gather or create lock icon, completion badge sprite assets (optimize sizes, naming conventions) and place under `Assets/_Project/SokobanSummer/Sprites/UI/LevelButton/`.
3. Create prefab hierarchy:
   - Root `LevelButton` (with `RectTransform` + selectable component as needed)
   - Background Image (9-sliced)
   - Thumbnail Container (child Image with fallback sprite; maybe use a script for fallback referencing DynamicLevelButton data)
   - Text Wrapper (Vertical Layout Group) containing Level Name (TMP) and Goal Text (TMP)
   - Overlay Container: Lock Overlay (semi-transparent panel + lock icon), Completion Badge (badge icon positioned to a corner)
4. Hook `DynamicLevelButton` serialized fields to: thumbnail image, level name text, goal text text, lock overlay, completion badge.
5. Implement fallback thumbnail logic (if level thumbnail missing, use default placeholder sprite).
6. Verify readability & alignment at 1920x1080 and 1280x720: adjust anchors, padding, layout groups.
7. Test state transitions in Play Mode (simulate locked/unlocked/completed) ensuring no missing references or warnings.
8. Update any relevant documentation if new sprites or naming conventions were introduced.

## Acceptance Criteria Mapping
- Prefab at normalized path, loads with DynamicLevelButton without warnings.
- Distinct visuals for Locked/Unlocked/Completed.
- Text readability verified at target resolutions.
- Proper sprite import settings applied.

## Assets Needed
- Background 9-slice sprite
- Lock icon sprite
- Completion badge sprite
- Fallback thumbnail placeholder sprite

## Notes
- Ensure no additional AudioListener/EventSystem duplicates when testing in additive scenes.
- Maintain consistent naming: `LevelButton` root, `Thumbnail`, `LevelNameText`, `GoalText`, `LockOverlay`, `CompletionBadge`.

## Checklist
- [ ] Background 9-slice prepared
- [ ] Icons imported & optimized
- [ ] Prefab hierarchy built
- [ ] Serialized references linked
- [ ] Fallback thumbnail implemented
- [ ] Visual states tested
- [ ] Documentation updated
