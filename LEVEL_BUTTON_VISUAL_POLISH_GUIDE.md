# Level Button Visual Polish Guide

Bring cohesion to the level-select screen by pairing charming textures with a thoughtfully structured button prefab. This guide walks through art direction, asset creation, and Unity setup so the Dynamic Level Selector feels as friendly as the rest of Sokoban Summer.

---

## 0. Layout Mode & Section Headers Policy

### When Section Headers Appear

The Dynamic Level Selector supports two layout modes: **Vertical List** and **Grid**.

- **Vertical List Mode** (`layoutMode = LayoutMode.VerticalList`):
  - Section headers ("Tutorials" / "Levels") **are supported** and appear when `addSectionHeaders = true`
  - Headers are instantiated from `sectionHeaderPrefab` and inserted before each section
  - Use case: Traditional scrolling list where clear section dividers enhance scanability
  - Headers respect the configured `headerHeight` and spacing settings

- **Grid Mode** (`layoutMode = LayoutMode.Grid`):
  - Section headers are **NOT rendered** regardless of `addSectionHeaders` setting
  - Rationale: Grid pagination breaks sequential flow; headers between pages would be confusing
  - Players browse pages of mixed content (tutorials + levels) without section breaks
  - Use case: Modern card-based UI with pagination controls for navigation

### Configuration Guidelines

In the `DynamicLevelSelector` inspector:
1. Set `Layout Mode` to control the layout style
2. Set `Add Section Headers` to `true` for Vertical List if you want section dividers
3. Assign a `Section Header Prefab` (a GameObject with a Text/TextMeshPro component showing the section name)
4. Configure `Header Height` to match your header prefab's visual design

**Recommendation**: Use Vertical List with headers for smaller level sets (< 15 levels) where players benefit from clear categorization. Use Grid mode for larger level sets (15+ levels) where pagination provides better performance and modern visual appeal.

---

## 1. Art Direction Snapshot

- **Mood:** Bright, breezy, and optimistic—match the sky background with soft gradients and rounded shapes.
- **Palette:** Use the existing UI accent greens (`#4BDB67` highlight, `#2FB54E` base) plus off-white (`#FBFFFD`) and midnight outline (`#0C1F35`). Keep saturation high but introduce one desaturated neutral for contrast.
- **Surface Style:** Embrace subtle pixel or painterly textures—avoid flat solid rectangles. Introduce light inner shadows and rim highlights to imply depth without heavy bevels.
- **Hierarchy:** Players should read completion state > level name > challenge info > preview art at a glance.

---

## 2. Font & Background Cohesion

Choosing the right typography ensures your level select screen feels intentional and readable.

### Font Style Decision

**Pixel Fonts** (retro/nostalgic aesthetic):
- **When to use**: Match pixel-art backgrounds, 8/16-bit inspired themes, or when targeting a cozy retro feel
- **Best practices**:
  - Use multiples of the font's native pixel size (e.g., 8px base → use 16, 24, 32)
  - Set TextMeshPro to `Point` filtering to keep sharp edges
  - Ensure at least 3px contrast between text and background for legibility
  - Test at 1280×720 and 1920×1080 to avoid sub-pixel shimmer
- **Examples**: Press Start 2P, Silkscreen, PxPlus IBM VGA8

**Modern Fonts** (clean/polished aesthetic):
- **When to use**: Smooth gradients, photo-realistic backgrounds, or when targeting broad accessibility
- **Best practices**:
  - Use `Bilinear` or `Trilinear` filtering for smooth anti-aliasing
  - Font size 18+ for body text, 28+ for headers (tested at 1080p)
  - Use SDF (Signed Distance Field) rendering in TextMeshPro for crisp scaling
  - Add subtle drop shadow or outline for readability on busy backgrounds
- **Examples**: Roboto, Lato, Nunito, Open Sans

### Background Compatibility

Your button textures and level preview backgrounds must work harmoniously:

1. **Contrast Check**:
   - Level name text should have 4.5:1 contrast ratio minimum (WCAG AA standard)
   - Use semi-transparent overlays on preview images if text overlaps thumbnails
   - Test against both light sky backgrounds and dark level screenshots

2. **Visual Weight**:
   - Pixel fonts pair well with flat color backgrounds and low-detail textures
   - Modern fonts pair well with gradients, photos, and high-detail environments
   - Avoid mixing: pixel font on photo-realistic background feels mismatched

