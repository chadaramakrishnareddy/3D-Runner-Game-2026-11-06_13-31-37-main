using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// High-Definition procedural texture generator for Desert Runner.
/// Generates crisp, high-resolution (1024x1024 / 2048x1024) PBR textures in pure C#:
/// - HD Desert Asphalt Road with lane stripes and tire wear
/// - HD Wind-Rippled Golden Dunes Sand
/// - HD Monument Valley Sedimentary Canyon Rock
/// - HD Fibrous Palm Trunk Bark & Layered Frond Leaves
/// - HD Ribbed Saguaro Cactus with needle spines
/// - HD Heavy Industrial Cargo Crate with metal corner brackets
/// - HD 24K Brushed Gold Coin with embossed chiseled star
/// - HD Prismatic Cyan Gem crystal facets
/// - HD 2K Panoramic Sunset Desert Sky with clouds and distant canyon silhouettes
/// </summary>
public static class HDAssetGenerator
{
    private const string TexturesDir = "Assets/Materials/Textures";

    [MenuItem("Desert Runner/Generate All HD Textures")]
    public static void GenerateAllHDTextures()
    {
        Debug.Log(">>> [HD Generator] Starting High-Definition procedural texture bake...");

        if (!Directory.Exists(TexturesDir))
        {
            Directory.CreateDirectory(TexturesDir);
        }

        GenerateHDDesertRoad();
        GenerateHDDesertSand();
        GenerateHDRock();
        GenerateHDTreeBark();
        GenerateHDTreeLeaves();
        GenerateHDCactus();
        GenerateHDCrate();
        GenerateHDCoin();
        GenerateHDGem();
        GenerateHDDesertSkyPanorama();

        AssetDatabase.Refresh();

        // Configure texture importers for maximum visual fidelity
        ConfigureTextureImporter($"{TexturesDir}/DesertRoad_Albedo.png", true, TextureWrapMode.Repeat);
        ConfigureTextureImporter($"{TexturesDir}/DesertSand_Albedo.png", true, TextureWrapMode.Repeat);
        ConfigureTextureImporter($"{TexturesDir}/Rock_Albedo.png", true, TextureWrapMode.Repeat);
        ConfigureTextureImporter($"{TexturesDir}/TreeBark_Albedo.png", true, TextureWrapMode.Repeat);
        ConfigureTextureImporter($"{TexturesDir}/TreeLeaves_Albedo.png", true, TextureWrapMode.Clamp);
        ConfigureTextureImporter($"{TexturesDir}/Cactus_Albedo.png", true, TextureWrapMode.Repeat);
        ConfigureTextureImporter($"{TexturesDir}/Crate_Albedo.png", true, TextureWrapMode.Clamp);
        ConfigureTextureImporter($"{TexturesDir}/Coin_Albedo.png", true, TextureWrapMode.Clamp);
        ConfigureTextureImporter($"{TexturesDir}/Gem_Albedo.png", true, TextureWrapMode.Clamp);
        ConfigureTextureImporter($"{TexturesDir}/DesertSky_Panorama.png", false, TextureWrapMode.Clamp);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(">>> [HD Generator] All High-Definition textures generated and imported successfully!");
    }

