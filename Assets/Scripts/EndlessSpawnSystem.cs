using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controls procedural endless generation of desert terrain segments,
/// obstacles (trees, rocks, crates), collectibles (coins, gems), and side desert scenery.
/// Ensures fair generation rules and progressive difficulty.
/// </summary>
public class EndlessSpawnSystem : MonoBehaviour
{
    public static EndlessSpawnSystem Instance { get; private set; }

    [Header("Player Tracking")]
    [SerializeField] private Transform playerTransform;

    [Header("Terrain Prefabs")]
    [SerializeField] private GameObject desertSegmentPrefab;
    [SerializeField] private float segmentLength = 40f;
    [SerializeField] private int initialSegments = 6;
    [SerializeField] private int safeInitialSegments = 2;

    [Header("Obstacle Prefabs")]
    [SerializeField] private GameObject treeObstaclePrefab;
    [SerializeField] private GameObject rockObstaclePrefab;
    [SerializeField] private GameObject crateObstaclePrefab;
    [Header("Track Obstacle Prefabs (Integrated from 3D Runner)")]
    [SerializeField] private GameObject[] additionalObstaclePrefabs;

    [Header("Collectible Prefabs")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject gemPrefab;

    [Header("Scenery Prefabs (Side Props)")]
    [SerializeField] private GameObject cactusPrefab;
    [SerializeField] private GameObject desertRockPrefab;

    [Header("Lane Setup")]
    [SerializeField] private float laneDistance = 3.0f; // Lanes at X = -3, 0, 3

    // Tracking active segments
    private List<DesertSegmentManager> activeSegments = new List<DesertSegmentManager>();
    private float nextSpawnZ = 0f;
    private int segmentsSpawnedCount = 0;

    private readonly float[] lanePositions = new float[] { -3.0f, 0f, 3.0f };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            if (player != null) playerTransform = player.transform;
        }

        lanePositions[0] = -laneDistance;
        lanePositions[1] = 0f;
        lanePositions[2] = laneDistance;

        InitializeSpawning();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Check if we need to spawn ahead
        float distanceToSpawnPoint = nextSpawnZ - playerTransform.position.z;
        if (distanceToSpawnPoint < (initialSegments * segmentLength))
        {
            SpawnSegment();
        }

