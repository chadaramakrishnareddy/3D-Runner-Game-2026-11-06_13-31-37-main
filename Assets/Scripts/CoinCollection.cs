using UnityEngine;

/// <summary>
/// Attached to Coin prefabs. Detects when player runs into the coin,
/// adds points, triggers particle FX and audio, and destroys the coin.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CoinCollection : MonoBehaviour
{
    [Header("Coin Properties")]
    [Tooltip("Score bonus awarded for collecting this coin")]
    [SerializeField] private int scoreValue = 10;
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
        // Add score via ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddCoin(1);
        }

        // Play audio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCoinSound();
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
