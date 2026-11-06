using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game controller managing gameplay state transitions,
/// game over sequences, and scene transitions.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Playing,
        GameOver,
        Paused
    }

    [Header("Current State")]
    [SerializeField] private GameState currentState = GameState.Playing;

    [Header("Player Reference")]
    [SerializeField] private PlayerMovement player;

    public GameState State => currentState;
    public bool IsPlaying => currentState == GameState.Playing;

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
        currentState = GameState.Playing;
        Time.timeScale = 1.0f;

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerMovement>();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDesertMusic();
        }
    }

    /// <summary>
    /// Invoked when the player strikes an obstacle.
    /// Halts player, records final stats, and presents the Game Over screen.
    /// </summary>
    public void TriggerGameOver()
    {
        if (currentState == GameState.GameOver) return;
        currentState = GameState.GameOver;

        if (player != null)
        {
            player.Die();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOverSound();
        }

        // Show Game Over panel with final run statistics
        if (ScoreManager.Instance != null && UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(
                ScoreManager.Instance.Score,
                ScoreManager.Instance.Distance,
                ScoreManager.Instance.Coins,
                ScoreManager.Instance.Gems
            );
        }
    }

    /// <summary>
    /// Restarts the gameplay run cleanly.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1.0f;
        // Reload active scene to ensure completely clean slate and zero memory leaks
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    /// <summary>
    /// Navigates back to the Main Menu scene.
    /// </summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("MainMenu");
    }
}
