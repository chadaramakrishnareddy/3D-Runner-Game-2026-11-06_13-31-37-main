import math, random
from PIL import Image, ImageDraw, ImageFilter

out_dir = 'Assets/Materials/Textures'

# 1. DesertRoad_Albedo.png (512x512)
road = Image.new('RGB', (512, 512), (185, 140, 95))
draw = ImageDraw.Draw(road)

# Stone tile pavement pattern
for y in range(0, 512, 32):
    offset = 16 if (y // 32) % 2 == 1 else 0
    for x in range(-offset, 512 + 32, 64):
        shade = random.randint(-12, 12)
        r = max(0, min(255, 195 + shade))
        g = max(0, min(255, 148 + shade))
        b = max(0, min(255, 102 + shade))
        draw.rectangle([x + 2, y + 2, x + 62, y + 30], fill=(r, g, b), outline=(150, 110, 75))

# Shaded lane dividers (3 lanes: left ~85, center ~256, right ~427)
# Lane dividing line 1 at x = 170
# Lane dividing line 2 at x = 342
for y in range(0, 512, 64):
    # Bright dashed runner stripes
    draw.rectangle([165, y + 10, 175, y + 54], fill=(255, 230, 80), outline=(210, 170, 40))
    draw.rectangle([337, y + 10, 347, y + 54], fill=(255, 230, 80), outline=(210, 170, 40))

# Edge borders / curbs shading
for i in range(16):
    alpha = int(255 * (1.0 - i / 16.0) * 0.4)
    draw.line([(i, 0), (i, 512)], fill=(120, 80, 50))
    draw.line([(511 - i, 0), (511 - i, 512)], fill=(120, 80, 50))

road.save(f'{out_dir}/DesertRoad_Albedo.png')

# 2. DesertSand_Albedo.png (512x512)
sand = Image.new('RGB', (512, 512), (235, 185, 115))
draw = ImageDraw.Draw(sand)

# Sand wind ripples
for y in range(0, 512, 18):
    for x in range(512):
        wave = math.sin(x * 0.05 + y * 0.1) * 8.0
        val = int(20 * math.sin(x * 0.08))
        r = max(0, min(255, 240 + val))
        g = max(0, min(255, 192 + int(val * 0.8)))
        b = max(0, min(255, 122 + int(val * 0.5)))
        sand.putpixel((x, max(0, min(511, int(y + wave)))), (r, g, b))

sand = sand.filter(ImageFilter.GaussianBlur(1.5))
sand.save(f'{out_dir}/DesertSand_Albedo.png')

# 3. Crate_Albedo.png (512x512)
crate = Image.new('RGB', (512, 512), (160, 105, 60))
draw = ImageDraw.Draw(crate)

# Wood grain planks
for y in range(0, 512, 64):
    c_mod = random.randint(-15, 15)
    draw.rectangle([0, y, 512, y + 62], fill=(165 + c_mod, 110 + c_mod, 65 + c_mod), outline=(120, 75, 40), width=3)
    # Grain lines
    for gy in range(y + 8, y + 56, 12):
        draw.line([(0, gy), (512, gy)], fill=(145, 95, 52), width=1)

# Diagonal cross brace
draw.line([(0, 0), (512, 512)], fill=(130, 80, 45), width=36)
draw.line([(0, 512), (512, 0)], fill=(130, 80, 45), width=36)
draw.line([(0, 0), (512, 512)], fill=(175, 120, 75), width=24)
draw.line([(0, 512), (512, 0)], fill=(175, 120, 75), width=24)

# Metal corner plates & rivets
metal_color = (75, 75, 80)
metal_light = (110, 110, 115)
border_w = 40
draw.rectangle([0, 0, 512, border_w], fill=metal_color)
draw.rectangle([0, 512 - border_w, 512, 512], fill=metal_color)
draw.rectangle([0, 0, border_w, 512], fill=metal_color)
draw.rectangle([512 - border_w, 0, 512, 512], fill=metal_color)

# Rivets on corners
corners = [(20, 20), (492, 20), (20, 492), (492, 492), (256, 20), (256, 492), (20, 256), (492, 256)]
for cx, cy in corners:
    draw.ellipse([cx - 8, cy - 8, cx + 8, cy + 8], fill=(45, 45, 50))
    draw.ellipse([cx - 6, cy - 6, cx + 4, cy + 4], fill=metal_light)

# Stenciled Warning / Cargo Stripes in center
for i in range(-4, 5):
    sx = 256 + i * 40
    draw.polygon([(sx, 220), (sx + 20, 220), (sx - 10, 292), (sx - 30, 292)], fill=(240, 180, 30))

crate.save(f'{out_dir}/Crate_Albedo.png')

# 4. Rock_Albedo.png (512x512)
rock = Image.new('RGB', (512, 512), (175, 110, 70))
draw = ImageDraw.Draw(rock)

# Canyon sandstone geological strata
for y in range(0, 512, 24):
    band_shade = random.randint(-25, 25)
    r = max(0, min(255, 180 + band_shade))
    g = max(0, min(255, 115 + int(band_shade * 0.7)))
    b = max(0, min(255, 75 + int(band_shade * 0.5)))
    draw.rectangle([0, y, 512, y + 24], fill=(r, g, b))
    # Rock crack lines
    for _ in range(3):
        x1 = random.randint(0, 400)
        draw.line([(x1, y + 12), (x1 + random.randint(30, 100), y + random.randint(4, 20))], fill=(110, 65, 40), width=2)

rock = rock.filter(ImageFilter.GaussianBlur(1.0))
rock.save(f'{out_dir}/Rock_Albedo.png')

# 5. TreeBark_Albedo.png (512x512)
bark = Image.new('RGB', (512, 512), (110, 75, 50))
draw = ImageDraw.Draw(bark)
for y in range(0, 512, 28):
    draw.rectangle([0, y, 512, y + 24], fill=(125, 85, 55), outline=(85, 55, 35), width=2)
    for x in range(0, 512, 16):
        draw.line([(x, y + 4), (x + random.randint(-4, 4), y + 20)], fill=(90, 60, 40), width=1)
bark.save(f'{out_dir}/TreeBark_Albedo.png')

# 6. TreeLeaves_Albedo.png (512x512)
leaves = Image.new('RGB', (512, 512), (45, 155, 60))
draw = ImageDraw.Draw(leaves)
# Palm frond veins
draw.line([(256, 0), (256, 512)], fill=(120, 210, 80), width=12)
for y in range(0, 512, 20):
    draw.line([(256, y), (20, y + 40)], fill=(65, 185, 75), width=6)
    draw.line([(256, y), (492, y + 40)], fill=(65, 185, 75), width=6)
leaves.save(f'{out_dir}/TreeLeaves_Albedo.png')

# 7. Coin_Albedo.png (512x512)
coin = Image.new('RGB', (512, 512), (220, 165, 20))
draw = ImageDraw.Draw(coin)
# Golden radial gradients & rims
draw.ellipse([20, 20, 492, 492], fill=(255, 215, 30), outline=(190, 135, 10), width=22)
draw.ellipse([65, 65, 447, 447], fill=(245, 195, 25), outline=(215, 155, 15), width=14)
# Center Star
cx, cy, r_out, r_in = 256, 256, 130, 55
pts = []
for i in range(10):
    r = r_out if i % 2 == 0 else r_in
    angle = i * math.pi / 5 - math.pi / 2
    pts.append((cx + r * math.cos(angle), cy + r * math.sin(angle)))
draw.polygon(pts, fill=(255, 245, 160), outline=(200, 140, 10))
# Shiny specular highlight arc
draw.arc([75, 75, 437, 437], start=210, end=320, fill=(255, 255, 230), width=18)
coin.save(f'{out_dir}/Coin_Albedo.png')

# 8. Gem_Albedo.png (512x512)
gem = Image.new('RGB', (512, 512), (15, 200, 220))
draw = ImageDraw.Draw(gem)
draw.polygon([(256, 30), (460, 170), (256, 480), (52, 170)], fill=(25, 225, 245), outline=(10, 160, 180), width=12)
# Internal crystal facets
draw.polygon([(256, 30), (360, 170), (256, 480)], fill=(70, 240, 255))
draw.polygon([(256, 30), (256, 480), (152, 170)], fill=(10, 185, 205))
draw.polygon([(256, 30), (460, 170), (360, 170)], fill=(120, 250, 255))
# Bright white glint
draw.ellipse([230, 60, 280, 110], fill=(255, 255, 255))
gem.save(f'{out_dir}/Gem_Albedo.png')

# 9. Cactus_Albedo.png (512x512)
cactus = Image.new('RGB', (512, 512), (40, 145, 60))
draw = ImageDraw.Draw(cactus)
for x in range(0, 512, 32):
    draw.line([(x, 0), (x, 512)], fill=(30, 120, 50), width=4)
    for y in range(16, 512, 32):
        draw.ellipse([x - 3, y - 3, x + 3, y + 3], fill=(230, 230, 180))
cactus.save(f'{out_dir}/Cactus_Albedo.png')

# 10. DesertSky_Panorama.png (1024x512)
# Vibrant blue zenith to warm golden-orange sunset horizon
sky = Image.new('RGB', (1024, 512), (100, 180, 245))
draw = ImageDraw.Draw(sky)
for y in range(512):
    t = y / 511.0
    if t < 0.5:
        # Zenith blue to warm cyan
        sub_t = t / 0.5
        r = int(50 * (1 - sub_t) + 120 * sub_t)
        g = int(140 * (1 - sub_t) + 210 * sub_t)
        b = int(245 * (1 - sub_t) + 240 * sub_t)
    else:
        # Cyan to golden sunset orange
        sub_t = (t - 0.5) / 0.5
        r = int(120 * (1 - sub_t) + 255 * sub_t)
        g = int(210 * (1 - sub_t) + 175 * sub_t)
        b = int(240 * (1 - sub_t) + 90 * sub_t)
    draw.line([(0, y), (1024, y)], fill=(r, g, b))

# Distant desert mountains along horizon (bottom 120px)
for i in range(1024):
    h = 80 + int(35 * math.sin(i * 0.015) + 20 * math.sin(i * 0.04))
    draw.line([(i, 512 - h), (i, 512)], fill=(215, 130, 75))

# Golden Sun Disc
draw.ellipse([460, 290, 564, 394], fill=(255, 255, 220))
# Sun halo glow
for r in range(60, 110, 8):
    alpha = int(40 * (1 - (r - 60) / 50.0))
    # Outer ring
    draw.ellipse([512 - r, 342 - r, 512 + r, 342 + r], outline=(255, 230, 140))

sky = sky.filter(ImageFilter.GaussianBlur(1.0))
sky.save(f'{out_dir}/DesertSky_Panorama.png')

print('All 10 stylized textures generated successfully in Assets/Materials/Textures!')