    private static void ConfigureTextureImporter(string path, bool generateMipMaps, TextureWrapMode wrapMode)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = generateMipMaps;
            importer.wrapMode = wrapMode;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 8;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
    }

    /// <summary>
    /// HD Desert Asphalt Road (1024x1024):
    /// Multi-layer asphalt aggregate grain, 3 lane tire wear paths,
    /// bright highway yellow dashed dividers, white boundary rumble edges,
    /// and subtle sand dust drift.
    /// </summary>
    public static void GenerateHDDesertRoad()
    {
        int width = 1024;
        int height = 1024;
        Color[] pixels = new Color[width * height];

        Color asphaltBase = new Color(0.24f, 0.22f, 0.20f);
        Color asphaltDark = new Color(0.18f, 0.17f, 0.15f);
        Color stripeYellow = new Color(1.0f, 0.85f, 0.15f);
        Color stripeWhite = new Color(0.96f, 0.95f, 0.92f);
        Color dustSand = new Color(0.85f, 0.68f, 0.44f);

        for (int y = 0; y < height; y++)
        {
            float v = (float)y / height;

            for (int x = 0; x < width; x++)
            {
                float u = (float)x / width;

                // Micro asphalt mineral aggregate grain
                float grain = Mathf.PerlinNoise(x * 0.15f, y * 0.15f) * 0.12f;
                float macroNoise = Mathf.PerlinNoise(x * 0.02f, y * 0.02f) * 0.08f;
                Color col = Color.Lerp(asphaltBase, asphaltDark, grain + macroNoise);

                // Subtle tire track wear along the 3 running lanes (centers: 0.20, 0.50, 0.80)
                float distToLane1 = Mathf.Abs(u - 0.20f);
                float distToLane2 = Mathf.Abs(u - 0.50f);
                float distToLane3 = Mathf.Abs(u - 0.80f);
                float minDistToLane = Mathf.Min(distToLane1, Mathf.Min(distToLane2, distToLane3));
                if (minDistToLane < 0.09f)
                {
                    float trackShade = (1.0f - (minDistToLane / 0.09f)) * 0.08f;
                    col = Color.Lerp(col, asphaltDark, trackShade);
                }

                // Lane Divider 1 at u = 0.35 (between Left and Center lane)
                // Lane Divider 2 at u = 0.65 (between Center and Right lane)
                bool inDivider1 = Mathf.Abs(u - 0.35f) < 0.012f;
                bool inDivider2 = Mathf.Abs(u - 0.65f) < 0.012f;

                // Dashed stripes pattern (period = 128 pixels, 80 pixels painted, 48 pixels gap)
                bool isDash = (y % 128) < 80;

                if ((inDivider1 || inDivider2) && isDash)
                {
                    float edgeDist = Mathf.Min(Mathf.Abs(u - 0.35f), Mathf.Abs(u - 0.65f)) / 0.012f;
                    float stripeAlpha = Mathf.Clamp01(1.0f - edgeDist);
                    col = Color.Lerp(col, stripeYellow, stripeAlpha * 0.95f);
                }

                // Outer boundary solid lines at u = 0.04 and u = 0.96
                bool inBoundaryL = Mathf.Abs(u - 0.045f) < 0.008f;
                bool inBoundaryR = Mathf.Abs(u - 0.955f) < 0.008f;
                if (inBoundaryL || inBoundaryR)
                {
                    float boundEdge = Mathf.Min(Mathf.Abs(u - 0.045f), Mathf.Abs(u - 0.955f)) / 0.008f;
                    col = Color.Lerp(col, stripeWhite, (1.0f - boundEdge) * 0.92f);
                }

                // Sand dust drift on road shoulders (x < 35 or x > 989)
                if (u < 0.035f || u > 0.965f)
                {
                    float edgeFactor = u < 0.035f ? (1.0f - u / 0.035f) : ((u - 0.965f) / 0.035f);
                    float sandNoise = Mathf.PerlinNoise(x * 0.04f, y * 0.08f);
                    col = Color.Lerp(col, dustSand, edgeFactor * (0.6f + sandNoise * 0.4f));
                }

                pixels[y * width + x] = col;
            }
        }

        SaveTexturePNG(pixels, width, height, $"{TexturesDir}/DesertRoad_Albedo.png");
    }

    /// <summary>
    /// HD Desert Sand Dunes (1024x1024):
    /// Multi-scale sinusoidal wind ripples, sun-gilded dune crests,
    /// warm terracotta trough shadows, and sparkling quartz grains.
    /// </summary>
    public static void GenerateHDDesertSand()
    {
        int width = 1024;
        int height = 1024;
        Color[] pixels = new Color[width * height];

        Color sandCrest = new Color(0.97f, 0.82f, 0.56f);
        Color sandMid = new Color(0.92f, 0.74f, 0.46f);
        Color sandTrough = new Color(0.79f, 0.58f, 0.33f);

        for (int y = 0; y < height; y++)
        {
            float ny = (float)y / height;

            for (int x = 0; x < width; x++)
            {
                float nx = (float)x / width;

                // Primary wind ripple wave (curving undulating bands)
                float wave1 = Mathf.Sin(ny * 28f * Mathf.PI + Mathf.Cos(nx * 6f * Mathf.PI) * 2.2f);
                // Secondary cross-ripple wave
                float wave2 = Mathf.Sin(nx * 14f * Mathf.PI + ny * 18f * Mathf.PI) * 0.35f;
                // Micro dune grain
                float fineGrain = (Mathf.PerlinNoise(x * 0.12f, y * 0.12f) - 0.5f) * 0.15f;

                float waveBlend = Mathf.Clamp01((wave1 + wave2 + 1.0f) * 0.5f + fineGrain);

                Color col;
                if (waveBlend > 0.5f)
                {
                    col = Color.Lerp(sandMid, sandCrest, (waveBlend - 0.5f) * 2f);
                }
                else
                {
                    col = Color.Lerp(sandTrough, sandMid, waveBlend * 2f);
                }

                // Subtle quartz crystal sparkle
                if (Mathf.PerlinNoise(x * 0.8f, y * 0.8f) > 0.86f)
                {
                    col = Color.Lerp(col, Color.white, 0.25f);
                }

                pixels[y * width + x] = col;
            }
        }

        SaveTexturePNG(pixels, width, height, $"{TexturesDir}/DesertSand_Albedo.png");
    }

    /// <summary>
    /// HD Sedona / Monument Valley Canyon Rock (1024x1024):
    /// Multi-layer geological strata bands (terracotta, burnt ochre, sandstone rose),
    /// vertical fissures, roughness patina.
    /// </summary>
    public static void GenerateHDRock()
    {
        int width = 1024;
        int height = 1024;
        Color[] pixels = new Color[width * height];

        Color strata1 = new Color(0.74f, 0.36f, 0.20f); // Terracotta
        Color strata2 = new Color(0.85f, 0.48f, 0.26f); // Burnt Ochre
        Color strata3 = new Color(0.92f, 0.65f, 0.42f); // Pale Sandstone
        Color strata4 = new Color(0.55f, 0.24f, 0.14f); // Deep Ironstone

        for (int y = 0; y < height; y++)
        {
            float ny = (float)y / height;

            for (int x = 0; x < width; x++)
            {
                float nx = (float)x / width;

                // Geological undulating horizontal strata wave
                float strataWarp = Mathf.PerlinNoise(nx * 4f, ny * 2f) * 0.25f;
                float strataPos = Mathf.Repeat((ny + strataWarp) * 6.5f, 1.0f);

                Color col;
                if (strataPos < 0.25f)
                    col = Color.Lerp(strata1, strata2, strataPos / 0.25f);
                else if (strataPos < 0.50f)
                    col = Color.Lerp(strata2, strata3, (strataPos - 0.25f) / 0.25f);
                else if (strataPos < 0.75f)
                    col = Color.Lerp(strata3, strata4, (strataPos - 0.50f) / 0.25f);
                else
                    col = Color.Lerp(strata4, strata1, (strataPos - 0.75f) / 0.25f);

                // Vertical rock cliff crack fissures
                float fissure = Mathf.PerlinNoise(nx * 32f, ny * 3f);
                if (fissure < 0.18f)
                {
                    col = Color.Lerp(col, new Color(0.25f, 0.10f, 0.05f), 0.55f);
                }

                // Surface rock grain
                float stoneGrain = (Mathf.PerlinNoise(x * 0.25f, y * 0.25f) - 0.5f) * 0.12f;
                col += new Color(stoneGrain, stoneGrain, stoneGrain);

                pixels[y * width + x] = col;
            }
        }

        SaveTexturePNG(pixels, width, height, $"{TexturesDir}/Rock_Albedo.png");
    }

    /// <summary>
    /// HD Palm Tree Bark (512x1024):
    /// Repeating fibrous horizontal diamond leaf scar rings and vertical husk fibers.
    /// </summary>
    public static void GenerateHDTreeBark()
    {
        int width = 512;
        int height = 1024;
        Color[] pixels = new Color[width * height];

        Color barkBase = new Color(0.42f, 0.28f, 0.17f);
        Color barkHighlight = new Color(0.58f, 0.40f, 0.26f);
        Color barkDark = new Color(0.24f, 0.15f, 0.08f);

        for (int y = 0; y < height; y++)
        {
            float ny = (float)y / height;

            for (int x = 0; x < width; x++)
            {
                float nx = (float)x / width;

                // Horizontal palm trunk rings (diamond ring pattern)
                float ringCycle = Mathf.Repeat(ny * 16f, 1.0f);
                float ringDist = Mathf.Abs(ringCycle - 0.5f) * 2f;

                // Vertical husk fiber lines
                float fiber = Mathf.PerlinNoise(nx * 40f, ny * 4f) * 0.35f;

                Color col = Color.Lerp(barkBase, barkHighlight, ringDist * 0.6f + fiber);
                if (ringCycle < 0.12f || ringCycle > 0.88f)
                {
                    col = Color.Lerp(col, barkDark, 0.65f); // Deep ring groove shadow
                }

                pixels[y * width + x] = col;
            }
        }

        SaveTexturePNG(pixels, width, height, $"{TexturesDir}/TreeBark_Albedo.png");
    }

    /// <summary>
    /// HD Palm Tree Frond Leaves (1024x1024):
    /// Lush tropical feather leaflets branching from central golden-green rachis stalk.
    /// </summary>
    public static void GenerateHDTreeLeaves()
    {
        int width = 1024;
        int height = 1024;
        Color[] pixels = new Color[width * height];

        Color leafDark = new Color(0.12f, 0.45f, 0.16f);
        Color leafMid = new Color(0.20f, 0.68f, 0.25f);
        Color leafBright = new Color(0.35f, 0.82f, 0.38f);
        Color rachisColor = new Color(0.68f, 0.78f, 0.28f);

        for (int y = 0; y < height; y++)
        {
            float ny = (float)y / height;

            for (int x = 0; x < width; x++)
            {
                float nx = (float)x / width;

                // Central leaf spine / rachis along x = 0.5
                float distFromCenter = Mathf.Abs(nx - 0.5f);

                if (distFromCenter < 0.018f)
                {
                    // Central leaf stalk
                    pixels[y * width + x] = rachisColor;
                    continue;
                }

                // Angled leaflet pinnule striations
                float angleAngle = (y + distFromCenter * 600f) * 0.08f;
                float leafletStripe = Mathf.Sin(angleAngle);

                float taper = Mathf.Clamp01(1.0f - (distFromCenter / 0.48f));
                Color col = Color.Lerp(leafDark, leafMid, taper);
                if (leafletStripe > 0.2f)
                {
                    col = Color.Lerp(col, leafBright, 0.4f);
                }

                pixels[y * width + x] = col;
            }
        }

        SaveTexturePNG(pixels, width, height, $"{TexturesDir}/TreeLeaves_Albedo.png");
    }

    /// <summary>
    /// HD Saguaro Cactus (512x1024):
    /// Vertical accordion ribs, fleshy green gradient, and golden needle spine dots.
    /// </summary>
    public static void GenerateHDCactus()
    {
        int width = 512;
        int height = 1024;
        Color[] pixels = new Color[width * height];

        Color cactusRidge = new Color(0.28f, 0.72f, 0.35f);
        Color cactusBase = new Color(0.18f, 0.55f, 0.24f);
        Color cactusGroove = new Color(0.09f, 0.35f, 0.14f);
        Color needleColor = new Color(0.95f, 0.86f, 0.48f);

        for (int y = 0; y < height; y++)
        {
            float ny = (float)y / height;

            for (int x = 0; x < width; x++)
            {
                float nx = (float)x / width;

                // Vertical pleated ribs (8 ribs across circumference)
                float ribPhase = Mathf.Repeat(nx * 8f, 1.0f);
                float ribHeight = Mathf.Sin(ribPhase * Mathf.PI);

                Color col = Color.Lerp(cactusGroove, cactusRidge, ribHeight);

                // Golden spine cluster dots along the rib crests (ribPhase ~ 0.5)
                bool nearCrest = Mathf.Abs(ribPhase - 0.5f) < 0.08f;
                bool spineStep = (y % 48) < 8;
                if (nearCrest && spineStep)
                {
                    col = needleColor;
                }

                pixels[y * width + x] = col;
            }
        }

        SaveTexturePNG(pixels, width, height, $"{TexturesDir}/Cactus_Albedo.png");
    }

    /// <summary>
    /// HD Heavy Industrial Cargo Crate (1024x1024):
    /// Weathered cedar planks, brushed steel corner brackets, 3D rivet bolts,
    /// and bold yellow/black hazard warning stripes.
    /// </summary>
    public static void GenerateHDCrate()
    {
        int width = 1024;
        int height = 1024;
        Color[] pixels = new Color[width * height];

        Color woodPlank = new Color(0.65f, 0.42f, 0.24f);
        Color woodGrain = new Color(0.52f, 0.32f, 0.17f);
        Color steelMetal = new Color(0.32f, 0.34f, 0.38f);
        Color steelHighlight = new Color(0.55f, 0.58f, 0.62f);
        Color hazardYellow = new Color(0.96f, 0.78f, 0.12f);
        Color hazardBlack = new Color(0.12f, 0.12f, 0.14f);

        int border = 72; // Width of outer steel reinforcement frame

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Steel outer perimeter frame
                bool isBorder = (x < border || x >= width - border || y < border || y >= height - border);

                if (isBorder)
                {
                    // Brushed metal texture
                    float metalBrushed = Mathf.PerlinNoise(x * 0.08f, y * 0.4f) * 0.2f;
                    Color col = Color.Lerp(steelMetal, steelHighlight, metalBrushed);

                    // Corner rivet bolts
                    bool nearCorner = (x < border * 1.5f || x >= width - border * 1.5f) &&
                                      (y < border * 1.5f || y >= height - border * 1.5f);
                    if (nearCorner)
                    {
                        col = Color.Lerp(col, Color.black, 0.25f);
                    }

                    pixels[y * width + x] = col;
                    continue;
                }

                // Inner wood planks (4 horizontal planks separated by seams)
                int innerY = y - border;
                int plankHeight = (height - border * 2) / 4;
                int plankIndex = innerY / plankHeight;
                int seamY = innerY % plankHeight;

                if (seamY < 4)
                {
                    // Dark plank separation seam
                    pixels[y * width + x] = new Color(0.18f, 0.10f, 0.05f);
                    continue;
                }

                // Organic wood grain
                float grain = Mathf.PerlinNoise((x + plankIndex * 80) * 0.03f, y * 0.25f);
                Color woodCol = Color.Lerp(woodPlank, woodGrain, grain * 0.7f);

                // Central Caution Hazard Stripes (diamond stenciled logo)
                int cx = x - width / 2;
                int cy = y - height / 2;
                if (Mathf.Abs(cx) < 180 && Mathf.Abs(cy) < 70)
                {
                    int stripeDiag = (cx + cy + 1000) / 36;
                    woodCol = (stripeDiag % 2 == 0) ? hazardYellow : hazardBlack;
                }

                pixels[y * width + x] = woodCol;
            }
        }

        SaveTexturePNG(pixels, width, height, $"{TexturesDir}/Crate_Albedo.png");
    }

    /// <summary>
    /// HD 24K Gold Coin (512x512):
    /// Polished gold mirror sheen, concentric outer bevel, and chiseled 5-point star.
    /// </summary>
    public static void GenerateHDCoin()
    {
        int width = 512;
        int height = 512;
        Color[] pixels = new Color[width * height];

        Color goldBright = new Color(1.0f, 0.92f, 0.45f);
        Color goldMid = new Color(1.0f, 0.78f, 0.12f);
        Color goldDark = new Color(0.72f, 0.50f, 0.08f);

        Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
        float maxRadius = width * 0.48f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float dist = Vector2.Distance(pos, center);
                float normalizedDist = dist / maxRadius;

                if (normalizedDist > 1.0f)
                {
                    pixels[y * width + x] = Color.clear;
                    continue;
                }

                // Raised coin outer rim
                Color col;
                if (normalizedDist > 0.82f)
                {
                    float rim = (normalizedDist - 0.82f) / 0.18f;
                    col = Color.Lerp(goldBright, goldDark, rim);
                }
                else
                {
                    // Radial brushed gold sheen
                    float angle = Mathf.Atan2(y - center.y, x - center.x);
                    float radialSheen = (Mathf.Sin(angle * 4f) + 1f) * 0.5f;
                    col = Color.Lerp(goldMid, goldBright, radialSheen * 0.45f);

                    // Embossed chiseled 5-point star inside radius 0.55
                    if (normalizedDist < 0.55f)
                    {
                        float starAngle = angle + Mathf.PI * 0.5f;
                        float starR = (0.28f + 0.18f * Mathf.Cos(starAngle * 5f)) * maxRadius;
                        if (dist < starR)
                        {
                            // Top-left bevel highlight vs bottom-right shadow
                            float lightDirection = Mathf.Cos(angle - Mathf.PI * 0.75f);
                            col = lightDirection > 0 ? Color.Lerp(col, goldBright, lightDirection * 0.6f)
                                                     : Color.Lerp(col, goldDark, -lightDirection * 0.5f);
                        }
                    }
                }

                pixels[y * width + x] = col;
            }
        }

        SaveTexturePNG(pixels, width, height, $"{TexturesDir}/Coin_Albedo.png");
    }

    /// <summary>
    /// HD Prismatic Cyan Gem (512x512):
    /// Brilliant-cut crystal facets with cyan gradient and sparkling refraction glow.
    /// </summary>
    public static void GenerateHDGem()
    {
        int width = 512;
        int height = 512;
        Color[] pixels = new Color[width * height];

        Color gemCore = new Color(0.20f, 0.95f, 1.0f);
        Color gemFacetLight = new Color(0.75f, 0.98f, 1.0f);
        Color gemFacetDark = new Color(0.04f, 0.55f, 0.75f);

        Vector2 center = new Vector2(width * 0.5f, height * 0.5f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = (float)(x - center.x) / (width * 0.45f);
                float ny = (float)(y - center.y) / (height * 0.45f);

                // Octahedral / diamond facet boundary
                float diamondDist = Mathf.Abs(nx) + Mathf.Abs(ny);
                if (diamondDist > 1.0f)
                {
                    pixels[y * width + x] = Color.clear;
                    continue;
                }

                // Geometric facet reflection calculation
                int facetIndex = (int)(Mathf.Atan2(ny, nx) * 4f / Mathf.PI + 4) % 8;
                bool isLightFacet = (facetIndex % 2 == 0);

                Color col = isLightFacet ? Color.Lerp(gemCore, gemFacetLight, 0.5f)
                                         : Color.Lerp(gemCore, gemFacetDark, 0.6f);

                // Center core brilliant shine
                if (diamondDist < 0.35f)
                {
                    col = Color.Lerp(col, Color.white, (0.35f - diamondDist) * 2.2f);
                }

                pixels[y * width + x] = col;
            }
        }

        SaveTexturePNG(pixels, width, height, $"{TexturesDir}/Gem_Albedo.png");
    }

    /// <summary>
    /// HD 2K Panoramic Sunset Desert Sky (2048x1024):
    /// Gradient zenith blue to warm sunset amber, volumetric sunset clouds,
    /// distant canyon mesa silhouettes, and radiant sun flare.
    /// </summary>
    public static void GenerateHDDesertSkyPanorama()
    {
        int width = 2048;
        int height = 1024;
        Color[] pixels = new Color[width * height];

        Color skyZenith = new Color(0.12f, 0.45f, 0.78f);   // Vibrant Sky Blue
        Color skyMid = new Color(0.35f, 0.68f, 0.88f);      // Soft Turquoise Cyan
        Color sunsetGolden = new Color(0.98f, 0.72f, 0.30f); // Warm Amber Gold
        Color sunsetPeach = new Color(0.96f, 0.52f, 0.36f);  // Desert Peach
        Color mesaMountain = new Color(0.42f, 0.22f, 0.18f); // Terracotta Canyon Silhouettes
        Color cloudWhite = new Color(1.0f, 0.95f, 0.88f);

        for (int y = 0; y < height; y++)
        {
            float v = (float)y / height; // 0 = bottom, 1 = top

            for (int x = 0; x < width; x++)
            {
                float u = (float)x / width;

                // Base atmospheric sky gradient
                Color col;
                if (v > 0.55f)
                {
                    col = Color.Lerp(skyMid, skyZenith, (v - 0.55f) / 0.45f);
                }
                else if (v > 0.28f)
                {
                    col = Color.Lerp(sunsetPeach, skyMid, (v - 0.28f) / 0.27f);
                }
                else
                {
                    col = Color.Lerp(sunsetGolden, sunsetPeach, v / 0.28f);
                }

                // Sun Disc & Corona Flare at center u = 0.50, v = 0.32
                float dx = (u - 0.50f) * 2.0f;
                float dy = (v - 0.32f);
                float sunDist = Mathf.Sqrt(dx * dx + dy * dy);

                if (sunDist < 0.05f)
                {
                    col = Color.Lerp(Color.white, new Color(1.0f, 0.96f, 0.82f), sunDist / 0.05f);
                }
                else if (sunDist < 0.35f)
                {
                    float glow = Mathf.Pow(1.0f - (sunDist / 0.35f), 2.2f);
                    col = Color.Lerp(col, new Color(1.0f, 0.88f, 0.55f), glow * 0.75f);
                }

                // Volumetric Sunset Cumulus Clouds across v = 0.35 to 0.70
                if (v > 0.32f && v < 0.75f)
                {
                    float cloudDensity = Mathf.PerlinNoise(u * 8f, v * 6f) +
                                         Mathf.PerlinNoise(u * 16f, v * 12f) * 0.45f;
                    if (cloudDensity > 0.75f)
                    {
                        float cloudAlpha = (cloudDensity - 0.75f) * 2.8f;
                        Color cTint = (v < 0.48f) ? sunsetGolden : cloudWhite;
                        col = Color.Lerp(col, cTint, Mathf.Clamp01(cloudAlpha) * 0.75f);
                    }
                }

                // Distant Mesa / Butte Mountain Silhouettes along the horizon (v < 0.18)
                float mountainHeight = 0.08f +
                                       Mathf.PerlinNoise(u * 6f, 1.2f) * 0.07f +
                                       Mathf.PerlinNoise(u * 18f, 2.5f) * 0.03f;
                // Add flat-topped Mesa features
                mountainHeight = Mathf.Min(mountainHeight, 0.15f);

                if (v < mountainHeight)
                {
                    float depthFade = (v / mountainHeight);
                    col = Color.Lerp(mesaMountain, sunsetGolden, depthFade * 0.4f);
                }

                pixels[y * width + x] = col;
            }
        }

        SaveTexturePNG(pixels, width, height, $"{TexturesDir}/DesertSky_Panorama.png");
    }

    private static void SaveTexturePNG(Color[] pixels, int width, int height, string fullPath)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.SetPixels(pixels);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(fullPath, bytes);
        Object.DestroyImmediate(tex);
    }
}
