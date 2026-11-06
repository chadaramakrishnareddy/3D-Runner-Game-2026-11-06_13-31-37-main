import math, os
from PIL import Image, ImageDraw

os.makedirs('Assets/UI', exist_ok=True)
os.makedirs('Assets/Models', exist_ok=True)
os.makedirs('Assets/Animations', exist_ok=True)
os.makedirs('Assets/Resources', exist_ok=True)

# 1. UI: coin_icon.png (256x256)
img = Image.new('RGBA', (256, 256), (0, 0, 0, 0))
draw = ImageDraw.Draw(img)
draw.ellipse([16, 16, 240, 240], fill=(245, 180, 20), outline=(210, 140, 10), width=8)
draw.ellipse([36, 36, 220, 220], fill=(255, 210, 40), outline=(230, 165, 15), width=6)
cx, cy, r_out, r_in = 128, 128, 60, 26
pts = []
for i in range(10):
    r = r_out if i % 2 == 0 else r_in
    angle = i * math.pi / 5 - math.pi / 2
    pts.append((cx + r * math.cos(angle), cy + r * math.sin(angle)))
draw.polygon(pts, fill=(255, 245, 160), outline=(220, 160, 10))
img.save('Assets/UI/coin_icon.png')

# 2. UI: gem_icon.png (256x256)
img = Image.new('RGBA', (256, 256), (0, 0, 0, 0))
draw = ImageDraw.Draw(img)
pts = [(128, 20), (225, 95), (128, 235), (31, 95)]
draw.polygon(pts, fill=(20, 220, 240), outline=(10, 160, 180), width=6)
draw.polygon([(128, 20), (180, 95), (128, 235)], fill=(60, 240, 255))
draw.polygon([(128, 20), (128, 235), (76, 95)], fill=(15, 190, 210))
draw.polygon([(128, 20), (76, 95), (31, 95)], fill=(10, 165, 185))
draw.polygon([(128, 20), (225, 95), (180, 95)], fill=(90, 245, 255))
img.save('Assets/UI/gem_icon.png')

# 3. UI: runner_icon.png (256x256)
img = Image.new('RGBA', (256, 256), (0, 0, 0, 0))
draw = ImageDraw.Draw(img)
draw.ellipse([140, 25, 185, 70], fill=(255, 255, 255))
draw.polygon([(120, 80), (160, 70), (145, 140), (110, 130)], fill=(255, 255, 255))
draw.line([(145, 140), (180, 190), (220, 185)], fill=(255, 255, 255), width=18)
draw.line([(120, 135), (85, 175), (50, 230)], fill=(255, 255, 255), width=18)
draw.line([(155, 85), (200, 110), (185, 145)], fill=(255, 255, 255), width=16)
draw.line([(125, 90), (80, 105), (70, 75)], fill=(255, 255, 255), width=16)
img.save('Assets/UI/runner_icon.png')

# 4. UI: trophy_icon.png (256x256)
img = Image.new('RGBA', (256, 256), (0, 0, 0, 0))
draw = ImageDraw.Draw(img)
draw.polygon([(60, 40), (196, 40), (180, 140), (140, 170), (116, 170), (76, 140)], fill=(255, 200, 30), outline=(210, 150, 15), width=5)
draw.rectangle([116, 170, 140, 210], fill=(220, 170, 20))
draw.rectangle([70, 210, 186, 235], fill=(240, 180, 25), outline=(200, 140, 10), width=4)
img.save('Assets/UI/trophy_icon.png')

# 5. Models: CoinModel.obj
with open('Assets/Models/CoinModel.obj', 'w') as f:
    f.write('# Coin 3D Model\n')
    f.write('v 0.0 0.05 0.0\n')
    for i in range(16):
        a = i * 2 * math.pi / 16
        f.write(f'v {0.5 * math.cos(a):.4f} 0.05 {0.5 * math.sin(a):.4f}\n')
        f.write(f'v {0.5 * math.cos(a):.4f} -0.05 {0.5 * math.sin(a):.4f}\n')
    f.write('v 0.0 -0.05 0.0\n')
    f.write('s 1\n')
    for i in range(16):
        nxt = (i + 1) % 16
        f.write(f'f 1 {i*2 + 2} {nxt*2 + 2}\n')
        f.write(f'f {i*2 + 2} {i*2 + 3} {nxt*2 + 3}\n')
        f.write(f'f {i*2 + 2} {nxt*2 + 3} {nxt*2 + 2}\n')
        f.write(f'f 34 {nxt*2 + 3} {i*2 + 3}\n')

# 6. Models: GemModel.obj
with open('Assets/Models/GemModel.obj', 'w') as f:
    f.write('# Gem 3D Octahedron Model\n')
    f.write('v 0.0 0.7 0.0\n')
    f.write('v 0.5 0.0 0.5\n')
    f.write('v -0.5 0.0 0.5\n')
    f.write('v -0.5 0.0 -0.5\n')
    f.write('v 0.5 0.0 -0.5\n')
    f.write('v 0.0 -0.7 0.0\n')
    f.write('f 1 2 3\n')
    f.write('f 1 3 4\n')
    f.write('f 1 4 5\n')
    f.write('f 1 5 2\n')
    f.write('f 6 3 2\n')
    f.write('f 6 4 3\n')
    f.write('f 6 5 4\n')
    f.write('f 6 2 5\n')

# 7. Models: CrateModel.obj
with open('Assets/Models/CrateModel.obj', 'w') as f:
    f.write('# Crate 3D Model\n')
    f.write('v -0.6 -0.6 0.6\n')
    f.write('v 0.6 -0.6 0.6\n')
    f.write('v 0.6 0.6 0.6\n')
    f.write('v -0.6 0.6 0.6\n')
    f.write('v -0.6 -0.6 -0.6\n')
    f.write('v 0.6 -0.6 -0.6\n')
    f.write('v 0.6 0.6 -0.6\n')
    f.write('v -0.6 0.6 -0.6\n')
    f.write('f 1 2 3 4\n')
    f.write('f 5 8 7 6\n')
    f.write('f 1 5 6 2\n')
    f.write('f 2 6 7 3\n')
    f.write('f 3 7 8 4\n')
    f.write('f 4 8 5 1\n')

# 8. Resources: GameConfig.json
with open('Assets/Resources/GameConfig.json', 'w') as f:
    f.write('{\n  "gameTitle": "Desert Runner",\n  "subtitle": "RUN • COLLECT • AVOID • SURVIVE",\n  "forwardSpeed": 15.0,\n  "laneDistance": 3.0,\n  "pointsPerMeter": 1,\n  "pointsPerCoin": 10,\n  "pointsPerGem": 50\n}\n')

print('All assets created successfully!')
