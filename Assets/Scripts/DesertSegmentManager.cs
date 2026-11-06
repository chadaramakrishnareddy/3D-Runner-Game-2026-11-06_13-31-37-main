using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attached to individual DesertSegment prefabs.
/// Manages segment length, connection points, and active spawned objects (obstacles, collectibles).
/// </summary>
public class DesertSegmentManager : MonoBehaviour
{
    [Header("Segment Dimensions")]
    [Tooltip("Length of this road segment in meters along Z")]
    [SerializeField] private float segmentLength = 40.0f;

    [Header("Spawn Points / Anchors")]
    [SerializeField] private Transform endPoint;

    // Track objects spawned on this segment so they are cleanly destroyed when segment is removed
    private List<GameObject> spawnedItems = new List<GameObject>();

    public float SegmentLength => segmentLength;

    public Vector3 EndPosition
    {
        get
        {
            if (endPoint != null) return endPoint.position;
            return transform.position + new Vector3(0f, 0f, segmentLength);
        }
    }

    /// <summary>
    /// Registers an obstacle or collectible created on this segment.
    /// </summary>
    public void RegisterSpawnedItem(GameObject item)
    {
        if (item != null)
        {
            item.transform.SetParent(this.transform);
            spawnedItems.Add(item);
        }
    }

    /// <summary>
    /// Cleans up this segment and all its registered items.
    /// </summary>
    public void ClearSegment()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        spawnedItems.Clear();
        Destroy(gameObject);
    }
}
