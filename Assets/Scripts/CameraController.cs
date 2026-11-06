using UnityEngine;

/// <summary>
/// Third-person camera that smoothly follows behind the player runner,
/// maintaining clear visibility of upcoming lanes, obstacles, and collectibles.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Offsets")]
    [Tooltip("Offset relative to player position")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2.8f, -4.8f);
    [Tooltip("Look ahead target offset")]
    [SerializeField] private Vector3 lookAheadOffset = new Vector3(0f, 1.4f, 9.0f);

    [Header("Smoothness")]
    [Tooltip("Smoothing factor for horizontal follow (dampens sudden lane shifts)")]
    [SerializeField] private float horizontalSmoothTime = 0.12f;
    [Tooltip("Camera pitch angle looking down at path")]
    [SerializeField] private float cameraPitchAngle = 14f;

    private float currentVelocityX;

    private void Start()
    {
        if (target == null)
        {
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            if (player != null)
            {
                target = player.transform;
            }
        }

        // Set initial orientation
        transform.rotation = Quaternion.Euler(cameraPitchAngle, 0f, 0f);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Smooth X follow so camera doesn't jerk violently on lane changes
        float smoothX = Mathf.SmoothDamp(transform.position.x, target.position.x + offset.x, ref currentVelocityX, horizontalSmoothTime);

        // Follow target along Z and Y directly
        float targetY = target.position.y + offset.y;
        float targetZ = target.position.z + offset.z;

        transform.position = new Vector3(smoothX, targetY, targetZ);

        // Look toward the path ahead of the player
        Vector3 lookTarget = target.position + lookAheadOffset;
        Vector3 direction = (lookTarget - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