3. **Color Temperature**:
   - Warm backgrounds (yellows, oranges) → use cool text colors (blues, teals)
   - Cool backgrounds (blues, purples) → use warm text colors (oranges, yellows) or high-contrast white
   - Sokoban Summer's sky blue background → recommended text: midnight blue (`#0C1F35`), off-white (`#FBFFFD`)

### Practical Examples

**Scenario A: Pixel art with retro font**
- Background: Flat sky gradient with pixel clouds
- Font: Press Start 2P at 16px (level name), 12px (goals)
- Button texture: Solid color with 2px border, no gradients
- Filter mode: Point (no smoothing)

**Scenario B: Modern clean with SDF font**
- Background: Smooth photo-realistic sky with sun rays
- Font: Roboto SDF at 28px (level name), 18px (goals)
- Button texture: Subtle gradient with soft inner shadow
- Filter mode: Bilinear, enable SDF rendering
- Add 1px black outline to text for clarity

### Quick Decision Flowchart

1. Does your game use pixel art sprites and tilesets?
   - **Yes** → Use pixel font + Point filtering
   - **No** → Continue to step 2

2. Do you have smooth gradients or photo textures in backgrounds?
   - **Yes** → Use modern font + Bilinear/SDF
   - **No** → Either works; prefer modern for accessibility

3. Test readability:
   - Can you read level names at 80% UI scale?
   - Can you read goal text without squinting?
   - Does the font complement (not clash with) button textures?

**If any answer is no**, adjust font size, add outlines/shadows, or increase background contrast.

---

## 3. Unity Sprite Import Settings Checklist

**Critical**: Proper import settings prevent blurriness, compression artifacts, and scaling issues. Apply these settings to **all** level select UI textures before using them in prefabs.

### Import Settings Table (Quick Reference)

| Setting | Value | Notes |
| --- | --- | --- |
| **Texture Type** | **Sprite (2D and UI)** | Enables UI rendering pipeline |
| **Sprite Mode** | Single (or Multiple for atlases) | Use Multiple only for sprite sheets |
| **Packing Tag** | `UI_LevelButtons` (optional) | Groups textures in atlas for performance |
| **Pixels Per Unit** | 100 | Matches Unity's default UI scale |
| **Mesh Type** | Tight | Reduces overdraw for non-rectangular sprites |
| **Extrude Edges** | 1 | Prevents edge bleeding |
| **Pivot** | Center | Simplifies positioning in UI |
| **Generate Mip Maps** | **Disabled** (unchecked) | UI textures don't need mipmaps |
| **Filter Mode** | **Point** (pixel art) or **Bilinear** (smooth) | See font guidance above |
| **Max Size** | 2048 (buttons/headers) / 512 (icons) | Balance quality vs memory |
| **Compression** | **None** (editor) / **High Quality** (build) | Preserve crisp colors for UI |
| **Alpha Source** | Input Texture Alpha | Preserve transparency |
| **Alpha Is Transparency** | **Enabled** (checked) | Required for proper blending |
| **Advanced → Read/Write** | Disabled | Saves memory unless scripting needs pixel data |

### Step-by-Step Import Workflow

1. **Import texture to project**:
   - Drag PNG files into `Assets/Textures/UI/LevelButtons/` folder
   - Unity auto-imports with default settings (usually wrong for UI)

2. **Select texture in Project window**:
   - Click the texture file to open Inspector

3. **Set Texture Type**:
   - Change from `Default` to `Sprite (2D and UI)`
   - Inspector updates to show sprite-specific settings

