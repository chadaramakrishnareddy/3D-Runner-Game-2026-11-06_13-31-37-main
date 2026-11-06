using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Dedicated restart controller that provides both full scene reloads
/// and in-place soft resets for testing or rapid restarting.
/// </summary>
public class RestartManager : MonoBehaviour
{
    public static RestartManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Performs a clean full scene reload (standard Unity method).
    /// </summary>
    public void ReloadScene()
    {
        Time.timeScale = 1.0f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    /// <summary>
    /// Performs an in-memory soft reset without reloading the scene.
    /// Resets Player position, Score, Spawner segments, and HUD.
    /// </summary>
    public void SoftReset()
    {
        Time.timeScale = 1.0f;

        // Reset Score & Collectibles
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScores();
        }

        // Reset Player Position & State
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            player.ResetPlayer();
        }

        // Reset Endless Spawner
        if (EndlessSpawnSystem.Instance != null)
        {
            EndlessSpawnSystem.Instance.InitializeSpawning();
        }

        // Hide Game Over UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideGameOver();
        }
    }
}
