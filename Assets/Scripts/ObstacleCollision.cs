using UnityEngine;

/// <summary>
/// Attached to obstacles (Tree, Rock, Crate) or player.
/// Handles collision detection when player collides with an obstacle,
/// halting movement, spawning crash effects, and triggering Game Over.
/// </summary>
public class ObstacleCollision : MonoBehaviour
{
    [Header("Crash Effects")]
    [Tooltip("Particle effect spawned upon crash")]
    [SerializeField] private GameObject crashVfxPrefab;

    private bool hasCollided = false;

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.gameObject);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        HandleHit(hit.gameObject);
    }

    private void HandleHit(GameObject hitObj)
    {
        if (hasCollided) return;

        // Check if collision is between Player and Obstacle
        bool isThisObstacle = CompareTag("Obstacle");
        bool hitIsPlayer = hitObj.CompareTag("Player") || hitObj.GetComponent<PlayerMovement>() != null;

        bool isThisPlayer = CompareTag("Player") || GetComponent<PlayerMovement>() != null;
        bool hitIsObstacle = hitObj.CompareTag("Obstacle");

        if ((isThisObstacle && hitIsPlayer) || (isThisPlayer && hitIsObstacle))
        {
            hasCollided = true;

            PlayerMovement player = isThisPlayer ? GetComponent<PlayerMovement>() : hitObj.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.Die();
            }

            // Play collision sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayObstacleCrashSound();
            }

            // Spawn crash visual effects
            Vector3 contactPoint = (transform.position + hitObj.transform.position) * 0.5f;
            if (crashVfxPrefab != null)
            {
                Instantiate(crashVfxPrefab, contactPoint + Vector3.up * 0.5f, Quaternion.identity);
            }

            // Trigger Game Over in GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
    }
}
