using UnityEngine;

/// <summary>
/// Rotates and gently bobs collectible items (Coins and Gems) in 3D space.
/// </summary>
public class CollectibleRotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Rotation speed in degrees per second around each axis")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 120f, 0f);

    [Header("Floating / Bobbing Settings")]
    [Tooltip("Enable vertical hovering bob motion")]
    [SerializeField] private bool enableBobbing = true;
    [Tooltip("Bobbing amplitude (height delta)")]
    [SerializeField] private float bobHeight = 0.2f;
    [Tooltip("Bobbing frequency (speed)")]
    [SerializeField] private float bobSpeed = 3f;

    private Vector3 initialPosition;
    private float randomOffset;

    private void Start()
    {
        initialPosition = transform.position;
        // Random phase offset so multiple collectibles don't bob identically
        randomOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        // Smooth rotation
        transform.Rotate(rotationSpeed * Time.deltaTime, Space.World);

        // Gentle floating bob
        if (enableBobbing)
        {
            float newY = initialPosition.y + Mathf.Sin(Time.time * bobSpeed + randomOffset) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}
