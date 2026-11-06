import os, re

# Texture GUIDs
GUIDS = {
    'Cactus': '61d35946228248a4b9a5186383bd7902',
    'Coin': 'da5fd2d394ed4c2881f75c33e75f3099',
    'Crate': '701cb5b249fb4b84a9fb926ade5991d3',
    'DesertRoad': 'ebe28766a1dd4799bcee464547bfaac8',
    'DesertSand': '925c300e1f5d49bdbf2dc877399a42bb',
    'DesertSky': '02b48c37ce3e4a81bf1f37f80fec532e',
    'Gem': 'cc9b944d9b254a9d817488b246173823',
    'Rock': '2884eb4cb4854c19a8945b2d2b07d843',
    'TreeBark': '7a52304e38154c0e9ccd74a1613247c0',
    'TreeLeaves': '849d6f839c2f4659ab89e6b76691a39d'
}

TILINGS = {
    'DesertRoad': (1, 8),
    'DesertSand': (4, 8),
}

materials_dir = 'Assets/Materials'

def update_material(mat_name, guid, scale=(1, 1)):
    filepath = os.path.join(materials_dir, f'{mat_name}.mat')
    if not os.path.exists(filepath):
        print(f'Missing {filepath}')
        return
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Replace _BaseMap texture
    tex_str = f'm_Texture: {{fileID: 2800000, guid: {guid}, type: 3}}\n        m_Scale: {{x: {scale[0]}, y: {scale[1]}}}'
    content = re.sub(r'(- _BaseMap:\s+m_Texture:\s*\{fileID:[^\}]+\}\s+m_Scale:\s*\{x:[^\}]+\})',
                     f'- _BaseMap:\n        {tex_str}', content)
    content = re.sub(r'(- _MainTex:\s+m_Texture:\s*\{fileID:[^\}]+\}\s+m_Scale:\s*\{x:[^\}]+\})',
                     f'- _MainTex:\n        {tex_str}', content)

    # Make base color bright white so texture details show with 100% vibrance
    content = re.sub(r'(- _BaseColor:\s*\{r:)[^\}]+(\})', r'\g<1> 1, g: 1, b: 1, a: 1\2', content)
    content = re.sub(r'(- _Color:\s*\{r:)[^\}]+(\})', r'\g<1> 1, g: 1, b: 1, a: 1\2', content)

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print(f'Updated {mat_name}.mat with texture {guid}')

# Update each material
for name, guid in GUIDS.items():
    if name != 'DesertSky':
        scale = TILINGS.get(name, (1, 1))
        update_material(name, guid, scale)

print('All materials updated with textures!')
