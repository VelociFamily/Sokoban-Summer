"""
Generate simple placeholder sprites for LevelButton prefab.
Creates: background (9-slice), lock icon, completion badge, fallback thumbnail
"""
from PIL import Image, ImageDraw
import os
import sys

# Get script directory and project root
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PROJECT_ROOT = os.path.dirname(SCRIPT_DIR)
OUTPUT_DIR = os.path.join(PROJECT_ROOT, "Assets", "_Project", "SokobanSummer", "Sprites", "UI", "LevelButton")
os.makedirs(OUTPUT_DIR, exist_ok=True)

# Color palette from guide
BG_BASE = (75, 219, 103)  # #4BDB67
BG_HIGHLIGHT = (47, 181, 78)  # #2FB54E
TEXT_DARK = (12, 31, 53)  # #0C1F35
OFF_WHITE = (251, 255, 253)  # #FBFFFD
BADGE_GOLD = (255, 220, 90)  # #FFDC5A

def create_background(width=256, height=128):
    """Create 9-sliceable button background with gradient"""
    img = Image.new('RGBA', (width, height), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Draw gradient from top (lighter) to bottom (darker)
    for y in range(height):
        ratio = y / height
        r = int(BG_BASE[0] + (BG_HIGHLIGHT[0] - BG_BASE[0]) * ratio)
        g = int(BG_BASE[1] + (BG_HIGHLIGHT[1] - BG_BASE[1]) * ratio)
        b = int(BG_BASE[2] + (BG_HIGHLIGHT[2] - BG_BASE[2]) * ratio)
        draw.line([(0, y), (width, y)], fill=(r, g, b, 255))
    
    # Draw rounded rectangle outline
    border = 4
    draw.rounded_rectangle([border, border, width-border-1, height-border-1], 
                          radius=16, outline=TEXT_DARK, width=border)
    
    # Add highlight on top edge
    for i in range(3):
        draw.line([(border+8+i, border+4), (width-border-8-i, border+4)], 
                 fill=(*OFF_WHITE, 160-i*40))
    
    img.save(os.path.join(OUTPUT_DIR, "LevelButton_Base.png"))
    print(f"Created: LevelButton_Base.png")

def create_lock_icon(size=64):
    """Create simple lock icon"""
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Lock body (rectangle)
    body_h = size // 2
    body_w = int(size * 0.6)
    body_x = (size - body_w) // 2
    body_y = size - body_h - 4
    draw.rounded_rectangle([body_x, body_y, body_x + body_w, body_y + body_h], 
                          radius=4, fill=TEXT_DARK)
    
    # Shackle (arc)
    shackle_w = int(body_w * 0.7)
    shackle_h = int(size * 0.35)
    shackle_x = (size - shackle_w) // 2
    shackle_y = body_y - shackle_h
    draw.arc([shackle_x, shackle_y, shackle_x + shackle_w, body_y + 4], 
             180, 360, fill=TEXT_DARK, width=6)
    
    # Keyhole
    keyhole_r = 6
    keyhole_y = body_y + body_h // 3
    draw.ellipse([size//2 - keyhole_r, keyhole_y - keyhole_r, 
                  size//2 + keyhole_r, keyhole_y + keyhole_r], 
                 fill=(*OFF_WHITE, 200))
    
    img.save(os.path.join(OUTPUT_DIR, "LockIcon.png"))
    print(f"Created: LockIcon.png")

def create_completion_badge(size=64):
    """Create star/badge for completed levels"""
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Draw star shape
    center = size // 2
    outer_r = size // 2 - 4
    inner_r = outer_r // 2
    
    import math
    points = []
    for i in range(10):
        angle = math.pi * 2 * i / 10 - math.pi / 2
        r = outer_r if i % 2 == 0 else inner_r
        x = center + r * math.cos(angle)
        y = center + r * math.sin(angle)
        points.append((x, y))
    
    draw.polygon(points, fill=BADGE_GOLD, outline=TEXT_DARK, width=2)
    
    # Inner highlight
    draw.ellipse([center-8, center-8, center+8, center+8], 
                fill=(*OFF_WHITE, 100))
    
    img.save(os.path.join(OUTPUT_DIR, "CompletionBadge.png"))
    print(f"Created: CompletionBadge.png")

def create_fallback_thumbnail(size=96):
    """Create placeholder thumbnail for levels without preview"""
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Background
    draw.rounded_rectangle([0, 0, size-1, size-1], radius=8, 
                          fill=(150, 150, 150, 180))
    
    # Simple question mark or puzzle icon
    # Draw grid pattern
    grid_size = 3
    cell = size // grid_size
    for i in range(grid_size):
        for j in range(grid_size):
            if (i + j) % 2 == 0:
                x = i * cell + 8
                y = j * cell + 8
                draw.rectangle([x, y, x + cell - 16, y + cell - 16], 
                             fill=(*TEXT_DARK, 120))
    
    img.save(os.path.join(OUTPUT_DIR, "FallbackThumbnail.png"))
    print(f"Created: FallbackThumbnail.png")

def create_lock_overlay(width=256, height=128):
    """Create semi-transparent overlay for locked levels"""
    img = Image.new('RGBA', (width, height), (0, 16, 32, 180))
    img.save(os.path.join(OUTPUT_DIR, "LockOverlay.png"))
    print(f"Created: LockOverlay.png")

if __name__ == "__main__":
    print("Generating LevelButton sprites...")
    create_background()
    create_lock_icon()
    create_completion_badge()
    create_fallback_thumbnail()
    create_lock_overlay()
    print("\nAll sprites created successfully!")
    print(f"Location: {OUTPUT_DIR}")
