using UnityEngine;

/// <summary>
/// Attached to Gem prefabs. Detects when player runs into the gem,
/// adds points (+50), triggers particle FX and audio, and destroys the gem.
/// </summary>
[RequireComponent(typeof(Collider))]
public class GemCollection : MonoBehaviour
{
    [Header("Gem Properties")]
    [Tooltip("Score bonus awarded for collecting this gem")]
    [SerializeField] private int scoreValue = 50;
    [Tooltip("Particle prefab or visual flash to spawn upon pickup")]
    [SerializeField] private GameObject collectParticlePrefab;

    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        // Check if player collided
        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null)
        {
            collected = true;
            Collect();
        }
    }

    private void Collect()
    {
        // Add gem and score via ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddGem(1);
        }

        // Play audio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGemSound();
        }

        // Spawn pickup particle effect if assigned
        if (collectParticlePrefab != null)
        {
            Instantiate(collectParticlePrefab, transform.position, Quaternion.identity);
        }

        // Destroy or disable
        Destroy(gameObject);
    }
}
