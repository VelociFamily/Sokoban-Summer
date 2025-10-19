# Level Button Visual Polish Guide

Bring cohesion to the level-select screen by pairing charming textures with a thoughtfully structured button prefab. This guide walks through art direction, asset creation, and Unity setup so the Dynamic Level Selector feels as friendly as the rest of Sokoban Summer.

---

## 1. Art Direction Snapshot

- **Mood:** Bright, breezy, and optimistic—match the sky background with soft gradients and rounded shapes.
- **Palette:** Use the existing UI accent greens (`#4BDB67` highlight, `#2FB54E` base) plus off-white (`#FBFFFD`) and midnight outline (`#0C1F35`). Keep saturation high but introduce one desaturated neutral for contrast.
- **Surface Style:** Embrace subtle pixel or painterly textures—avoid flat solid rectangles. Introduce light inner shadows and rim highlights to imply depth without heavy bevels.
- **Hierarchy:** Players should read completion state > level name > challenge info > preview art at a glance.

---

## 2. Texture Creation Workflow

1. **Canvas & Resolution**
   - Work at 256×128 px (2× the recommended 200×100 button size) so you can downsample cleanly.
   - Enable a 16 px grid; align corners to whole grid coordinates.

2. **Base Layer**
   - Block in the capsule or rounded rectangle shape.
   - Add a 4–6 px outer stroke using the darker accent color.
   - Introduce a vertical gradient lightening towards the top to suggest sunlight.

3. **Detail Pass**
   - Paint subtle cloudlike speckles or diagonal hatching at <20% opacity.
   - Add tiny highlights (2–3 px tall) along the top rim.
   - For pixel-art workflow, keep clusters 2×2 px minimum to prevent shimmer.

4. **State Variants**
   - Duplicate the base texture for **Normal**, **Highlighted**, **Pressed**, and **Disabled** states.
   - Highlighted: Increase brightness +8%, add a thin glowing rim.
   - Pressed: Shift gradient down, add inner shadow at the top.
   - Disabled: Desaturate and reduce contrast 40%; ensure text sits on readable grey.

5. **Export**
   - Save as PNG with transparent background. Name using suffixes: `LevelButton_Base.png`, `LevelButton_Highlight.png`, etc.
   - Store in `Assets/Textures/UI/LevelButtons/`.

6. **Tooling Tips**
   - **Aseprite**: Use slices for nine-slicing later.
   - **Photoshop/Krita**: Lock transparency before adding lighting to avoid edge halos.
   - **Affinity**: Group variants inside an artboard so you can export all at once via slices.

---

## 3. Thumbnail & Icon Textures

- Capture level thumbnails in Play Mode; use `Game View` screenshots cropped to 96×96 px.
- Apply a gentle vignette (multiply layer) to keep text readable.
- For lock icons, create a 64×64 px sprite with solid silhouette + lighter rim so it reads at small sizes.
- Consider badge icons (star, checkmark) to overlay when a level is perfected—stick to a golden yellow (`#FFDC5A`).

---

## 4. Unity Import Settings Checklist

For each texture (select in Project window, then Inspector):

| Setting | Value |
| --- | --- |
| Texture Type | **Sprite (2D and UI)** |
| Sprite Mode | Single (or Multiple if using a sprite sheet) |
| Pixels Per Unit | 100 (matches default UI scale) |
| Mesh Type | Tight |
| Filter Mode | Point (pixel look) or Bilinear (soft) depending on art style |
| Compression | None (preserve crisp colors) |
| Mip Maps | Disabled |
| Sprite Editor | Define a 16 px border when you want nine-slice scaling |

Remember to apply after editing borders so the button can scale without stretching corners.

---

## 5. Prefab Layout Blueprint

Start from the existing setup in [`LEVEL_BUTTON_SETUP_GUIDE.md`](./LEVEL_BUTTON_SETUP_GUIDE.md) and add the styling elements below.

```
Level Button (Button + Image + DynamicLevelButton)
├── Background (Image)          — renders Base sprite, handles ColorTint swaps
├── Frame Highlight (Image)     — additive blend for hover glow
├── Content Wrapper (RectTransform)
│   ├── Preview Image (Image)   — 96×96 thumbnail, anchored left
│   ├── Text Column (RectTransform)
│   │   ├── Level Name (TMP_Text)
│   │   └── Goal Text (TMP_Text)
├── Completion Badge (Image)    — optional star/check overlay
└── Lock Overlay (Image + CanvasGroup)
```

### Key Settings
- **Background Image**: Assign Base sprite. `Preserve Aspect` off, `Image Type` = Sliced.
- **Frame Highlight**: Use Highlight sprite, `Image Type` = Sliced, set Color to white and `Material` to `UI/Particles/Additive` for a soft glow. Toggle via Animator or `Selectable` transition.
- **Content Wrapper**: Padding of 24 left/right, 16 top/bottom to keep text away from borders. Use `HorizontalLayoutGroup` to line up preview + text.
- **Level Name**: TextMeshProUGUI, font size 32, weight semi-bold, color `#0C1F35`.
- **Goal Text**: TMP, size 22, color `#103A63`, with uppercase labels (`Par`, `Best Time`).
- **Lock Overlay**: Full stretch anchors, assign semi-transparent dark sprite (RGBA 0, 16, 32, 180). Include `Image` for tint + child lock icon centered.

---

## 6. Setting Up Transitions

1. **Selectable Colors** (on the Button component):
   - Normal: `#FFFFFF` (no tint)
   - Highlighted: `#E4FFEF`
   - Pressed: `#C7EED6`
   - Disabled: `#8FAAA0`
2. Bind the Button to change the Background and Frame Highlight colors via the built-in `Color Tint` transition, or drive sprite swaps with an Animator.
3. For the locked state, set the Button to `interactable = false` from `DynamicLevelButton`; the disabled color palette will apply automatically.

---

## 7. Texture Variations for Progress Feedback

- **Unlocked & New:** Add a subtle animated sparkle overlay (particle system or animated sprite) that plays once when unlocked.
- **Completed:** Swap the Background sprite to a teal-blue variant and show the Completion Badge.
- **Perfect Run:** Overlay a thin golden outline and set Goal Text color to match the badge.

These cues can be toggled by extending `DynamicLevelButton` to expose `SetCompletionState()` style methods.

---

## 8. Workflow for Creating Matching Button Sets

1. Design the Normal state first.
2. Derive Highlighted and Pressed by adjusting curves, not repainting from scratch.
3. Export all variants together to guarantee alignment.
4. Test them inside a sample Canvas with the `Game` view scaling to 1920×1080 and 1280×720 to ensure readability across resolutions.

---

## 9. Quick Reference Checklist

- [ ] Palette and gradients complement the background sky.
- [ ] Corners stay crisp thanks to nine-slice borders.
- [ ] Text remains legible at 80% scale.
- [ ] Lock overlay and badges read clearly at a glance.
- [ ] Texture import settings match the table above.
- [ ] Prefab uses layout groups so the Dynamic Level Selector can paginate without manual tweaks.
- [ ] Button transition states show obvious feedback without overpowering the thumbnail.

Follow this guide alongside the existing setup instructions and your level select screen will feel polished, readable, and inviting.