4. **Configure Basic Settings**:
   - Sprite Mode: `Single` (or `Multiple` if you're using a sprite sheet)
   - Pixels Per Unit: `100` (critical for consistent UI scaling)
   - Mesh Type: `Tight` (optimizes rendering for non-rectangular shapes)
   - Extrude Edges: `1` (prevents edge artifacts)
   - Pivot: `Center` (or `Custom` if you need specific alignment)

5. **Configure Filtering**:
   - **Pixel art** → Filter Mode: `Point (no filter)` → keeps sharp edges
   - **Smooth art** → Filter Mode: `Bilinear` → soft anti-aliasing

6. **Configure Compression**:
   - Compression: `None` (editor) or `High Quality` (build)
   - Format: `RGBA 32 bit` for transparency, `RGB 24 bit` if fully opaque

7. **Disable Mipmaps**:
   - Advanced section → Uncheck `Generate Mip Maps`
   - Mipmaps add memory overhead and aren't needed for fixed-resolution UI

8. **Configure Alpha**:
   - Alpha Source: `Input Texture Alpha`
   - Check `Alpha Is Transparency`

9. **Optional: Nine-Slice for Buttons**:
   - Click `Sprite Editor` button
   - Drag borders inward (typically 16-32px) to define stretchable regions
   - Apply and close editor
   - In Button prefab, set Image `Image Type` to `Sliced`

10. **Click Apply** (bottom of Inspector)

### Platform-Specific Overrides (Optional)

For optimized builds, configure per-platform compression:

- **PC/Mac Standalone**:
  - Override: Enabled
  - Max Size: 2048
  - Format: `RGBA Compressed DXT5` (transparent) or `RGB Compressed DXT1` (opaque)

- **Android**:
  - Override: Enabled
  - Max Size: 1024 (lower-end devices) or 2048 (high-end)
  - Format: `RGBA Compressed ETC2`

- **iOS**:
  - Override: Enabled
  - Max Size: 2048
  - Format: `RGBA Compressed ASTC 6x6 block`

### Common Import Mistakes

❌ **Leaving Texture Type as `Default`** → Sprite won't render in UI
❌ **Enabling mipmaps** → Wasted memory, no quality benefit for UI
❌ **Using `Trilinear` filtering** → Unnecessary for UI, uses more GPU
❌ **Max Size too low** → Blurry buttons on high-DPI displays
❌ **Compression too aggressive** → Color banding and artifacts
❌ **Forgetting to apply settings** → Changes don't take effect

### Validation Checklist (Before Use)

Before assigning textures to button prefabs, verify:
- [ ] Texture previews in Project window look crisp (no blur/artifacts)
- [ ] Transparency works correctly (semi-transparent edges blend smoothly)
- [ ] Textures import within memory budget (check Inspector → Preview → Size)
- [ ] Filter mode matches art style (Point for pixel art, Bilinear for smooth)
- [ ] Nine-slice borders are defined if using sliced Image components

### Bulk Import Tip

For large texture sets:
1. Import all textures first
2. Select all textures in Project window (Shift+Click or Ctrl+A in folder)
3. Apply common settings in Inspector (changes apply to all selected)
4. Fine-tune individual textures as needed

---

## 4. Texture Creation Workflow

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

## 5. Thumbnail & Icon Textures

- Capture level thumbnails in Play Mode; use `Game View` screenshots cropped to 96×96 px.
- Apply a gentle vignette (multiply layer) to keep text readable.
- For lock icons, create a 64×64 px sprite with solid silhouette + lighter rim so it reads at small sizes.
- Consider badge icons (star, checkmark) to overlay when a level is perfected—stick to a golden yellow (`#FFDC5A`).

---

## 6. Additional Unity Import Notes

The comprehensive import settings in **Section 3** cover most use cases. This section provides legacy notes for reference.

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

## 7. Prefab Layout Blueprint

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

## 8. Setting Up Transitions

1. **Selectable Colors** (on the Button component):
   - Normal: `#FFFFFF` (no tint)
   - Highlighted: `#E4FFEF`
   - Pressed: `#C7EED6`
   - Disabled: `#8FAAA0`
2. Bind the Button to change the Background and Frame Highlight colors via the built-in `Color Tint` transition, or drive sprite swaps with an Animator.
3. For the locked state, set the Button to `interactable = false` from `DynamicLevelButton`; the disabled color palette will apply automatically.

---

## 9. Texture Variations for Progress Feedback

- **Unlocked & New:** Add a subtle animated sparkle overlay (particle system or animated sprite) that plays once when unlocked.
- **Completed:** Swap the Background sprite to a teal-blue variant and show the Completion Badge.
- **Perfect Run:** Overlay a thin golden outline and set Goal Text color to match the badge.

These cues can be toggled by extending `DynamicLevelButton` to expose `SetCompletionState()` style methods.

---

## 10. Workflow for Creating Matching Button Sets

1. Design the Normal state first.
2. Derive Highlighted and Pressed by adjusting curves, not repainting from scratch.
3. Export all variants together to guarantee alignment.
4. Test them inside a sample Canvas with the `Game` view scaling to 1920×1080 and 1280×720 to ensure readability across resolutions.

---

## 11. Quick Reference Checklist

- [ ] Palette and gradients complement the background sky.
- [ ] Corners stay crisp thanks to nine-slice borders.
- [ ] Text remains legible at 80% scale.
- [ ] Lock overlay and badges read clearly at a glance.
- [ ] Texture import settings match the table above.
- [ ] Prefab uses layout groups so the Dynamic Level Selector can paginate without manual tweaks.
- [ ] Button transition states show obvious feedback without overpowering the thumbnail.

Follow this guide alongside the existing setup instructions and your level select screen will feel polished, readable, and inviting.
