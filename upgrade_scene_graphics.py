import re

def upgrade_scene(scene_path):
    with open(scene_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # 1. Camera background to vibrant Subway Surfers blue sky
    content = re.sub(
        r'(Camera:\s+m_ObjectHideFlags:[^\n]+\s+m_CorrespondingSourceObject:[^\n]+\s+m_PrefabInstance:[^\n]+\s+m_PrefabAsset:[^\n]+\s+m_GameObject:[^\n]+\s+m_Enabled:[^\n]+\s+serializedVersion:[^\n]+\s+m_ClearFlags:\s*)\d+',
        r'\g<1>2', content
    )
    content = re.sub(
        r'm_BackGroundColor:\s*\{r:\s*0\.9[0-9]*, g:\s*0\.7[0-9]*, b:\s*0\.5[0-9]*, a:\s*1\}',
        r'm_BackGroundColor: {r: 0.28, g: 0.65, b: 0.96, a: 1}',
        content
    )

    # 2. RenderSettings: Enable warm sunset fog
    content = re.sub(r'm_Fog:\s*0', r'm_Fog: 1', content)
    content = re.sub(r'm_FogColor:\s*\{r:\s*0\.5, g:\s*0\.5, b:\s*0\.5, a:\s*1\}',
                     r'm_FogColor: {r: 0.96, g: 0.76, b: 0.46, a: 1}', content)
    content = re.sub(r'm_FogDensity:\s*0\.01', r'm_FogDensity: 0.005', content)

    # 3. Ambient lighting warmth
    content = re.sub(r'm_AmbientSkyColor:\s*\{[^\}]+\}', r'm_AmbientSkyColor: {r: 0.95, g: 0.88, b: 0.78, a: 1}', content)
    content = re.sub(r'm_AmbientGroundColor:\s*\{[^\}]+\}', r'm_AmbientGroundColor: {r: 0.55, g: 0.38, b: 0.22, a: 1}', content)

    # 4. Directional Light: Enable real-time soft shadows
    content = re.sub(r'm_Shadows:\s+m_Type:\s*0', r'm_Shadows:\n    m_Type: 2', content)
    content = re.sub(r'm_Strength:\s*1', r'm_Strength: 0.85', content)

    with open(scene_path, 'w', encoding='utf-8') as f:
        f.write(content)
    print(f'Successfully upgraded graphics in {scene_path}')

upgrade_scene('Assets/Scenes/GamePlay.unity')
upgrade_scene('Assets/Scenes/MainMenu.unity')