        // Clean up segments that are well behind the player
        if (activeSegments.Count > 0)
        {
            DesertSegmentManager oldest = activeSegments[0];
            if (oldest != null && (playerTransform.position.z - oldest.transform.position.z) > (segmentLength * 1.8f))
            {
                activeSegments.RemoveAt(0);
                oldest.ClearSegment();
            }
        }
    }

    /// <summary>
    /// Spawns the initial strip of segments to begin the game smoothly.
    /// </summary>
    public void InitializeSpawning()
    {
        ClearAllSegments();
        nextSpawnZ = 0f;
        segmentsSpawnedCount = 0;

        for (int i = 0; i < initialSegments; i++)
        {
            SpawnSegment();
        }
    }

    /// <summary>
    /// Spawns a single desert road segment and populates obstacles/collectibles according to difficulty.
    /// </summary>
    private void SpawnSegment()
    {
        if (desertSegmentPrefab == null) return;

        Vector3 spawnPos = new Vector3(0f, 0f, nextSpawnZ);
        GameObject segmentObj = Instantiate(desertSegmentPrefab, spawnPos, Quaternion.identity, transform);
        DesertSegmentManager segmentMgr = segmentObj.GetComponent<DesertSegmentManager>();

        if (segmentMgr == null)
        {
            segmentMgr = segmentObj.AddComponent<DesertSegmentManager>();
        }

        activeSegments.Add(segmentMgr);

        // First few segments are safe from obstacles
        bool isSafeSegment = segmentsSpawnedCount < safeInitialSegments;

        if (!isSafeSegment)
        {
            PopulateSegment(segmentMgr, nextSpawnZ);
        }

        // Always add ambient desert side scenery
        SpawnSideDecorations(segmentMgr, nextSpawnZ);

        nextSpawnZ += segmentLength;
        segmentsSpawnedCount++;
    }

    /// <summary>
    /// Populates obstacles and collectibles on a segment based on distance traveled.
    /// Follows strict fairness rules: NEVER block all 3 lanes.
    /// </summary>
    private void PopulateSegment(DesertSegmentManager segment, float startZ)
    {
        float currentDistance = playerTransform != null ? playerTransform.position.z : startZ;

        // Determine difficulty stage
        // EASY: < 200m
        // MEDIUM: 200m - 500m
        // HARD: > 500m
        int obstacleRowCount = 1;
        int maxObstaclesPerRow = 1;

        if (currentDistance > 500f)
        {
            obstacleRowCount = 2;
            maxObstaclesPerRow = 2; // Hard: up to 2 obstacles per row, guaranteed 1 safe lane
        }
        else if (currentDistance > 200f)
        {
            obstacleRowCount = Random.value < 0.6f ? 2 : 1;
            maxObstaclesPerRow = Random.value < 0.5f ? 2 : 1;
        }

        float rowSpacing = segmentLength / (obstacleRowCount + 1);

        for (int r = 0; r < obstacleRowCount; r++)
        {
            float rowZ = startZ + (r + 1) * rowSpacing;

            // Pick 1 or 2 obstacle lanes, ensuring at least one lane is ALWAYS completely open
            List<int> availableLanes = new List<int> { 0, 1, 2 };
            List<int> obstacleLanes = new List<int>();

            int countToPlace = Mathf.Min(maxObstaclesPerRow, Random.Range(1, 3)); // 1 or 2
            if (countToPlace > 2) countToPlace = 2; // Strict safety rule

            for (int i = 0; i < countToPlace; i++)
            {
                int pickedIndex = Random.Range(0, availableLanes.Count);
                obstacleLanes.Add(availableLanes[pickedIndex]);
                availableLanes.RemoveAt(pickedIndex);
            }

            // Spawn obstacles in picked lanes
            foreach (int laneIdx in obstacleLanes)
            {
                float x = lanePositions[laneIdx];
                Vector3 obstaclePos = new Vector3(x, 0.5f, rowZ);
                GameObject obstaclePrefab = PickRandomObstacle();
                if (obstaclePrefab != null)
                {
                    GameObject obsInstance = Instantiate(obstaclePrefab, obstaclePos, Quaternion.identity);
                    segment.RegisterSpawnedItem(obsInstance);
                }
            }

            // In one of the SAFE lanes, spawn a line of coins or a gem!
            if (availableLanes.Count > 0)
            {
                int safeLane = availableLanes[Random.Range(0, availableLanes.Count)];
                float safeX = lanePositions[safeLane];

                // 25% chance for a bonus Gem on medium/hard, otherwise a nice trail of Coins
                bool spawnGem = (currentDistance > 150f) && (Random.value < 0.25f);

                if (spawnGem && gemPrefab != null)
                {
                    Vector3 gemPos = new Vector3(safeX, 1.0f, rowZ);
                    GameObject gemObj = Instantiate(gemPrefab, gemPos, Quaternion.identity);
                    segment.RegisterSpawnedItem(gemObj);
                }
                else if (coinPrefab != null)
                {
                    // Spawn a series of 3 coins along the safe lane
                    for (int c = -1; c <= 1; c++)
                    {
                        Vector3 coinPos = new Vector3(safeX, 0.8f, rowZ + (c * 2.5f));
                        GameObject coinObj = Instantiate(coinPrefab, coinPos, Quaternion.identity);
                        segment.RegisterSpawnedItem(coinObj);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Spawns decorative desert cacti and rocks on the dunes outside the road.
    /// </summary>
    private void SpawnSideDecorations(DesertSegmentManager segment, float startZ)
    {
        int decorCount = Random.Range(3, 6);
        for (int i = 0; i < decorCount; i++)
        {
            float side = (Random.value > 0.5f) ? 1f : -1f;
            float xPos = side * Random.Range(6.5f, 16f); // Outside lane borders
            float zPos = startZ + Random.Range(2f, segmentLength - 2f);

            GameObject decorPrefab = (Random.value > 0.4f) ? cactusPrefab : desertRockPrefab;
            if (decorPrefab != null)
            {
                Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                float scaleMod = Random.Range(0.8f, 1.4f);
                GameObject decorInstance = Instantiate(decorPrefab, new Vector3(xPos, 0f, zPos), rot);
                decorInstance.transform.localScale = decorPrefab.transform.localScale * scaleMod;
                segment.RegisterSpawnedItem(decorInstance);
            }
        }
    }

    private GameObject PickRandomObstacle()
    {
        List<GameObject> allObstacles = new List<GameObject>();
        if (treeObstaclePrefab != null) allObstacles.Add(treeObstaclePrefab);
        if (rockObstaclePrefab != null) allObstacles.Add(rockObstaclePrefab);
        if (crateObstaclePrefab != null) allObstacles.Add(crateObstaclePrefab);

        if (additionalObstaclePrefabs != null)
        {
            foreach (var obs in additionalObstaclePrefabs)
            {
                if (obs != null) allObstacles.Add(obs);
            }
        }

        if (allObstacles.Count > 0)
        {
            return allObstacles[Random.Range(0, allObstacles.Count)];
        }

        return null;
    }

    /// <summary>
    /// Clears all existing active segments from the scene.
    /// </summary>
    public void ClearAllSegments()
    {
        foreach (var seg in activeSegments)
        {
            if (seg != null)
            {
                seg.ClearSegment();
            }
        }
        activeSegments.Clear();
    }
}
