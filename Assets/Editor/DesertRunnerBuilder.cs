using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public static class DesertRunnerBuilder
{
    private const string MaterialsPath = "Assets/Materials";
    private const string PrefabsPath = "Assets/Prefabs";
    private const string ScenesPath = "Assets/Scenes";
    private const string AudioPath = "Assets/Audio";

    [MenuItem("Desert Runner/Fix Shaders and Materials (Remove Pink)")]
    public static void FixShadersAndMaterials()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying) return;
        Debug.Log(">>> [Desert Runner] Fixing materials, shaders, skybox, and animations...");
        HDAssetGenerator.GenerateAllHDTextures();
        CreateMaterials();
        CreatePrefabs();
        FixSkyboxAndSceneVisuals();
        RefreshActiveScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(">>> [Desert Runner] Complete! All shaders and materials fixed (Standard PBR & textures applied).");
    }

    [MenuItem("Desert Runner/Build Complete Game Project")]
    public static void BuildAll()
    {
        Debug.Log(">>> Starting Subway Surfers Quality Build for Desert Runner...");

        EnsureDirectories();
        HDAssetGenerator.GenerateAllHDTextures();
        CreateMaterials();
        CreatePrefabs();
        BuildMainMenuScene();
        BuildInformationScene();
        BuildStageSelectScene();
        BuildGamePlayScene();
        SetupBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(">>> Subway Surfers Quality Desert Runner Build Complete!");
    }

    [MenuItem("Desert Runner/Build and Setup Game")]
    public static void BuildAndSetupGame()
    {
        BuildAll();
    }

    private static void EnsureDirectories()
    {
        string[] dirs = { MaterialsPath, PrefabsPath, ScenesPath, AudioPath, "Assets/Scripts", "Assets/UI" };
        foreach (var dir in dirs)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        AssetDatabase.Refresh();
    }

    private static Shader GetAppropriateShader()
    {
        if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
        {
            Shader urp = Shader.Find("Universal Render Pipeline/Lit");
            if (urp != null) return urp;
        }

        Shader standard = Shader.Find("Standard");
        if (standard != null) return standard;

        Shader mobile = Shader.Find("Mobile/Diffuse");
        if (mobile != null) return mobile;

        return Shader.Find("Diffuse");
    }

    private static Material CreateOrGetMaterial(string name, Color color, float metallic = 0.1f, float smoothness = 0.5f, Color? emission = null)
    {
        string path = $"{MaterialsPath}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader targetShader = GetAppropriateShader();

        if (mat == null)
        {
            mat = new Material(targetShader);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.shader = targetShader;
        }

        mat.color = color;
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);

        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

        if (emission.HasValue)
        {
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission.Value);
        }

        EditorUtility.SetDirty(mat);
        return mat;
    }

    public static void CreateMaterials()
    {
        // Stylized Subway Surfers Desert Palette
        CreateOrGetMaterial("DesertSand", new Color(0.94f, 0.78f, 0.48f), 0.0f, 0.1f);
        CreateOrGetMaterial("DesertRoad", new Color(0.82f, 0.65f, 0.42f), 0.05f, 0.25f);
        CreateOrGetMaterial("RoadStripe", new Color(1.0f, 0.95f, 0.75f), 0.1f, 0.4f, new Color(0.2f, 0.18f, 0.10f));
        CreateOrGetMaterial("DesertCurb", new Color(0.65f, 0.45f, 0.28f), 0.1f, 0.2f);
        CreateOrGetMaterial("DesertCurbAlt", new Color(0.85f, 0.55f, 0.32f), 0.1f, 0.2f);
        CreateOrGetMaterial("Rock", new Color(0.70f, 0.44f, 0.26f), 0.15f, 0.3f);
        CreateOrGetMaterial("TreeBark", new Color(0.45f, 0.30f, 0.18f), 0.05f, 0.2f);
        CreateOrGetMaterial("TreeLeaves", new Color(0.22f, 0.68f, 0.28f), 0.0f, 0.35f);
        CreateOrGetMaterial("Cactus", new Color(0.18f, 0.62f, 0.26f), 0.0f, 0.25f);
        CreateOrGetMaterial("Crate", new Color(0.62f, 0.42f, 0.24f), 0.1f, 0.3f);
        CreateOrGetMaterial("CrateBorder", new Color(0.40f, 0.25f, 0.14f), 0.2f, 0.4f);
        CreateOrGetMaterial("Coin", new Color(1.0f, 0.84f, 0.05f), 0.95f, 0.88f, new Color(0.35f, 0.25f, 0.0f));
        CreateOrGetMaterial("CoinInner", new Color(1.0f, 0.92f, 0.30f), 0.9f, 0.9f);
        CreateOrGetMaterial("Gem", new Color(0.05f, 0.95f, 0.95f), 0.4f, 0.95f, new Color(0.1f, 0.55f, 0.65f));

        // Runner Character Materials (Subway Surfers Jake style outfit)
        CreateOrGetMaterial("PlayerHoodie", new Color(0.15f, 0.50f, 0.85f), 0.1f, 0.4f); // Cool blue runner jacket
        CreateOrGetMaterial("PlayerCap", new Color(0.92f, 0.25f, 0.20f), 0.1f, 0.4f);    // Red backward runner cap
        CreateOrGetMaterial("PlayerSkin", new Color(0.96f, 0.82f, 0.70f), 0.0f, 0.3f);   // Skin tone
        CreateOrGetMaterial("PlayerPants", new Color(0.18f, 0.22f, 0.32f), 0.1f, 0.35f); // Dark denim pants
        CreateOrGetMaterial("PlayerSneakers", new Color(0.98f, 0.98f, 0.98f), 0.1f, 0.5f);// White sneakers
        CreateOrGetMaterial("PlayerBackpack", new Color(0.88f, 0.42f, 0.15f), 0.15f, 0.4f);// Orange backpack
        CreateOrGetMaterial("PlayerHair", new Color(0.22f, 0.15f, 0.10f), 0.05f, 0.35f); // Rich brown stylized hair
        CreateOrGetMaterial("PlayerShades", new Color(0.08f, 0.08f, 0.10f), 0.85f, 0.95f);// Glossy polarized sunglasses
        CreateOrGetMaterial("PlayerDetail", new Color(0.92f, 0.92f, 0.92f), 0.70f, 0.75f);// Drawstrings & metallic accents
        CreateOrGetMaterial("WoodPole", new Color(0.42f, 0.30f, 0.20f), 0.05f, 0.20f);    // Weathered roadside telephone pole
        CreateOrGetMaterial("WireLine", new Color(0.15f, 0.15f, 0.17f), 0.40f, 0.45f);    // Overhead highway wire

        // Apply HD textures from Assets/Materials/Textures/ to the appropriate materials
        ApplyTextureToMaterial("DesertSand", "DesertSand_Albedo", new Vector2(4f, 8f));
        ApplyTextureToMaterial("DesertRoad", "DesertRoad_Albedo", new Vector2(1f, 8f));
        ApplyTextureToMaterial("Rock", "Rock_Albedo", new Vector2(1f, 1f));
        ApplyTextureToMaterial("TreeBark", "TreeBark_Albedo", new Vector2(1f, 2f));
        ApplyTextureToMaterial("TreeLeaves", "TreeLeaves_Albedo", new Vector2(1f, 1f));
        ApplyTextureToMaterial("Cactus", "Cactus_Albedo", new Vector2(1f, 2f));
        ApplyTextureToMaterial("Coin", "Coin_Albedo", new Vector2(1f, 1f));
        ApplyTextureToMaterial("Crate", "Crate_Albedo", new Vector2(1f, 1f));
        ApplyTextureToMaterial("Gem", "Gem_Albedo", new Vector2(1f, 1f));
        ApplyTextureToMaterial("PlayerHoodie", "PlayerOutfit_Albedo", new Vector2(1f, 1f));
        ApplyTextureToMaterial("PlayerPants", "PlayerOutfit_Albedo", new Vector2(1f, 1f));
        ApplyTextureToMaterial("PlayerBackpack", "PlayerOutfit_Albedo", new Vector2(1f, 1f));

        AssetDatabase.SaveAssets();
    }

    private static void ApplyTextureToMaterial(string materialName, string textureName, Vector2? tiling = null)
    {
        string matPath = $"{MaterialsPath}/{materialName}.mat";
        string texPath = $"{MaterialsPath}/Textures/{textureName}.png";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (mat != null && tex != null)
        {
            mat.shader = GetAppropriateShader();
            mat.mainTexture = tex;
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);

            mat.color = Color.white;
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);

            Vector2 scale = tiling ?? Vector2.one;
            mat.mainTextureScale = scale;
            if (mat.HasProperty("_MainTex")) mat.SetTextureScale("_MainTex", scale);
            if (mat.HasProperty("_BaseMap")) mat.SetTextureScale("_BaseMap", scale);

            EditorUtility.SetDirty(mat);
        }
    }

    public static void FixSkyboxAndSceneVisuals()
    {
        // 1. Skybox Material
        Texture2D skyTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{MaterialsPath}/Textures/DesertSky_Panorama.png");
        if (skyTex != null)
        {
            Shader skyShader = Shader.Find("Skybox/Panoramic");
            if (skyShader == null) skyShader = Shader.Find("Skybox/6 Sided");
            if (skyShader != null)
            {
                string skyMatPath = $"{MaterialsPath}/DesertSkybox.mat";
                Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(skyMatPath);
                if (skyMat == null)
                {
                    skyMat = new Material(skyShader);
                    AssetDatabase.CreateAsset(skyMat, skyMatPath);
                }
                else
                {
                    skyMat.shader = skyShader;
                }
                skyMat.SetTexture("_MainTex", skyTex);
                if (skyMat.HasProperty("_Tint")) skyMat.SetColor("_Tint", new Color(1f, 0.95f, 0.88f));
                if (skyMat.HasProperty("_Exposure")) skyMat.SetFloat("_Exposure", 1.15f);
                EditorUtility.SetDirty(skyMat);
                RenderSettings.skybox = skyMat;
            }
        }

        // 2. Warm Desert Atmospheric Fog
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.96f, 0.78f, 0.52f);
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.005f;

        // 3. Ambient Lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.96f, 0.86f, 0.68f);
        RenderSettings.ambientEquatorColor = new Color(0.85f, 0.68f, 0.45f);
        RenderSettings.ambientGroundColor = new Color(0.55f, 0.38f, 0.22f);

        // 4. Update Main Camera in active scene if present
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.Skybox;
            mainCam.fieldOfView = 58f;
            CameraController camCtrl = mainCam.GetComponent<CameraController>();
            if (camCtrl != null)
            {
                SerializedObject camSo = new SerializedObject(camCtrl);
                camSo.FindProperty("offset").vector3Value = new Vector3(0f, 2.8f, -4.8f);
                camSo.FindProperty("lookAheadOffset").vector3Value = new Vector3(0f, 1.4f, 9.0f);
                camSo.ApplyModifiedProperties();
            }
            EditorUtility.SetDirty(mainCam.gameObject);
        }
    }

    public static void CreatePrefabs()
    {
        CreatePlayerPrefab();
        CreateCoinPrefab();
        CreateGemPrefab();
        CreateTreePrefab();
        CreateRockPrefab();
        CreateCratePrefab();
        CreateCactusPrefab();
        CreateDesertRockPrefab();
        CreateDesertSegmentPrefab();
    }

    private static void CreatePlayerPrefab()
    {
        string path = $"{PrefabsPath}/Player.prefab";
        GameObject root = new GameObject("Player");
        root.tag = "Player";

        CharacterController cc = root.AddComponent<CharacterController>();
        cc.center = new Vector3(0f, 1.0f, 0f);
        cc.radius = 0.42f;
        cc.height = 2.0f;

        PlayerMovement pm = root.AddComponent<PlayerMovement>();
        root.AddComponent<ObstacleCollision>();

        // Character Model Hierarchy
        GameObject model = new GameObject("CharacterModel");
        model.transform.SetParent(root.transform);
        model.transform.localPosition = Vector3.zero;

        // Add Animator component to Player root
        Animator animator = root.AddComponent<Animator>();
        RuntimeAnimatorController animController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/PlayerAnimator.controller");
        if (animController != null) animator.runtimeAnimatorController = animController;
        animator.applyRootMotion = false;

        Material hoodieMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/PlayerHoodie.mat");
        Material capMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/PlayerCap.mat");
        Material skinMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/PlayerSkin.mat");
        Material pantsMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/PlayerPants.mat");
        Material shoeMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/PlayerSneakers.mat");
        Material packMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/PlayerBackpack.mat");
        Material hairMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/PlayerHair.mat");
        Material shadesMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/PlayerShades.mat");
        Material detailMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/PlayerDetail.mat");

        // 1. Torso: Rounded Capsule Hoodie
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        torso.name = "Torso";
        torso.transform.SetParent(model.transform);
        torso.transform.localPosition = new Vector3(0f, 1.06f, 0f);
        torso.transform.localScale = new Vector3(0.54f, 0.42f, 0.40f);
        if (hoodieMat != null) torso.GetComponent<Renderer>().sharedMaterial = hoodieMat;
        Object.DestroyImmediate(torso.GetComponent<Collider>());

        // Kangaroo front pocket on hoodie
        GameObject pocket = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        pocket.name = "HoodiePocket";
        pocket.transform.SetParent(torso.transform);
        pocket.transform.localPosition = new Vector3(0f, -0.28f, 0.40f);
        pocket.transform.localScale = new Vector3(0.75f, 0.28f, 0.35f);
        if (hoodieMat != null) pocket.GetComponent<Renderer>().sharedMaterial = hoodieMat;
        Object.DestroyImmediate(pocket.GetComponent<Collider>());

        // Hoodie collar / cowl
        GameObject collar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        collar.name = "HoodieCollar";
        collar.transform.SetParent(torso.transform);
        collar.transform.localPosition = new Vector3(0f, 0.88f, -0.05f);
        collar.transform.localScale = new Vector3(0.68f, 0.28f, 0.65f);
        if (hoodieMat != null) collar.GetComponent<Renderer>().sharedMaterial = hoodieMat;
        Object.DestroyImmediate(collar.GetComponent<Collider>());

        // Center front zipper line
        GameObject zipper = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        zipper.name = "ZipperLine";
        zipper.transform.SetParent(torso.transform);
        zipper.transform.localPosition = new Vector3(0f, 0.15f, 0.46f);
        zipper.transform.localScale = new Vector3(0.04f, 0.65f, 0.05f);
        if (detailMat != null) zipper.GetComponent<Renderer>().sharedMaterial = detailMat;
        Object.DestroyImmediate(zipper.GetComponent<Collider>());

        // Dual hoodie drawstrings hanging down
        GameObject stringL = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        stringL.name = "DrawstringL";
        stringL.transform.SetParent(torso.transform);
        stringL.transform.localPosition = new Vector3(-0.10f, 0.45f, 0.44f);
        stringL.transform.localScale = new Vector3(0.035f, 0.22f, 0.035f);
        if (detailMat != null) stringL.GetComponent<Renderer>().sharedMaterial = detailMat;
        Object.DestroyImmediate(stringL.GetComponent<Collider>());

        GameObject stringR = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        stringR.name = "DrawstringR";
        stringR.transform.SetParent(torso.transform);
        stringR.transform.localPosition = new Vector3(0.10f, 0.45f, 0.44f);
        stringR.transform.localScale = new Vector3(0.035f, 0.22f, 0.035f);
        if (detailMat != null) stringR.GetComponent<Renderer>().sharedMaterial = detailMat;
        Object.DestroyImmediate(stringR.GetComponent<Collider>());

        // 2. Backpack: Rounded sporty pack
        GameObject pack = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        pack.name = "Backpack";
        pack.transform.SetParent(torso.transform);
        pack.transform.localPosition = new Vector3(0f, 0.05f, -0.68f);
        pack.transform.localScale = new Vector3(0.72f, 0.62f, 0.52f);
        if (packMat != null) pack.GetComponent<Renderer>().sharedMaterial = packMat;
        Object.DestroyImmediate(pack.GetComponent<Collider>());

        GameObject packPouch = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        packPouch.name = "PackPouch";
        packPouch.transform.SetParent(pack.transform);
        packPouch.transform.localPosition = new Vector3(0f, -0.22f, -0.42f);
        packPouch.transform.localScale = new Vector3(0.70f, 0.38f, 0.42f);
        if (packMat != null) packPouch.GetComponent<Renderer>().sharedMaterial = packMat;
        Object.DestroyImmediate(packPouch.GetComponent<Collider>());

        // Backpack shoulder straps
        GameObject strapL = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        strapL.name = "ShoulderStrapL";
        strapL.transform.SetParent(torso.transform);
        strapL.transform.localPosition = new Vector3(-0.24f, 0.32f, 0.32f);
        strapL.transform.localScale = new Vector3(0.09f, 0.52f, 0.09f);
        if (packMat != null) strapL.GetComponent<Renderer>().sharedMaterial = packMat;
        Object.DestroyImmediate(strapL.GetComponent<Collider>());

        GameObject strapR = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        strapR.name = "ShoulderStrapR";
        strapR.transform.SetParent(torso.transform);
        strapR.transform.localPosition = new Vector3(0.24f, 0.32f, 0.32f);
        strapR.transform.localScale = new Vector3(0.09f, 0.52f, 0.09f);
        if (packMat != null) strapR.GetComponent<Renderer>().sharedMaterial = packMat;
        Object.DestroyImmediate(strapR.GetComponent<Collider>());

        // Subway Surfers spray paint can in backpack side slot
        GameObject sprayCan = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        sprayCan.name = "SprayPaintCan";
        sprayCan.transform.SetParent(pack.transform);
        sprayCan.transform.localPosition = new Vector3(0.42f, -0.05f, -0.15f);
        sprayCan.transform.localScale = new Vector3(0.20f, 0.30f, 0.20f);
        if (detailMat != null) sprayCan.GetComponent<Renderer>().sharedMaterial = detailMat;
        Object.DestroyImmediate(sprayCan.GetComponent<Collider>());

        GameObject sprayTip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sprayTip.name = "SprayTip";
        sprayTip.transform.SetParent(sprayCan.transform);
        sprayTip.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        sprayTip.transform.localScale = new Vector3(0.5f, 0.4f, 0.5f);
        if (capMat != null) sprayTip.GetComponent<Renderer>().sharedMaterial = capMat;
        Object.DestroyImmediate(sprayTip.GetComponent<Collider>());

        // 3. Head: Smooth Sphere
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(model.transform);
        head.transform.localPosition = new Vector3(0f, 1.62f, 0f);
        head.transform.localScale = new Vector3(0.44f, 0.44f, 0.44f);
        if (skinMat != null) head.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(head.GetComponent<Collider>());

        // Stylized hair strands peeking out beneath cap
        GameObject hairBangs = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        hairBangs.name = "HairBangs";
        hairBangs.transform.SetParent(head.transform);
        hairBangs.transform.localPosition = new Vector3(0f, 0.16f, 0.38f);
        hairBangs.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        hairBangs.transform.localScale = new Vector3(0.18f, 0.28f, 0.14f);
        if (hairMat != null) hairBangs.GetComponent<Renderer>().sharedMaterial = hairMat;
        Object.DestroyImmediate(hairBangs.GetComponent<Collider>());

        GameObject hairLeft = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        hairLeft.name = "HairLeft";
        hairLeft.transform.SetParent(head.transform);
        hairLeft.transform.localPosition = new Vector3(-0.24f, -0.04f, 0.12f);
        hairLeft.transform.localScale = new Vector3(0.14f, 0.22f, 0.14f);
        if (hairMat != null) hairLeft.GetComponent<Renderer>().sharedMaterial = hairMat;
        Object.DestroyImmediate(hairLeft.GetComponent<Collider>());

        GameObject hairRight = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        hairRight.name = "HairRight";
        hairRight.transform.SetParent(head.transform);
        hairRight.transform.localPosition = new Vector3(0.24f, -0.04f, 0.12f);
        hairRight.transform.localScale = new Vector3(0.14f, 0.22f, 0.14f);
        if (hairMat != null) hairRight.GetComponent<Renderer>().sharedMaterial = hairMat;
        Object.DestroyImmediate(hairRight.GetComponent<Collider>());

        // Sporty Runner Sunglasses
        GameObject shades = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        shades.name = "RunnerShades";
        shades.transform.SetParent(head.transform);
        shades.transform.localPosition = new Vector3(0f, 0.06f, 0.44f);
        shades.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        shades.transform.localScale = new Vector3(0.20f, 0.46f, 0.18f);
        if (shadesMat != null) shades.GetComponent<Renderer>().sharedMaterial = shadesMat;
        Object.DestroyImmediate(shades.GetComponent<Collider>());

        GameObject bridge = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bridge.name = "ShadesBridge";
        bridge.transform.SetParent(head.transform);
        bridge.transform.localPosition = new Vector3(0f, 0.08f, 0.46f);
        bridge.transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);
        if (shadesMat != null) bridge.GetComponent<Renderer>().sharedMaterial = shadesMat;
        Object.DestroyImmediate(bridge.GetComponent<Collider>());

        // Backward Runner Cap: Rounded Dome
        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cap.name = "Cap";
        cap.transform.SetParent(head.transform);
        cap.transform.localPosition = new Vector3(0f, 0.15f, -0.06f);
        cap.transform.localScale = new Vector3(0.96f, 0.68f, 1.02f);
        if (capMat != null) cap.GetComponent<Renderer>().sharedMaterial = capMat;
        Object.DestroyImmediate(cap.GetComponent<Collider>());

        // Cap button on crown
        GameObject capButton = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        capButton.name = "CapButton";
        capButton.transform.SetParent(cap.transform);
        capButton.transform.localPosition = new Vector3(0f, 0.52f, 0f);
        capButton.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
        if (detailMat != null) capButton.GetComponent<Renderer>().sharedMaterial = detailMat;
        Object.DestroyImmediate(capButton.GetComponent<Collider>());

        // Backward Curved Visor
        GameObject capVisor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        capVisor.name = "VisorBackward";
        capVisor.transform.SetParent(cap.transform);
        capVisor.transform.localPosition = new Vector3(0f, -0.06f, -0.52f);
        capVisor.transform.localRotation = Quaternion.Euler(-15f, 0f, 0f);
        capVisor.transform.localScale = new Vector3(0.70f, 0.04f, 0.52f);
        if (capMat != null) capVisor.GetComponent<Renderer>().sharedMaterial = capMat;
        Object.DestroyImmediate(capVisor.GetComponent<Collider>());

        // 4. Left Arm (Shoulder pivot)
        GameObject leftArmPivot = new GameObject("LeftArm");
        leftArmPivot.transform.SetParent(model.transform);
        leftArmPivot.transform.localPosition = new Vector3(-0.38f, 1.34f, 0f);

        GameObject leftShoulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftShoulder.name = "Shoulder";
        leftShoulder.transform.SetParent(leftArmPivot.transform);
        leftShoulder.transform.localPosition = Vector3.zero;
        leftShoulder.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
        if (hoodieMat != null) leftShoulder.GetComponent<Renderer>().sharedMaterial = hoodieMat;
        Object.DestroyImmediate(leftShoulder.GetComponent<Collider>());

        GameObject leftArmMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftArmMesh.name = "Sleeve";
        leftArmMesh.transform.SetParent(leftArmPivot.transform);
        leftArmMesh.transform.localPosition = new Vector3(0f, -0.24f, 0f);
        leftArmMesh.transform.localScale = new Vector3(0.18f, 0.28f, 0.18f);
        if (hoodieMat != null) leftArmMesh.GetComponent<Renderer>().sharedMaterial = hoodieMat;
        Object.DestroyImmediate(leftArmMesh.GetComponent<Collider>());

        GameObject leftCuff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        leftCuff.name = "WristCuff";
        leftCuff.transform.SetParent(leftArmPivot.transform);
        leftCuff.transform.localPosition = new Vector3(0f, -0.42f, 0f);
        leftCuff.transform.localScale = new Vector3(0.19f, 0.04f, 0.19f);
        if (detailMat != null) leftCuff.GetComponent<Renderer>().sharedMaterial = detailMat;
        Object.DestroyImmediate(leftCuff.GetComponent<Collider>());

        GameObject leftHand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftHand.name = "Hand";
        leftHand.transform.SetParent(leftArmPivot.transform);
        leftHand.transform.localPosition = new Vector3(0f, -0.54f, 0f);
        leftHand.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);
        if (skinMat != null) leftHand.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(leftHand.GetComponent<Collider>());

        GameObject leftThumb = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftThumb.name = "Thumb";
        leftThumb.transform.SetParent(leftHand.transform);
        leftThumb.transform.localPosition = new Vector3(0.45f, 0.2f, 0.35f);
        leftThumb.transform.localScale = new Vector3(0.4f, 0.5f, 0.4f);
        if (skinMat != null) leftThumb.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(leftThumb.GetComponent<Collider>());

        // 5. Right Arm (Shoulder pivot)
        GameObject rightArmPivot = new GameObject("RightArm");
        rightArmPivot.transform.SetParent(model.transform);
        rightArmPivot.transform.localPosition = new Vector3(0.38f, 1.34f, 0f);

        GameObject rightShoulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightShoulder.name = "Shoulder";
        rightShoulder.transform.SetParent(rightArmPivot.transform);
        rightShoulder.transform.localPosition = Vector3.zero;
        rightShoulder.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
        if (hoodieMat != null) rightShoulder.GetComponent<Renderer>().sharedMaterial = hoodieMat;
        Object.DestroyImmediate(rightShoulder.GetComponent<Collider>());

        GameObject rightArmMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightArmMesh.name = "Sleeve";
        rightArmMesh.transform.SetParent(rightArmPivot.transform);
        rightArmMesh.transform.localPosition = new Vector3(0f, -0.24f, 0f);
        rightArmMesh.transform.localScale = new Vector3(0.18f, 0.28f, 0.18f);
        if (hoodieMat != null) rightArmMesh.GetComponent<Renderer>().sharedMaterial = hoodieMat;
        Object.DestroyImmediate(rightArmMesh.GetComponent<Collider>());

        GameObject rightCuff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rightCuff.name = "WristCuff";
        rightCuff.transform.SetParent(rightArmPivot.transform);
        rightCuff.transform.localPosition = new Vector3(0f, -0.42f, 0f);
        rightCuff.transform.localScale = new Vector3(0.19f, 0.04f, 0.19f);
        if (detailMat != null) rightCuff.GetComponent<Renderer>().sharedMaterial = detailMat;
        Object.DestroyImmediate(rightCuff.GetComponent<Collider>());

        GameObject rightHand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightHand.name = "Hand";
        rightHand.transform.SetParent(rightArmPivot.transform);
        rightHand.transform.localPosition = new Vector3(0f, -0.54f, 0f);
        rightHand.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);
        if (skinMat != null) rightHand.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(rightHand.GetComponent<Collider>());

        GameObject rightThumb = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightThumb.name = "Thumb";
        rightThumb.transform.SetParent(rightHand.transform);
        rightThumb.transform.localPosition = new Vector3(-0.45f, 0.2f, 0.35f);
        rightThumb.transform.localScale = new Vector3(0.4f, 0.5f, 0.4f);
        if (skinMat != null) rightThumb.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(rightThumb.GetComponent<Collider>());

        // 6. Left Leg (Hip pivot)
        GameObject leftLegPivot = new GameObject("LeftLeg");
        leftLegPivot.transform.SetParent(model.transform);
        leftLegPivot.transform.localPosition = new Vector3(-0.18f, 0.72f, 0f);

        GameObject leftLegMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftLegMesh.name = "Pants";
        leftLegMesh.transform.SetParent(leftLegPivot.transform);
        leftLegMesh.transform.localPosition = new Vector3(0f, -0.25f, 0f);
        leftLegMesh.transform.localScale = new Vector3(0.22f, 0.32f, 0.22f);
        if (pantsMat != null) leftLegMesh.GetComponent<Renderer>().sharedMaterial = pantsMat;
        Object.DestroyImmediate(leftLegMesh.GetComponent<Collider>());

        GameObject leftAnkleCuff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        leftAnkleCuff.name = "AnkleCuff";
        leftAnkleCuff.transform.SetParent(leftLegPivot.transform);
        leftAnkleCuff.transform.localPosition = new Vector3(0f, -0.46f, 0f);
        leftAnkleCuff.transform.localScale = new Vector3(0.24f, 0.05f, 0.24f);
        if (pantsMat != null) leftAnkleCuff.GetComponent<Renderer>().sharedMaterial = pantsMat;
        Object.DestroyImmediate(leftAnkleCuff.GetComponent<Collider>());

        // Athletic Sneaker
        GameObject leftShoeUpper = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftShoeUpper.name = "SneakerUpper";
        leftShoeUpper.transform.SetParent(leftLegPivot.transform);
        leftShoeUpper.transform.localPosition = new Vector3(0f, -0.55f, 0.04f);
        leftShoeUpper.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
        leftShoeUpper.transform.localScale = new Vector3(0.22f, 0.15f, 0.32f);
        if (capMat != null) leftShoeUpper.GetComponent<Renderer>().sharedMaterial = capMat;
        Object.DestroyImmediate(leftShoeUpper.GetComponent<Collider>());

        GameObject leftShoeSole = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftShoeSole.name = "SneakerSole";
        leftShoeSole.transform.SetParent(leftLegPivot.transform);
        leftShoeSole.transform.localPosition = new Vector3(0f, -0.63f, 0.06f);
        leftShoeSole.transform.localScale = new Vector3(0.24f, 0.10f, 0.42f);
        if (shoeMat != null) leftShoeSole.GetComponent<Renderer>().sharedMaterial = shoeMat;
        Object.DestroyImmediate(leftShoeSole.GetComponent<Collider>());

        GameObject leftShoeToe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftShoeToe.name = "SneakerToeCap";
        leftShoeToe.transform.SetParent(leftLegPivot.transform);
        leftShoeToe.transform.localPosition = new Vector3(0f, -0.58f, 0.24f);
        leftShoeToe.transform.localScale = new Vector3(0.22f, 0.12f, 0.16f);
        if (shoeMat != null) leftShoeToe.GetComponent<Renderer>().sharedMaterial = shoeMat;
        Object.DestroyImmediate(leftShoeToe.GetComponent<Collider>());

        // 7. Right Leg (Hip pivot)
        GameObject rightLegPivot = new GameObject("RightLeg");
        rightLegPivot.transform.SetParent(model.transform);
        rightLegPivot.transform.localPosition = new Vector3(0.18f, 0.72f, 0f);

        GameObject rightLegMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightLegMesh.name = "Pants";
        rightLegMesh.transform.SetParent(rightLegPivot.transform);
        rightLegMesh.transform.localPosition = new Vector3(0f, -0.25f, 0f);
        rightLegMesh.transform.localScale = new Vector3(0.22f, 0.32f, 0.22f);
        if (pantsMat != null) rightLegMesh.GetComponent<Renderer>().sharedMaterial = pantsMat;
        Object.DestroyImmediate(rightLegMesh.GetComponent<Collider>());

        GameObject rightAnkleCuff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rightAnkleCuff.name = "AnkleCuff";
        rightAnkleCuff.transform.SetParent(rightLegPivot.transform);
        rightAnkleCuff.transform.localPosition = new Vector3(0f, -0.46f, 0f);
        rightAnkleCuff.transform.localScale = new Vector3(0.24f, 0.05f, 0.24f);
        if (pantsMat != null) rightAnkleCuff.GetComponent<Renderer>().sharedMaterial = pantsMat;
        Object.DestroyImmediate(rightAnkleCuff.GetComponent<Collider>());

        GameObject rightShoeUpper = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightShoeUpper.name = "SneakerUpper";
        rightShoeUpper.transform.SetParent(rightLegPivot.transform);
        rightShoeUpper.transform.localPosition = new Vector3(0f, -0.55f, 0.04f);
        rightShoeUpper.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
        rightShoeUpper.transform.localScale = new Vector3(0.22f, 0.15f, 0.32f);
        if (capMat != null) rightShoeUpper.GetComponent<Renderer>().sharedMaterial = capMat;
        Object.DestroyImmediate(rightShoeUpper.GetComponent<Collider>());

        GameObject rightShoeSole = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightShoeSole.name = "SneakerSole";
        rightShoeSole.transform.SetParent(rightLegPivot.transform);
        rightShoeSole.transform.localPosition = new Vector3(0f, -0.63f, 0.06f);
        rightShoeSole.transform.localScale = new Vector3(0.24f, 0.10f, 0.42f);
        if (shoeMat != null) rightShoeSole.GetComponent<Renderer>().sharedMaterial = shoeMat;
        Object.DestroyImmediate(rightShoeSole.GetComponent<Collider>());

        GameObject rightShoeToe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightShoeToe.name = "SneakerToeCap";
        rightShoeToe.transform.SetParent(rightLegPivot.transform);
        rightShoeToe.transform.localPosition = new Vector3(0f, -0.58f, 0.24f);
        rightShoeToe.transform.localScale = new Vector3(0.22f, 0.12f, 0.16f);
        if (shoeMat != null) rightShoeToe.GetComponent<Renderer>().sharedMaterial = shoeMat;
        Object.DestroyImmediate(rightShoeToe.GetComponent<Collider>());

        // 8. Dual Sand Dust Particle System under feet
        GameObject dustObj = new GameObject("RunDustParticles");
        dustObj.transform.SetParent(root.transform);
        dustObj.transform.localPosition = new Vector3(0f, 0.04f, -0.35f);
        ParticleSystem ps = dustObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.45f;
        main.startSpeed = 2.0f;
        main.startSize = 0.32f;
        main.startColor = new Color(0.95f, 0.82f, 0.55f, 0.75f);
        var emission = ps.emission;
        emission.rateOverTime = 30f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.28f;

        // Wire references in PlayerMovement
        SerializedObject so = new SerializedObject(pm);
        so.FindProperty("characterModel").objectReferenceValue = model.transform;
        so.FindProperty("torsoTransform").objectReferenceValue = torso.transform;
        so.FindProperty("headTransform").objectReferenceValue = head.transform;
        so.FindProperty("leftArmTransform").objectReferenceValue = leftArmPivot.transform;
        so.FindProperty("rightArmTransform").objectReferenceValue = rightArmPivot.transform;
        so.FindProperty("leftLegTransform").objectReferenceValue = leftLegPivot.transform;
        so.FindProperty("rightLegTransform").objectReferenceValue = rightLegPivot.transform;
        so.FindProperty("runDustParticles").objectReferenceValue = ps;
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static void CreateCoinPrefab()
    {
        string path = $"{PrefabsPath}/Coin.prefab";
        GameObject root = new GameObject("Coin");
        root.tag = "Coin";

        Material coinMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/Coin.mat");
        Material innerMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/CoinInner.mat");

        // Outer coin disc
        GameObject outer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        outer.name = "CoinOuter";
        outer.transform.SetParent(root.transform);
        outer.transform.localPosition = Vector3.zero;
        outer.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        outer.transform.localScale = new Vector3(0.95f, 0.07f, 0.95f);
        if (coinMat != null) outer.GetComponent<Renderer>().sharedMaterial = coinMat;
        Object.DestroyImmediate(outer.GetComponent<Collider>());

        // Inner star emblem
        GameObject star = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        star.name = "StarEmblem";
        star.transform.SetParent(outer.transform);
        star.transform.localPosition = Vector3.zero;
        star.transform.localScale = new Vector3(0.72f, 1.15f, 0.72f);
        if (innerMat != null) star.GetComponent<Renderer>().sharedMaterial = innerMat;
        Object.DestroyImmediate(star.GetComponent<Collider>());

        SphereCollider col = root.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.65f;

        CollectibleRotate cr = root.AddComponent<CollectibleRotate>();
        SerializedObject so = new SerializedObject(cr);
        so.FindProperty("rotationSpeed").vector3Value = new Vector3(0f, 180f, 0f);
        so.FindProperty("bobHeight").floatValue = 0.16f;
        so.FindProperty("bobSpeed").floatValue = 3.5f;
        so.ApplyModifiedProperties();

        root.AddComponent<CoinCollection>();

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static void CreateGemPrefab()
    {
        string path = $"{PrefabsPath}/Gem.prefab";
        GameObject root = new GameObject("Gem");
        root.tag = "Gem";

        Material gemMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/Gem.mat");

        // Double diamond crystal
        GameObject crystal1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crystal1.name = "GemFacet1";
        crystal1.transform.SetParent(root.transform);
        crystal1.transform.localPosition = Vector3.zero;
        crystal1.transform.localRotation = Quaternion.Euler(45f, 45f, 45f);
        crystal1.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        if (gemMat != null) crystal1.GetComponent<Renderer>().sharedMaterial = gemMat;
        Object.DestroyImmediate(crystal1.GetComponent<Collider>());

        GameObject crystal2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crystal2.name = "GemFacet2";
        crystal2.transform.SetParent(root.transform);
        crystal2.transform.localPosition = Vector3.zero;
        crystal2.transform.localRotation = Quaternion.Euler(0f, 45f, 45f);
        crystal2.transform.localScale = new Vector3(0.52f, 0.52f, 0.52f);
        if (gemMat != null) crystal2.GetComponent<Renderer>().sharedMaterial = gemMat;
        Object.DestroyImmediate(crystal2.GetComponent<Collider>());

        SphereCollider col = root.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.70f;

        CollectibleRotate cr = root.AddComponent<CollectibleRotate>();
        SerializedObject so = new SerializedObject(cr);
        so.FindProperty("rotationSpeed").vector3Value = new Vector3(45f, 110f, 25f);
        so.FindProperty("bobHeight").floatValue = 0.24f;
        so.FindProperty("bobSpeed").floatValue = 2.8f;
        so.ApplyModifiedProperties();

        root.AddComponent<GemCollection>();

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static void CreateTreePrefab()
    {
        string path = $"{PrefabsPath}/Tree.prefab";
        GameObject root = new GameObject("TreeObstacle");
        root.tag = "Obstacle";

        Material barkMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/TreeBark.mat");
        Material leavesMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/TreeLeaves.mat");
        Material hairMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/PlayerHair.mat");

        // 1. Organic Leaning Palm Trunk (5 tapering curved segments)
        GameObject trunk1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk1.name = "TrunkBase";
        trunk1.transform.SetParent(root.transform);
        trunk1.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        trunk1.transform.localScale = new Vector3(0.64f, 0.65f, 0.64f);
        if (barkMat != null) trunk1.GetComponent<Renderer>().sharedMaterial = barkMat;
        Object.DestroyImmediate(trunk1.GetComponent<Collider>());

        GameObject trunk2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk2.name = "TrunkMidLow";
        trunk2.transform.SetParent(root.transform);
        trunk2.transform.localPosition = new Vector3(0.12f, 1.75f, 0.08f);
        trunk2.transform.localRotation = Quaternion.Euler(7f, 15f, -5f);
        trunk2.transform.localScale = new Vector3(0.56f, 0.65f, 0.56f);
        if (barkMat != null) trunk2.GetComponent<Renderer>().sharedMaterial = barkMat;
        Object.DestroyImmediate(trunk2.GetComponent<Collider>());

        GameObject trunk3 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk3.name = "TrunkMid";
        trunk3.transform.SetParent(root.transform);
        trunk3.transform.localPosition = new Vector3(0.30f, 2.85f, 0.18f);
        trunk3.transform.localRotation = Quaternion.Euler(14f, 25f, -9f);
        trunk3.transform.localScale = new Vector3(0.48f, 0.65f, 0.48f);
        if (barkMat != null) trunk3.GetComponent<Renderer>().sharedMaterial = barkMat;
        Object.DestroyImmediate(trunk3.GetComponent<Collider>());

        GameObject trunk4 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk4.name = "TrunkMidHigh";
        trunk4.transform.SetParent(root.transform);
        trunk4.transform.localPosition = new Vector3(0.54f, 3.90f, 0.28f);
        trunk4.transform.localRotation = Quaternion.Euler(20f, 35f, -12f);
        trunk4.transform.localScale = new Vector3(0.42f, 0.65f, 0.42f);
        if (barkMat != null) trunk4.GetComponent<Renderer>().sharedMaterial = barkMat;
        Object.DestroyImmediate(trunk4.GetComponent<Collider>());

        GameObject trunk5 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk5.name = "TrunkNeck";
        trunk5.transform.SetParent(root.transform);
        trunk5.transform.localPosition = new Vector3(0.82f, 4.80f, 0.38f);
        trunk5.transform.localRotation = Quaternion.Euler(25f, 40f, -15f);
        trunk5.transform.localScale = new Vector3(0.36f, 0.50f, 0.36f);
        if (barkMat != null) trunk5.GetComponent<Renderer>().sharedMaterial = barkMat;
        Object.DestroyImmediate(trunk5.GetComponent<Collider>());

        Vector3 crownPos = new Vector3(0.95f, 5.25f, 0.45f);
        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.name = "Crown";
        crown.transform.SetParent(root.transform);
        crown.transform.localPosition = crownPos;
        crown.transform.localScale = new Vector3(0.55f, 0.45f, 0.55f);
        if (barkMat != null) crown.GetComponent<Renderer>().sharedMaterial = barkMat;
        Object.DestroyImmediate(crown.GetComponent<Collider>());

        // 2. 16 Layered Drooping Palm Fronds radiating in 360 degrees
        for (int i = 0; i < 16; i++)
        {
            float angle = i * 22.5f;
            float pitch = (i % 2 == 0) ? 22f : 32f; // Alternating upper and lower drooping tiers
            float length = (i % 2 == 0) ? 2.6f : 2.2f;

            GameObject frond = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frond.name = $"Frond_{i}";
            frond.transform.SetParent(root.transform);

            Quaternion rot = Quaternion.Euler(pitch, angle, -8f);
            frond.transform.localRotation = rot;
            frond.transform.localPosition = crownPos + rot * new Vector3(0f, 0.05f, length * 0.55f);
            frond.transform.localScale = new Vector3(0.68f, 0.07f, length);
            if (leavesMat != null) frond.GetComponent<Renderer>().sharedMaterial = leavesMat;
            Object.DestroyImmediate(frond.GetComponent<Collider>());
        }

        // 3. Cluster of 6 Coconuts under crown
        for (int c = 0; c < 6; c++)
        {
            float cAngle = c * 60f + 12f;
            GameObject coconut = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coconut.name = $"Coconut_{c}";
            coconut.transform.SetParent(root.transform);
            coconut.transform.localPosition = crownPos + Quaternion.Euler(0f, cAngle, 0f) * new Vector3(0.20f, -0.18f, 0.20f);
            coconut.transform.localScale = new Vector3(0.26f, 0.30f, 0.26f);
            if (hairMat != null) coconut.GetComponent<Renderer>().sharedMaterial = hairMat;
            else if (barkMat != null) coconut.GetComponent<Renderer>().sharedMaterial = barkMat;
            Object.DestroyImmediate(coconut.GetComponent<Collider>());
        }

        CapsuleCollider col = root.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0.35f, 2.5f, 0.20f);
        col.radius = 0.60f;
        col.height = 5.2f;

        root.AddComponent<ObstacleCollision>();

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static void CreateRockPrefab()
    {
        string path = $"{PrefabsPath}/Rock.prefab";
        GameObject root = new GameObject("RockObstacle");
        root.tag = "Obstacle";

        Material rockMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/Rock.mat");

        // Main Sedona Canyon Sandstone Boulder
        GameObject main = GameObject.CreatePrimitive(PrimitiveType.Cube);
        main.name = "MainRock";
        main.transform.SetParent(root.transform);
        main.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        main.transform.localRotation = Quaternion.Euler(12f, 28f, 8f);
        main.transform.localScale = new Vector3(1.8f, 1.3f, 1.6f);
        if (rockMat != null) main.GetComponent<Renderer>().sharedMaterial = rockMat;
        Object.DestroyImmediate(main.GetComponent<Collider>());

        // Terraced faceted strata slab
        GameObject sub = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sub.name = "TerraceRock";
        sub.transform.SetParent(root.transform);
        sub.transform.localPosition = new Vector3(0.60f, 0.45f, 0.20f);
        sub.transform.localRotation = Quaternion.Euler(-18f, 52f, 12f);
        sub.transform.localScale = new Vector3(1.3f, 0.90f, 1.2f);
        if (rockMat != null) sub.GetComponent<Renderer>().sharedMaterial = rockMat;
        Object.DestroyImmediate(sub.GetComponent<Collider>());

        // Stepped upper ledge
        GameObject ledge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ledge.name = "LedgeRock";
        ledge.transform.SetParent(root.transform);
        ledge.transform.localPosition = new Vector3(-0.55f, 0.95f, -0.30f);
        ledge.transform.localRotation = Quaternion.Euler(8f, -35f, 15f);
        ledge.transform.localScale = new Vector3(0.90f, 0.60f, 0.80f);
        if (rockMat != null) ledge.GetComponent<Renderer>().sharedMaterial = rockMat;
        Object.DestroyImmediate(ledge.GetComponent<Collider>());

        // Base rubble stones
        GameObject rubble1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rubble1.name = "Rubble1";
        rubble1.transform.SetParent(root.transform);
        rubble1.transform.localPosition = new Vector3(-0.75f, 0.20f, 0.50f);
        rubble1.transform.localRotation = Quaternion.Euler(30f, 12f, 45f);
        rubble1.transform.localScale = new Vector3(0.60f, 0.40f, 0.55f);
        if (rockMat != null) rubble1.GetComponent<Renderer>().sharedMaterial = rockMat;
        Object.DestroyImmediate(rubble1.GetComponent<Collider>());

        GameObject rubble2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rubble2.name = "Rubble2";
        rubble2.transform.SetParent(root.transform);
        rubble2.transform.localPosition = new Vector3(0.70f, 0.18f, -0.55f);
        rubble2.transform.localRotation = Quaternion.Euler(-25f, 42f, 20f);
        rubble2.transform.localScale = new Vector3(0.50f, 0.35f, 0.45f);
        if (rockMat != null) rubble2.GetComponent<Renderer>().sharedMaterial = rockMat;
        Object.DestroyImmediate(rubble2.GetComponent<Collider>());

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.center = new Vector3(0.1f, 0.65f, 0.1f);
        col.size = new Vector3(2.0f, 1.3f, 1.8f);

        root.AddComponent<ObstacleCollision>();

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static void CreateCratePrefab()
    {
        string path = $"{PrefabsPath}/Crate.prefab";
        GameObject root = new GameObject("CrateObstacle");
        root.tag = "Obstacle";

        Material crateMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/Crate.mat");
        Material borderMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/CrateBorder.mat");
        Material stripeMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/RoadStripe.mat");

        // Main Wooden Cargo Box with HD wood texture
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = "Box";
        box.transform.SetParent(root.transform);
        box.transform.localPosition = new Vector3(0f, 0.62f, 0f);
        box.transform.localScale = new Vector3(1.30f, 1.25f, 1.30f);
        if (crateMat != null) box.GetComponent<Renderer>().sharedMaterial = crateMat;
        Object.DestroyImmediate(box.GetComponent<Collider>());

        // Raised Steel Frame Borders (Horizontal & Vertical)
        GameObject frameH = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frameH.name = "SteelFrameH";
        frameH.transform.SetParent(box.transform);
        frameH.transform.localPosition = Vector3.zero;
        frameH.transform.localScale = new Vector3(1.04f, 0.28f, 1.04f);
        if (borderMat != null) frameH.GetComponent<Renderer>().sharedMaterial = borderMat;
        Object.DestroyImmediate(frameH.GetComponent<Collider>());

        GameObject frameV1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frameV1.name = "SteelFrameV1";
        frameV1.transform.SetParent(box.transform);
        frameV1.transform.localPosition = Vector3.zero;
        frameV1.transform.localScale = new Vector3(0.28f, 1.04f, 1.04f);
        if (borderMat != null) frameV1.GetComponent<Renderer>().sharedMaterial = borderMat;
        Object.DestroyImmediate(frameV1.GetComponent<Collider>());

        // Diagonal Cross Bracing Plate
        GameObject brace1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        brace1.name = "CrossBrace1";
        brace1.transform.SetParent(box.transform);
        brace1.transform.localPosition = new Vector3(0f, 0f, 0.51f);
        brace1.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        brace1.transform.localScale = new Vector3(0.12f, 1.4f, 0.04f);
        if (borderMat != null) brace1.GetComponent<Renderer>().sharedMaterial = borderMat;
        Object.DestroyImmediate(brace1.GetComponent<Collider>());

        GameObject brace2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        brace2.name = "CrossBrace2";
        brace2.transform.SetParent(box.transform);
        brace2.transform.localPosition = new Vector3(0f, 0f, 0.51f);
        brace2.transform.localRotation = Quaternion.Euler(0f, 0f, -45f);
        brace2.transform.localScale = new Vector3(0.12f, 1.4f, 0.04f);
        if (borderMat != null) brace2.GetComponent<Renderer>().sharedMaterial = borderMat;
        Object.DestroyImmediate(brace2.GetComponent<Collider>());

        // Yellow & Black Hazard Stripe Band across the front
        GameObject hazardBand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hazardBand.name = "HazardStripeBand";
        hazardBand.transform.SetParent(box.transform);
        hazardBand.transform.localPosition = new Vector3(0f, -0.15f, 0.52f);
        hazardBand.transform.localScale = new Vector3(0.95f, 0.22f, 0.02f);
        if (stripeMat != null) hazardBand.GetComponent<Renderer>().sharedMaterial = stripeMat;
        Object.DestroyImmediate(hazardBand.GetComponent<Collider>());

        // 8 Corner Steel Rivet Bracket Plates
        for (int cx = -1; cx <= 1; cx += 2)
        {
            for (int cy = -1; cy <= 1; cy += 2)
            {
                for (int cz = -1; cz <= 1; cz += 2)
                {
                    GameObject bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bracket.name = $"CornerBracket_{cx}_{cy}_{cz}";
                    bracket.transform.SetParent(box.transform);
                    bracket.transform.localPosition = new Vector3(cx * 0.48f, cy * 0.48f, cz * 0.48f);
                    bracket.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
                    if (borderMat != null) bracket.GetComponent<Renderer>().sharedMaterial = borderMat;
                    Object.DestroyImmediate(bracket.GetComponent<Collider>());
                }
            }
        }

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 0.62f, 0f);
        col.size = new Vector3(1.30f, 1.25f, 1.30f);

        root.AddComponent<ObstacleCollision>();

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static void CreateCactusPrefab()
    {
        string path = $"{PrefabsPath}/Cactus.prefab";
        GameObject root = new GameObject("Cactus");

        Material cactusMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/Cactus.mat");
        Material flowerMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/Coin.mat");

        // Main Trunk: Rounded Capsule with domed top and HD ribbed fluting
        GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        stem.name = "Stem";
        stem.transform.SetParent(root.transform);
        stem.transform.localPosition = new Vector3(0f, 2.0f, 0f);
        stem.transform.localScale = new Vector3(0.64f, 1.15f, 0.64f);
        if (cactusMat != null) stem.GetComponent<Renderer>().sharedMaterial = cactusMat;
        Object.DestroyImmediate(stem.GetComponent<Collider>());

        // Left Branch (Elbow + rising arm)
        GameObject armL1 = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        armL1.name = "ArmL_Elbow";
        armL1.transform.SetParent(root.transform);
        armL1.transform.localPosition = new Vector3(-0.52f, 1.7f, 0f);
        armL1.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        armL1.transform.localScale = new Vector3(0.38f, 0.38f, 0.38f);
        if (cactusMat != null) armL1.GetComponent<Renderer>().sharedMaterial = cactusMat;
        Object.DestroyImmediate(armL1.GetComponent<Collider>());

        GameObject armL2 = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        armL2.name = "ArmL_Rise";
        armL2.transform.SetParent(root.transform);
        armL2.transform.localPosition = new Vector3(-0.90f, 2.4f, 0f);
        armL2.transform.localScale = new Vector3(0.36f, 0.65f, 0.36f);
        if (cactusMat != null) armL2.GetComponent<Renderer>().sharedMaterial = cactusMat;
        Object.DestroyImmediate(armL2.GetComponent<Collider>());

        // Right Branch (Higher up, asymmetric)
        GameObject armR1 = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        armR1.name = "ArmR_Elbow";
        armR1.transform.SetParent(root.transform);
        armR1.transform.localPosition = new Vector3(0.52f, 2.4f, 0f);
        armR1.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        armR1.transform.localScale = new Vector3(0.38f, 0.38f, 0.38f);
        if (cactusMat != null) armR1.GetComponent<Renderer>().sharedMaterial = cactusMat;
        Object.DestroyImmediate(armR1.GetComponent<Collider>());

        GameObject armR2 = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        armR2.name = "ArmR_Rise";
        armR2.transform.SetParent(root.transform);
        armR2.transform.localPosition = new Vector3(0.90f, 3.1f, 0f);
        armR2.transform.localScale = new Vector3(0.36f, 0.75f, 0.36f);
        if (cactusMat != null) armR2.GetComponent<Renderer>().sharedMaterial = cactusMat;
        Object.DestroyImmediate(armR2.GetComponent<Collider>());

        // Blossom flowers on tips
        if (flowerMat != null)
        {
            GameObject fl1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fl1.name = "FlowerCenter";
            fl1.transform.SetParent(root.transform);
            fl1.transform.localPosition = new Vector3(0f, 3.20f, 0f);
            fl1.transform.localScale = new Vector3(0.22f, 0.18f, 0.22f);
            fl1.GetComponent<Renderer>().sharedMaterial = flowerMat;
            Object.DestroyImmediate(fl1.GetComponent<Collider>());

            GameObject fl2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fl2.name = "FlowerLeft";
            fl2.transform.SetParent(root.transform);
            fl2.transform.localPosition = new Vector3(-0.90f, 3.10f, 0f);
            fl2.transform.localScale = new Vector3(0.18f, 0.15f, 0.18f);
            fl2.GetComponent<Renderer>().sharedMaterial = flowerMat;
            Object.DestroyImmediate(fl2.GetComponent<Collider>());

            GameObject fl3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fl3.name = "FlowerRight";
            fl3.transform.SetParent(root.transform);
            fl3.transform.localPosition = new Vector3(0.90f, 3.90f, 0f);
            fl3.transform.localScale = new Vector3(0.18f, 0.15f, 0.18f);
            fl3.GetComponent<Renderer>().sharedMaterial = flowerMat;
            Object.DestroyImmediate(fl3.GetComponent<Collider>());
        }

        CapsuleCollider col = root.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, 1.9f, 0f);
        col.radius = 0.65f;
        col.height = 3.8f;

        root.AddComponent<ObstacleCollision>();

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static void CreateDesertRockPrefab()
    {
        string path = $"{PrefabsPath}/DesertRock.prefab";
        GameObject root = new GameObject("DesertRock");

        Material rockMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/Rock.mat");

        GameObject mainRock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mainRock.name = "MainRockBase";
        mainRock.transform.SetParent(root.transform);
        mainRock.transform.localPosition = new Vector3(0f, 1.4f, 0f);
        mainRock.transform.localRotation = Quaternion.Euler(18f, 32f, 10f);
        mainRock.transform.localScale = new Vector3(3.6f, 2.8f, 3.4f);
        if (rockMat != null) mainRock.GetComponent<Renderer>().sharedMaterial = rockMat;
        Object.DestroyImmediate(mainRock.GetComponent<Collider>());

        GameObject topRock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        topRock.name = "TopRockTerrace";
        topRock.transform.SetParent(root.transform);
        topRock.transform.localPosition = new Vector3(0.50f, 3.0f, -0.30f);
        topRock.transform.localRotation = Quaternion.Euler(-15f, 60f, 18f);
        topRock.transform.localScale = new Vector3(2.4f, 1.8f, 2.2f);
        if (rockMat != null) topRock.GetComponent<Renderer>().sharedMaterial = rockMat;
        Object.DestroyImmediate(topRock.GetComponent<Collider>());

        GameObject sideShelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sideShelf.name = "SideShelf";
        sideShelf.transform.SetParent(root.transform);
        sideShelf.transform.localPosition = new Vector3(-0.80f, 2.2f, 0.50f);
        sideShelf.transform.localRotation = Quaternion.Euler(25f, -20f, 12f);
        sideShelf.transform.localScale = new Vector3(1.8f, 1.2f, 1.6f);
        if (rockMat != null) sideShelf.GetComponent<Renderer>().sharedMaterial = rockMat;
        Object.DestroyImmediate(sideShelf.GetComponent<Collider>());

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static void CreateDesertSegmentPrefab()
    {
        string path = $"{PrefabsPath}/DesertSegment.prefab";
        GameObject root = new GameObject("DesertSegment");

        Material sandMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/DesertSand.mat");
        Material roadMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/DesertRoad.mat");
        Material stripeMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/RoadStripe.mat");
        Material curbMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/DesertCurb.mat");
        Material curbAltMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/DesertCurbAlt.mat");
        Material rockMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/Rock.mat");
        Material poleMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/WoodPole.mat");
        Material wireMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/WireLine.mat");
        Material detailMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/PlayerDetail.mat");
        Material cactusMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/Cactus.mat");
        Material leavesMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/TreeLeaves.mat");

        float segLength = 40f;
        float roadWidth = 10f; // Lanes: Left=-3, Center=0, Right=+3

        // 1. 3-Lane Road surface
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = "RoadPath";
        road.transform.SetParent(root.transform);
        road.transform.localPosition = new Vector3(0f, -0.05f, segLength * 0.5f);
        road.transform.localScale = new Vector3(roadWidth, 0.1f, segLength);
        if (roadMat != null) road.GetComponent<Renderer>().sharedMaterial = roadMat;

        // 2. Lane divider dashed stripes at X = -1.5 and X = +1.5
        int stripeCount = 8;
        float stripeSpacing = segLength / stripeCount;
        for (int s = 0; s < stripeCount; s++)
        {
            float zStripe = (s + 0.5f) * stripeSpacing;

            GameObject stL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stL.name = $"StripeL_{s}";
            stL.transform.SetParent(root.transform);
            stL.transform.localPosition = new Vector3(-1.5f, 0.01f, zStripe);
            stL.transform.localScale = new Vector3(0.18f, 0.04f, stripeSpacing * 0.55f);
            if (stripeMat != null) stL.GetComponent<Renderer>().sharedMaterial = stripeMat;
            Object.DestroyImmediate(stL.GetComponent<Collider>());

            GameObject stR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stR.name = $"StripeR_{s}";
            stR.transform.SetParent(root.transform);
            stR.transform.localPosition = new Vector3(1.5f, 0.01f, zStripe);
            stR.transform.localScale = new Vector3(0.18f, 0.04f, stripeSpacing * 0.55f);
            if (stripeMat != null) stR.GetComponent<Renderer>().sharedMaterial = stripeMat;
            Object.DestroyImmediate(stR.GetComponent<Collider>());
        }

        // 3. Beveled Raised Stone Curbs with Alternating Safety Paint
        GameObject curbL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        curbL.name = "CurbLeft";
        curbL.transform.SetParent(root.transform);
        curbL.transform.localPosition = new Vector3(-roadWidth * 0.5f - 0.25f, 0.08f, segLength * 0.5f);
        curbL.transform.localScale = new Vector3(0.55f, 0.26f, segLength);
        if (curbMat != null) curbL.GetComponent<Renderer>().sharedMaterial = curbMat;

        GameObject curbR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        curbR.name = "CurbRight";
        curbR.transform.SetParent(root.transform);
        curbR.transform.localPosition = new Vector3(roadWidth * 0.5f + 0.25f, 0.08f, segLength * 0.5f);
        curbR.transform.localScale = new Vector3(0.55f, 0.26f, segLength);
        if (curbMat != null) curbR.GetComponent<Renderer>().sharedMaterial = curbMat;

        // Alternating curb blocks
        for (int cb = 0; cb < 4; cb++)
        {
            float zc = (cb * 2 + 1) * (segLength / 8f);
            GameObject blockL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blockL.name = $"CurbBlockL_{cb}";
            blockL.transform.SetParent(root.transform);
            blockL.transform.localPosition = new Vector3(-roadWidth * 0.5f - 0.25f, 0.09f, zc);
            blockL.transform.localScale = new Vector3(0.57f, 0.27f, 2.5f);
            if (curbAltMat != null) blockL.GetComponent<Renderer>().sharedMaterial = curbAltMat;
            Object.DestroyImmediate(blockL.GetComponent<Collider>());

            GameObject blockR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blockR.name = $"CurbBlockR_{cb}";
            blockR.transform.SetParent(root.transform);
            blockR.transform.localPosition = new Vector3(roadWidth * 0.5f + 0.25f, 0.09f, zc);
            blockR.transform.localScale = new Vector3(0.57f, 0.27f, 2.5f);
            if (curbAltMat != null) blockR.GetComponent<Renderer>().sharedMaterial = curbAltMat;
            Object.DestroyImmediate(blockR.GetComponent<Collider>());
        }

        // 4. Undulating Rolling Sand Dunes with HD wind ripples
        GameObject duneL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        duneL.name = "DunesLeft";
        duneL.transform.SetParent(root.transform);
        duneL.transform.localPosition = new Vector3(-16f, -0.05f, segLength * 0.5f);
        duneL.transform.localRotation = Quaternion.Euler(0f, 0f, 4f);
        duneL.transform.localScale = new Vector3(22f, 0.40f, segLength);
        if (sandMat != null) duneL.GetComponent<Renderer>().sharedMaterial = sandMat;

        GameObject duneR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        duneR.name = "DunesRight";
        duneR.transform.SetParent(root.transform);
        duneR.transform.localPosition = new Vector3(16f, -0.05f, segLength * 0.5f);
        duneR.transform.localRotation = Quaternion.Euler(0f, 0f, -4f);
        duneR.transform.localScale = new Vector3(22f, 0.40f, segLength);
        if (sandMat != null) duneR.GetComponent<Renderer>().sharedMaterial = sandMat;

        // 5. Classic American Desert Highway Telegraph / Utility Pole along left roadside verge
        GameObject poleRoot = new GameObject("UtilityPole");
        poleRoot.transform.SetParent(root.transform);
        poleRoot.transform.localPosition = new Vector3(-6.8f, 0f, 20f);

        GameObject poleCol = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        poleCol.name = "PoleShaft";
        poleCol.transform.SetParent(poleRoot.transform);
        poleCol.transform.localPosition = new Vector3(0f, 3.8f, 0f);
        poleCol.transform.localScale = new Vector3(0.22f, 3.8f, 0.22f);
        if (poleMat != null) poleCol.GetComponent<Renderer>().sharedMaterial = poleMat;
        Object.DestroyImmediate(poleCol.GetComponent<Collider>());

        GameObject crossarm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crossarm.name = "CrossArm";
        crossarm.transform.SetParent(poleRoot.transform);
        crossarm.transform.localPosition = new Vector3(0f, 7.2f, 0f);
        crossarm.transform.localScale = new Vector3(2.2f, 0.16f, 0.16f);
        if (poleMat != null) crossarm.GetComponent<Renderer>().sharedMaterial = poleMat;
        Object.DestroyImmediate(crossarm.GetComponent<Collider>());

        // 2 Ceramic insulators on crossarm tips
        GameObject ins1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ins1.name = "Insulator1";
        ins1.transform.SetParent(poleRoot.transform);
        ins1.transform.localPosition = new Vector3(-0.9f, 7.35f, 0f);
        ins1.transform.localScale = new Vector3(0.10f, 0.12f, 0.10f);
        if (detailMat != null) ins1.GetComponent<Renderer>().sharedMaterial = detailMat;
        Object.DestroyImmediate(ins1.GetComponent<Collider>());

        GameObject ins2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ins2.name = "Insulator2";
        ins2.transform.SetParent(poleRoot.transform);
        ins2.transform.localPosition = new Vector3(0.9f, 7.35f, 0f);
        ins2.transform.localScale = new Vector3(0.10f, 0.12f, 0.10f);
        if (detailMat != null) ins2.GetComponent<Renderer>().sharedMaterial = detailMat;
        Object.DestroyImmediate(ins2.GetComponent<Collider>());

        // Overhead highway power wire line spanning along the road
        GameObject wire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wire.name = "OverheadWire";
        wire.transform.SetParent(root.transform);
        wire.transform.localPosition = new Vector3(-7.7f, 7.4f, segLength * 0.5f);
        wire.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        wire.transform.localScale = new Vector3(0.04f, segLength * 0.5f, 0.04f);
        if (wireMat != null) wire.GetComponent<Renderer>().sharedMaterial = wireMat;
        Object.DestroyImmediate(wire.GetComponent<Collider>());

        // 6. Giant Distant Sedona / Monument Valley Canyon Mesas & Spire Towers
        // Left Horizon: Stratified Red-Rock Mesa + Chimney Spire
        GameObject mesaL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mesaL.name = "CanyonMesaLeft";
        mesaL.transform.SetParent(root.transform);
        mesaL.transform.localPosition = new Vector3(-48f, 8.5f, segLength * 0.5f);
        mesaL.transform.localRotation = Quaternion.Euler(3f, 12f, 0f);
        mesaL.transform.localScale = new Vector3(32f, 18f, segLength * 1.05f);
        if (rockMat != null) mesaL.GetComponent<Renderer>().sharedMaterial = rockMat;
        Object.DestroyImmediate(mesaL.GetComponent<Collider>());

        GameObject spireL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        spireL.name = "CanyonSpireLeft";
        spireL.transform.SetParent(root.transform);
        spireL.transform.localPosition = new Vector3(-40f, 12.5f, 12f);
        spireL.transform.localRotation = Quaternion.Euler(-2f, 25f, 4f);
        spireL.transform.localScale = new Vector3(12f, 26f, 14f);
        if (rockMat != null) spireL.GetComponent<Renderer>().sharedMaterial = rockMat;
        Object.DestroyImmediate(spireL.GetComponent<Collider>());

        GameObject shelfL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shelfL.name = "CanyonShelfLeft";
        shelfL.transform.SetParent(root.transform);
        shelfL.transform.localPosition = new Vector3(-54f, 4.2f, 30f);
        shelfL.transform.localScale = new Vector3(20f, 9f, 22f);
        if (rockMat != null) shelfL.GetComponent<Renderer>().sharedMaterial = rockMat;
        Object.DestroyImmediate(shelfL.GetComponent<Collider>());

        // Right Horizon: Monument Valley Flat-Topped Butte + Sandstone Spire Tower
        GameObject mesaR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mesaR.name = "CanyonMesaRight";
        mesaR.transform.SetParent(root.transform);
        mesaR.transform.localPosition = new Vector3(48f, 10.5f, segLength * 0.5f);
        mesaR.transform.localRotation = Quaternion.Euler(-4f, -18f, 0f);
        mesaR.transform.localScale = new Vector3(35f, 22f, segLength * 1.05f);
        if (rockMat != null) mesaR.GetComponent<Renderer>().sharedMaterial = rockMat;
        Object.DestroyImmediate(mesaR.GetComponent<Collider>());

        GameObject spireR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        spireR.name = "CanyonSpireRight";
        spireR.transform.SetParent(root.transform);
        spireR.transform.localPosition = new Vector3(42f, 13.5f, 26f);
        spireR.transform.localRotation = Quaternion.Euler(2f, -30f, -3f);
        spireR.transform.localScale = new Vector3(14f, 28f, 16f);
        if (rockMat != null) spireR.GetComponent<Renderer>().sharedMaterial = rockMat;
        Object.DestroyImmediate(spireR.GetComponent<Collider>());

        GameObject shelfR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shelfR.name = "CanyonShelfRight";
        shelfR.transform.SetParent(root.transform);
        shelfR.transform.localPosition = new Vector3(54f, 4.0f, 8f);
        shelfR.transform.localScale = new Vector3(22f, 9f, 24f);
        if (rockMat != null) shelfR.GetComponent<Renderer>().sharedMaterial = rockMat;
        Object.DestroyImmediate(shelfR.GetComponent<Collider>());

        // 7. Scenic Roadside Desert Flora Clumps (Saguaro Cacti & Sandstone Boulders)
        // Roadside Saguaro Cactus on left dune
        GameObject sideCactusL = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        sideCactusL.name = "SceneryCactusL";
        sideCactusL.transform.SetParent(root.transform);
        sideCactusL.transform.localPosition = new Vector3(-13.5f, 1.8f, 14f);
        sideCactusL.transform.localScale = new Vector3(0.55f, 1.8f, 0.55f);
        if (cactusMat != null) sideCactusL.GetComponent<Renderer>().sharedMaterial = cactusMat;
        Object.DestroyImmediate(sideCactusL.GetComponent<Collider>());

        // Roadside Saguaro Cactus on right dune
        GameObject sideCactusR = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        sideCactusR.name = "SceneryCactusR";
        sideCactusR.transform.SetParent(root.transform);
        sideCactusR.transform.localPosition = new Vector3(14.5f, 1.8f, 30f);
        sideCactusR.transform.localScale = new Vector3(0.55f, 1.8f, 0.55f);
        if (cactusMat != null) sideCactusR.GetComponent<Renderer>().sharedMaterial = cactusMat;
        Object.DestroyImmediate(sideCactusR.GetComponent<Collider>());

        // Roadside Sedona Sandstone Boulder on left dune
        GameObject sideRockL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sideRockL.name = "SceneryRockL";
        sideRockL.transform.SetParent(root.transform);
        sideRockL.transform.localPosition = new Vector3(-17.5f, 0.8f, 28f);
        sideRockL.transform.localRotation = Quaternion.Euler(25f, 40f, 15f);
        sideRockL.transform.localScale = new Vector3(2.4f, 1.6f, 2.2f);
        if (rockMat != null) sideRockL.GetComponent<Renderer>().sharedMaterial = rockMat;
        Object.DestroyImmediate(sideRockL.GetComponent<Collider>());

        // Roadside Sedona Sandstone Boulder on right dune
        GameObject sideRockR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sideRockR.name = "SceneryRockR";
        sideRockR.transform.SetParent(root.transform);
        sideRockR.transform.localPosition = new Vector3(17.5f, 0.8f, 9f);
        sideRockR.transform.localRotation = Quaternion.Euler(-18f, 62f, 22f);
        sideRockR.transform.localScale = new Vector3(2.2f, 1.5f, 2.0f);
        if (rockMat != null) sideRockR.GetComponent<Renderer>().sharedMaterial = rockMat;
        Object.DestroyImmediate(sideRockR.GetComponent<Collider>());

        // 8. Roadside Distance Kilometer Marker Posts
        for (int m = 0; m < 2; m++)
        {
            float zm = (m + 0.5f) * (segLength / 2f);
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = $"MilePost_{m}";
            post.transform.SetParent(root.transform);
            post.transform.localPosition = new Vector3(-roadWidth * 0.5f - 0.7f, 0.5f, zm);
            post.transform.localScale = new Vector3(0.12f, 0.5f, 0.12f);
            if (curbMat != null) post.GetComponent<Renderer>().sharedMaterial = curbMat;
            Object.DestroyImmediate(post.GetComponent<Collider>());

            // Green highway sign plate on post
            GameObject signPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signPlate.name = $"MilePlate_{m}";
            signPlate.transform.SetParent(post.transform);
            signPlate.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            signPlate.transform.localScale = new Vector3(2.5f, 1.2f, 0.4f);
            if (leavesMat != null) signPlate.GetComponent<Renderer>().sharedMaterial = leavesMat;
            Object.DestroyImmediate(signPlate.GetComponent<Collider>());
        }

        // End anchor point
        GameObject endPoint = new GameObject("EndPoint");
        endPoint.transform.SetParent(root.transform);
        endPoint.transform.localPosition = new Vector3(0f, 0f, segLength);

        // Segment Manager
        DesertSegmentManager sm = root.AddComponent<DesertSegmentManager>();
        SerializedObject so = new SerializedObject(sm);
        so.FindProperty("segmentLength").floatValue = segLength;
        so.FindProperty("endPoint").objectReferenceValue = endPoint.transform;
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    public static void RefreshActiveScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying) return;

        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.isLoaded) return;

        Debug.Log($">>> [Desert Runner] Refreshing active scene: {activeScene.name}...");

        // 1. Refresh Player instance in active scene
        PlayerMovement existingPm = Object.FindAnyObjectByType<PlayerMovement>();
        if (existingPm != null)
        {
            Vector3 playerPos = existingPm.transform.position;
            Quaternion playerRot = existingPm.transform.rotation;
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Player.prefab");
            if (playerPrefab != null)
            {
                Object.DestroyImmediate(existingPm.gameObject);
                GameObject newPlayer = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                newPlayer.name = "Player";
                newPlayer.transform.position = playerPos;
                newPlayer.transform.rotation = playerRot;

                // Re-wire CameraController
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    CameraController camCtrl = mainCam.GetComponent<CameraController>();
                    if (camCtrl != null)
                    {
                        SerializedObject camSo = new SerializedObject(camCtrl);
                        camSo.FindProperty("target").objectReferenceValue = newPlayer.transform;
                        camSo.ApplyModifiedProperties();
                    }
                }

                // Re-wire GameManager and EndlessSpawnSystem
                GameManager gm = Object.FindAnyObjectByType<GameManager>();
                if (gm != null)
                {
                    SerializedObject gmSo = new SerializedObject(gm);
                    gmSo.FindProperty("player").objectReferenceValue = newPlayer.GetComponent<PlayerMovement>();
                    gmSo.ApplyModifiedProperties();
                }

                EndlessSpawnSystem spawnSys = Object.FindAnyObjectByType<EndlessSpawnSystem>();
                if (spawnSys != null)
                {
                    SerializedObject spawnSo = new SerializedObject(spawnSys);
                    spawnSo.FindProperty("playerTransform").objectReferenceValue = newPlayer.transform;
                    spawnSo.ApplyModifiedProperties();
                }
            }
        }

        // 2. Refresh Starter Segments in active scene
        GameObject starter0 = GameObject.Find("StarterSegment_0");
        GameObject starter1 = GameObject.Find("StarterSegment_1");
        GameObject segPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/DesertSegment.prefab");
        if (segPrefab != null)
        {
            if (starter0 != null)
            {
                Vector3 p0 = starter0.transform.position;
                Object.DestroyImmediate(starter0);
                GameObject s0 = (GameObject)PrefabUtility.InstantiatePrefab(segPrefab);
                s0.name = "StarterSegment_0";
                s0.transform.position = p0;
            }
            if (starter1 != null)
            {
                Vector3 p1 = starter1.transform.position;
                Object.DestroyImmediate(starter1);
                GameObject s1 = (GameObject)PrefabUtility.InstantiatePrefab(segPrefab);
                s1.name = "StarterSegment_1";
                s1.transform.position = p1;
            }
        }

        // 3. Skybox, Fog, and Lighting
        FixSkyboxAndSceneVisuals();

        if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
        }
        Debug.Log(">>> [Desert Runner] Active scene refreshed and saved successfully!");
    }

    public static void BuildMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.96f, 0.78f, 0.52f);
        camObj.AddComponent<AudioListener>();
        camObj.transform.position = new Vector3(0f, 2f, -5f);

        GameObject lightObj = new GameObject("Sun Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1.0f, 0.95f, 0.85f);
        light.intensity = 1.3f;
        lightObj.transform.rotation = Quaternion.Euler(40f, -30f, 0f);

        CreateAudioManagerObject();

        GameObject canvasObj = CreateCanvas("MainMenuCanvas");
        GameObject bgPanel = CreatePanel(canvasObj, "BackgroundPanel", new Color(0.12f, 0.08f, 0.05f, 0.80f));
        SetAnchorStretch(bgPanel.GetComponent<RectTransform>());

        GameObject titleObj = CreateText(canvasObj, "TitleText", "DESERT RUNNER", 64, FontStyle.Bold, new Color(1.0f, 0.85f, 0.15f), TextAnchor.MiddleCenter);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0f, 175f);
        titleRt.sizeDelta = new Vector2(900f, 100f);

        GameObject subObj = CreateText(canvasObj, "SubtitleText", "RUN • COLLECT • AVOID • SURVIVE", 24, FontStyle.Bold, new Color(0.95f, 0.95f, 0.95f), TextAnchor.MiddleCenter);
        RectTransform subRt = subObj.GetComponent<RectTransform>();
        subRt.anchoredPosition = new Vector2(0f, 105f);
        subRt.sizeDelta = new Vector2(900f, 40f);

        GameObject playBtn = CreateButton(canvasObj, "PlayButton", "PLAY", new Vector2(0f, 10f), new Vector2(280f, 70f), new Color(0.20f, 0.70f, 0.25f));
        GameObject infoBtn = CreateButton(canvasObj, "HowToPlayButton", "HOW TO PLAY", new Vector2(0f, -80f), new Vector2(280f, 60f), new Color(0.22f, 0.52f, 0.82f));
        GameObject quitBtn = CreateButton(canvasObj, "QuitButton", "QUIT", new Vector2(0f, -160f), new Vector2(280f, 55f), new Color(0.45f, 0.25f, 0.25f));

        GameObject footerObj = CreateText(canvasObj, "FooterText", "B.Tech College Game Development Project • 3D Endless Runner", 16, FontStyle.Italic, new Color(0.85f, 0.85f, 0.85f, 0.85f), TextAnchor.MiddleCenter);
        RectTransform footerRt = footerObj.GetComponent<RectTransform>();
        footerRt.anchoredPosition = new Vector2(0f, -280f);
        footerRt.sizeDelta = new Vector2(900f, 40f);

        MainMenuController mmc = canvasObj.AddComponent<MainMenuController>();
        SerializedObject so = new SerializedObject(mmc);
        so.FindProperty("playButton").objectReferenceValue = playBtn.GetComponent<Button>();
        so.FindProperty("howToPlayButton").objectReferenceValue = infoBtn.GetComponent<Button>();
        so.FindProperty("quitButton").objectReferenceValue = quitBtn.GetComponent<Button>();
        so.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, $"{ScenesPath}/MainMenu.unity");
    }

    public static void BuildInformationScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.96f, 0.78f, 0.52f);
        camObj.AddComponent<AudioListener>();

        GameObject lightObj = new GameObject("Sun Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(40f, -30f, 0f);

        GameObject canvasObj = CreateCanvas("InfoCanvas");
        GameObject bgPanel = CreatePanel(canvasObj, "BgPanel", new Color(0.12f, 0.08f, 0.05f, 0.88f));
        SetAnchorStretch(bgPanel.GetComponent<RectTransform>());

        GameObject titleObj = CreateText(canvasObj, "Title", "HOW TO PLAY", 48, FontStyle.Bold, new Color(1.0f, 0.85f, 0.15f), TextAnchor.MiddleCenter);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0f, 220f);
        titleRt.sizeDelta = new Vector2(800f, 60f);

        string instructions = 
            "🏃  PLAYER AUTOMATICALLY RUNS FORWARD\n\n" +
            "⬅️  LEFT ARROW / A  —  MOVE LEFT (3 LANES)\n" +
            "➡️  RIGHT ARROW / D  —  MOVE RIGHT (3 LANES)\n\n" +
            "🪙  COLLECT COINS  (+10 SCORE)\n" +
            "💎  COLLECT GEMS   (+50 SCORE)\n\n" +
            "🌳  AVOID TREES   •   🪨 AVOID ROCKS   •   📦 AVOID CRATES\n\n" +
            "💥  HITTING AN OBSTACLE ENDS THE RUN\n\n" +
            "🏆  RUN AS FAR AS POSSIBLE FOR A HIGH SCORE!";

        GameObject contentObj = CreateText(canvasObj, "InstructionsText", instructions, 22, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter);
        RectTransform contentRt = contentObj.GetComponent<RectTransform>();
        contentRt.anchoredPosition = new Vector2(0f, 20f);
        contentRt.sizeDelta = new Vector2(900f, 320f);

        GameObject backBtn = CreateButton(canvasObj, "BackButton", "BACK TO MENU", new Vector2(0f, -220f), new Vector2(260f, 60f), new Color(0.85f, 0.40f, 0.15f));

        InformationController ic = canvasObj.AddComponent<InformationController>();
        SerializedObject so = new SerializedObject(ic);
        so.FindProperty("backButton").objectReferenceValue = backBtn.GetComponent<Button>();
        so.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Information.unity");
    }

    public static void BuildStageSelectScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.96f, 0.78f, 0.52f);
        camObj.AddComponent<AudioListener>();

        GameObject lightObj = new GameObject("Sun Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(40f, -30f, 0f);

        GameObject canvasObj = CreateCanvas("StageSelectCanvas");
        GameObject bgPanel = CreatePanel(canvasObj, "BgPanel", new Color(0.12f, 0.08f, 0.05f, 0.88f));
        SetAnchorStretch(bgPanel.GetComponent<RectTransform>());

        GameObject titleObj = CreateText(canvasObj, "Title", "SELECT STAGE", 48, FontStyle.Bold, new Color(1.0f, 0.85f, 0.15f), TextAnchor.MiddleCenter);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0f, 210f);
        titleRt.sizeDelta = new Vector2(800f, 60f);

        GameObject stage1Btn = CreateButton(canvasObj, "DesertRunButton", "DESERT RUN\n[ AVAILABLE - PLAY NOW ]", new Vector2(0f, 70f), new Vector2(420f, 80f), new Color(0.20f, 0.70f, 0.25f));
        GameObject stage2Btn = CreateButton(canvasObj, "CanyonRunButton", "CANYON RUN\n[ 🔒 LOCKED ]", new Vector2(0f, -20f), new Vector2(420f, 65f), new Color(0.35f, 0.35f, 0.35f));
        GameObject stage3Btn = CreateButton(canvasObj, "NightRunButton", "NIGHT RUN\n[ 🔒 LOCKED ]", new Vector2(0f, -100f), new Vector2(420f, 65f), new Color(0.35f, 0.35f, 0.35f));

        GameObject backBtn = CreateButton(canvasObj, "BackButton", "BACK", new Vector2(0f, -210f), new Vector2(240f, 55f), new Color(0.75f, 0.30f, 0.20f));

        StageSelectController ssc = canvasObj.AddComponent<StageSelectController>();
        SerializedObject so = new SerializedObject(ssc);
        so.FindProperty("desertRunButton").objectReferenceValue = stage1Btn.GetComponent<Button>();
        so.FindProperty("canyonRunButton").objectReferenceValue = stage2Btn.GetComponent<Button>();
        so.FindProperty("nightRunButton").objectReferenceValue = stage3Btn.GetComponent<Button>();
        so.FindProperty("backButton").objectReferenceValue = backBtn.GetComponent<Button>();
        so.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, $"{ScenesPath}/StageSelect.unity");
    }

    public static void BuildGamePlayScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Sunlight
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1.0f, 0.96f, 0.88f);
        light.intensity = 1.35f;
        light.shadows = LightShadows.Soft;
        lightObj.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

        // Secondary fill light to reduce harsh shadows
        GameObject fillLightObj = new GameObject("Fill Light");
        Light fillLight = fillLightObj.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.color = new Color(0.85f, 0.72f, 0.55f);
        fillLight.intensity = 0.4f;
        fillLight.shadows = LightShadows.None;
        fillLightObj.transform.rotation = Quaternion.Euler(30f, 150f, 0f);

        // Warm Desert Atmospheric Fog
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.95f, 0.78f, 0.52f);
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.007f;

        // Desert skybox setup using panoramic texture
        Texture2D skyTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{MaterialsPath}/Textures/DesertSky_Panorama.png");
        if (skyTex != null)
        {
            Shader skyShader = Shader.Find("Skybox/Panoramic");
            if (skyShader == null) skyShader = Shader.Find("Skybox/6 Sided");
            if (skyShader != null)
            {
                string skyMatPath = $"{MaterialsPath}/DesertSkybox.mat";
                Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(skyMatPath);
                if (skyMat == null)
                {
                    skyMat = new Material(skyShader);
                    AssetDatabase.CreateAsset(skyMat, skyMatPath);
                }
                skyMat.SetTexture("_MainTex", skyTex);
                if (skyMat.HasProperty("_Tint")) skyMat.SetColor("_Tint", new Color(1f, 0.95f, 0.88f));
                if (skyMat.HasProperty("_Exposure")) skyMat.SetFloat("_Exposure", 1.1f);
                EditorUtility.SetDirty(skyMat);
                RenderSettings.skybox = skyMat;
            }
        }

        // Ambient lighting warm desert tones
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.95f, 0.85f, 0.65f);
        RenderSettings.ambientEquatorColor = new Color(0.82f, 0.65f, 0.42f);
        RenderSettings.ambientGroundColor = new Color(0.55f, 0.38f, 0.22f);

        // Player Instance
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Player.prefab");
        GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        playerInstance.name = "Player";
        playerInstance.transform.position = new Vector3(0f, 0f, 0f);

        // Third-Person Camera with clear view of all 3 lanes
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.fieldOfView = 58f;
        camObj.AddComponent<AudioListener>();
        CameraController camCtrl = camObj.AddComponent<CameraController>();
        SerializedObject camSo = new SerializedObject(camCtrl);
        camSo.FindProperty("target").objectReferenceValue = playerInstance.transform;
        camSo.FindProperty("offset").vector3Value = new Vector3(0f, 2.8f, -4.8f);
        camSo.FindProperty("lookAheadOffset").vector3Value = new Vector3(0f, 1.4f, 9.0f);
        camSo.ApplyModifiedProperties();

        CreateAudioManagerObject();

        // Core Systems
        GameObject systemsObj = new GameObject("--- GAME SYSTEMS ---");
        ScoreManager scoreMgr = systemsObj.AddComponent<ScoreManager>();
        GameManager gameMgr = systemsObj.AddComponent<GameManager>();
        RestartManager restartMgr = systemsObj.AddComponent<RestartManager>();
        EndlessSpawnSystem spawnSys = systemsObj.AddComponent<EndlessSpawnSystem>();

        SerializedObject spawnSo = new SerializedObject(spawnSys);
        spawnSo.FindProperty("playerTransform").objectReferenceValue = playerInstance.transform;
        spawnSo.FindProperty("desertSegmentPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/DesertSegment.prefab");
        spawnSo.FindProperty("treeObstaclePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Tree.prefab");
        spawnSo.FindProperty("rockObstaclePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Rock.prefab");
        spawnSo.FindProperty("crateObstaclePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Crate.prefab");
        spawnSo.FindProperty("coinPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Coin.prefab");
        spawnSo.FindProperty("gemPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Gem.prefab");
        spawnSo.FindProperty("cactusPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Cactus.prefab");
        spawnSo.FindProperty("desertRockPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/DesertRock.prefab");
        spawnSo.ApplyModifiedProperties();

        SerializedObject gmSo = new SerializedObject(gameMgr);
        gmSo.FindProperty("player").objectReferenceValue = playerInstance.GetComponent<PlayerMovement>();
        gmSo.ApplyModifiedProperties();

        // Pre-place starter segments so scene is immediately visible in Editor
        GameObject segPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/DesertSegment.prefab");
        if (segPrefab != null)
        {
            GameObject starter1 = (GameObject)PrefabUtility.InstantiatePrefab(segPrefab);
            starter1.name = "StarterSegment_0";
            starter1.transform.position = new Vector3(0f, 0f, 0f);

            GameObject starter2 = (GameObject)PrefabUtility.InstantiatePrefab(segPrefab);
            starter2.name = "StarterSegment_1";
            starter2.transform.position = new Vector3(0f, 0f, 40f);
        }

        // --- SUBWAY SURFERS STYLE HUD ---
        GameObject canvasObj = CreateCanvas("GameplayCanvas");
        UIManager uiMgr = canvasObj.AddComponent<UIManager>();

        // Top-Left: Rounded Coin & Gem Counters
        GameObject hudPanelTL = CreatePanel(canvasObj, "HUD_TopLeft", new Color(0.08f, 0.08f, 0.08f, 0.65f));
        RectTransform tlRt = hudPanelTL.GetComponent<RectTransform>();
        tlRt.anchorMin = new Vector2(0f, 1f);
        tlRt.anchorMax = new Vector2(0f, 1f);
        tlRt.pivot = new Vector2(0f, 1f);
        tlRt.anchoredPosition = new Vector2(25f, -25f);
        tlRt.sizeDelta = new Vector2(240f, 95f);

        GameObject coinsTextObj = CreateText(hudPanelTL, "CoinsText", "🪙 COINS: 0", 22, FontStyle.Bold, new Color(1.0f, 0.85f, 0.1f), TextAnchor.MiddleLeft);
        RectTransform coinsRt = coinsTextObj.GetComponent<RectTransform>();
        coinsRt.anchoredPosition = new Vector2(15f, 20f);
        coinsRt.sizeDelta = new Vector2(210f, 35f);

        GameObject gemsTextObj = CreateText(hudPanelTL, "GemsText", "💎 GEMS: 0", 22, FontStyle.Bold, new Color(0.15f, 0.95f, 0.95f), TextAnchor.MiddleLeft);
        RectTransform gemsRt = gemsTextObj.GetComponent<RectTransform>();
        gemsRt.anchoredPosition = new Vector2(15f, -20f);
        gemsRt.sizeDelta = new Vector2(210f, 35f);

        // Top-Right: Distance Meter
        GameObject hudPanelTR = CreatePanel(canvasObj, "HUD_TopRight", new Color(0.08f, 0.08f, 0.08f, 0.65f));
        RectTransform trRt = hudPanelTR.GetComponent<RectTransform>();
        trRt.anchorMin = new Vector2(1f, 1f);
        trRt.anchorMax = new Vector2(1f, 1f);
        trRt.pivot = new Vector2(1f, 1f);
        trRt.anchoredPosition = new Vector2(-25f, -25f);
        trRt.sizeDelta = new Vector2(250f, 55f);

        GameObject distTextObj = CreateText(hudPanelTR, "DistanceText", "🏃 DISTANCE: 0 m", 22, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        RectTransform distRt = distTextObj.GetComponent<RectTransform>();
        distRt.anchoredPosition = Vector2.zero;
        distRt.sizeDelta = new Vector2(240f, 45f);

        // Top-Center: Score Badge
        GameObject hudPanelTC = CreatePanel(canvasObj, "HUD_Score", new Color(0.08f, 0.08f, 0.08f, 0.65f));
        RectTransform tcRt = hudPanelTC.GetComponent<RectTransform>();
        tcRt.anchorMin = new Vector2(0.5f, 1f);
        tcRt.anchorMax = new Vector2(0.5f, 1f);
        tcRt.pivot = new Vector2(0.5f, 1f);
        tcRt.anchoredPosition = new Vector2(0f, -25f);
        tcRt.sizeDelta = new Vector2(290f, 60f);

        GameObject scoreTextObj = CreateText(hudPanelTC, "ScoreText", "SCORE: 0", 28, FontStyle.Bold, new Color(1.0f, 0.88f, 0.15f), TextAnchor.MiddleCenter);
        RectTransform scoreRt = scoreTextObj.GetComponent<RectTransform>();
        scoreRt.anchoredPosition = Vector2.zero;
        scoreRt.sizeDelta = new Vector2(280f, 50f);

        // --- GAME OVER MODAL WINDOW ---
        GameObject gameOverPanel = CreatePanel(canvasObj, "GameOverPanel", new Color(0.06f, 0.04f, 0.04f, 0.92f));
        SetAnchorStretch(gameOverPanel.GetComponent<RectTransform>());

        GameObject goTitle = CreateText(gameOverPanel, "GameOverTitle", "GAME OVER", 58, FontStyle.Bold, new Color(0.95f, 0.25f, 0.20f), TextAnchor.MiddleCenter);
        RectTransform goTitleRt = goTitle.GetComponent<RectTransform>();
        goTitleRt.anchoredPosition = new Vector2(0f, 185f);
        goTitleRt.sizeDelta = new Vector2(700f, 80f);

        GameObject goScore = CreateText(gameOverPanel, "FinalScoreText", "FINAL SCORE\n0", 36, FontStyle.Bold, new Color(1.0f, 0.88f, 0.15f), TextAnchor.MiddleCenter);
        RectTransform goScoreRt = goScore.GetComponent<RectTransform>();
        goScoreRt.anchoredPosition = new Vector2(0f, 95f);
        goScoreRt.sizeDelta = new Vector2(700f, 85f);

        GameObject goDist = CreateText(gameOverPanel, "FinalDistanceText", "DISTANCE: 0 m", 24, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        RectTransform goDistRt = goDist.GetComponent<RectTransform>();
        goDistRt.anchoredPosition = new Vector2(0f, 22f);
        goDistRt.sizeDelta = new Vector2(700f, 40f);

        GameObject goCoins = CreateText(gameOverPanel, "FinalCoinsText", "COINS: 0", 24, FontStyle.Bold, new Color(1.0f, 0.85f, 0.1f), TextAnchor.MiddleCenter);
        RectTransform goCoinsRt = goCoins.GetComponent<RectTransform>();
        goCoinsRt.anchoredPosition = new Vector2(0f, -22f);
        goCoinsRt.sizeDelta = new Vector2(700f, 40f);

        GameObject goGems = CreateText(gameOverPanel, "FinalGemsText", "GEMS: 0", 24, FontStyle.Bold, new Color(0.15f, 0.95f, 0.95f), TextAnchor.MiddleCenter);
        RectTransform goGemsRt = goGems.GetComponent<RectTransform>();
        goGemsRt.anchoredPosition = new Vector2(0f, -66f);
        goGemsRt.sizeDelta = new Vector2(700f, 40f);

        GameObject restartBtn = CreateButton(gameOverPanel, "RestartButton", "TAP TO RESTART", new Vector2(0f, -145f), new Vector2(300f, 70f), new Color(0.20f, 0.72f, 0.25f));
        GameObject menuBtn = CreateButton(gameOverPanel, "MainMenuButton", "MAIN MENU", new Vector2(0f, -225f), new Vector2(300f, 52f), new Color(0.38f, 0.35f, 0.35f));

        // Wire UIManager
        SerializedObject uiSo = new SerializedObject(uiMgr);
        uiSo.FindProperty("scoreText").objectReferenceValue = scoreTextObj.GetComponent<Text>();
        uiSo.FindProperty("distanceText").objectReferenceValue = distTextObj.GetComponent<Text>();
        uiSo.FindProperty("coinsText").objectReferenceValue = coinsTextObj.GetComponent<Text>();
        uiSo.FindProperty("gemsText").objectReferenceValue = gemsTextObj.GetComponent<Text>();

        uiSo.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
        uiSo.FindProperty("finalScoreText").objectReferenceValue = goScore.GetComponent<Text>();
        uiSo.FindProperty("finalDistanceText").objectReferenceValue = goDist.GetComponent<Text>();
        uiSo.FindProperty("finalCoinsText").objectReferenceValue = goCoins.GetComponent<Text>();
        uiSo.FindProperty("finalGemsText").objectReferenceValue = goGems.GetComponent<Text>();
        uiSo.FindProperty("restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        uiSo.FindProperty("mainMenuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSo.ApplyModifiedProperties();

        gameOverPanel.SetActive(false);

        EditorSceneManager.SaveScene(scene, $"{ScenesPath}/GamePlay.unity");
    }

    private static GameObject CreateAudioManagerObject()
    {
        GameObject audioObj = new GameObject("AudioManager");
        AudioManager am = audioObj.AddComponent<AudioManager>();

        AudioSource musicSrc = audioObj.AddComponent<AudioSource>();
        musicSrc.loop = true;
        musicSrc.playOnAwake = false;

        AudioSource sfxSrc = audioObj.AddComponent<AudioSource>();
        sfxSrc.loop = false;
        sfxSrc.playOnAwake = false;

        SerializedObject so = new SerializedObject(am);
        so.FindProperty("musicSource").objectReferenceValue = musicSrc;
        so.FindProperty("sfxSource").objectReferenceValue = sfxSrc;

        so.FindProperty("desertMusicClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioPath}/desert_music.wav");
        so.FindProperty("coinCollectClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioPath}/coin_collect.wav");
        so.FindProperty("gemCollectClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioPath}/gem_collect.wav");
        so.FindProperty("obstacleCrashClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioPath}/obstacle_crash.wav");
        so.FindProperty("buttonClickClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioPath}/button_click.wav");
        so.FindProperty("gameOverClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioPath}/game_over.wav");
        so.ApplyModifiedProperties();

        return audioObj;
    }

    private static GameObject CreateCanvas(string name)
    {
        EnsureEventSystem();
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        return canvasObj;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
#if ENABLE_INPUT_SYSTEM
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
        }
    }

    private static GameObject CreatePanel(GameObject parent, string name, Color color)
    {
        GameObject panelObj = new GameObject(name);
        panelObj.transform.SetParent(parent.transform, false);

        Image img = panelObj.AddComponent<Image>();
        img.color = color;

        return panelObj;
    }

    private static GameObject CreateText(GameObject parent, string name, string text, int fontSize, FontStyle style, Color color, TextAnchor align)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);

        Text txt = textObj.AddComponent<Text>();
        txt.text = text;
        txt.fontSize = fontSize;
        txt.fontStyle = style;
        txt.color = color;
        txt.alignment = align;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return textObj;
    }

    private static GameObject CreateButton(GameObject parent, string name, string label, Vector2 pos, Vector2 size, Color btnColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent.transform, false);

        Image img = btnObj.AddComponent<Image>();
        img.color = btnColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = btnColor;
        colors.highlightedColor = btnColor * 1.2f;
        colors.pressedColor = btnColor * 0.8f;
        btn.colors = colors;

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        GameObject txtObj = CreateText(btnObj, "Text", label, 20, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchoredPosition = Vector2.zero;
        txtRt.sizeDelta = size;

        return btnObj;
    }

    private static void SetAnchorStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public static void SetupBuildSettings()
    {
        string[] scenePaths = {
            $"{ScenesPath}/MainMenu.unity",
            $"{ScenesPath}/Information.unity",
            $"{ScenesPath}/StageSelect.unity",
            $"{ScenesPath}/GamePlay.unity"
        };

        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[scenePaths.Length];
        for (int i = 0; i < scenePaths.Length; i++)
        {
            scenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
        }

        EditorBuildSettings.scenes = scenes;
    }
}
