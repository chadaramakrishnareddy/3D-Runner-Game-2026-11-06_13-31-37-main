using UnityEngine;
using System;

/// <summary>
/// Tracks player score, distance, coin count, and gem count.
/// Implements Singleton pattern for simple global access.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Scoring Multipliers")]
    [SerializeField] private int pointsPerMeter = 1;
    [SerializeField] private int pointsPerCoin = 10;
    [SerializeField] private int pointsPerGem = 50;

    [Header("Live Data (Read-Only)")]
    [SerializeField] private int currentCoins = 0;
    [SerializeField] private int currentGems = 0;
    [SerializeField] private int currentDistanceMeters = 0;
    [SerializeField] private int currentTotalScore = 0;

    // Events for UI listeners
    public event Action<int> OnCoinsChanged;
    public event Action<int> OnGemsChanged;
    public event Action<int> OnDistanceChanged;
    public event Action<int> OnScoreChanged;

    public int Coins => currentCoins;
    public int Gems => currentGems;
    public int Distance => currentDistanceMeters;
    public int Score => currentTotalScore;

    private PlayerMovement player;

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
        player = FindFirstObjectByType<PlayerMovement>();
        ResetScores();
    }

    private void Update()
    {
        if (player != null && !player.IsDead)
        {
            int newDist = Mathf.Max(0, Mathf.FloorToInt(player.CurrentDistance));
            if (newDist != currentDistanceMeters)
            {
                currentDistanceMeters = newDist;
                RecalculateTotalScore();
                OnDistanceChanged?.Invoke(currentDistanceMeters);
            }
        }
    }

    /// <summary>
    /// Adds collected coins and triggers events.
    /// </summary>
    public void AddCoin(int amount = 1)
    {
        currentCoins += amount;
        RecalculateTotalScore();
        OnCoinsChanged?.Invoke(currentCoins);
    }

    /// <summary>
    /// Adds collected gems and triggers events.
    /// </summary>
    public void AddGem(int amount = 1)
    {
        currentGems += amount;
        RecalculateTotalScore();
        OnGemsChanged?.Invoke(currentGems);
    }

    /// <summary>
    /// Recalculates total score = distance * 1 + coins * 10 + gems * 50
    /// </summary>
    private void RecalculateTotalScore()
    {
        currentTotalScore = (currentDistanceMeters * pointsPerMeter) +
                            (currentCoins * pointsPerCoin) +
                            (currentGems * pointsPerGem);
        OnScoreChanged?.Invoke(currentTotalScore);
    }

    /// <summary>
    /// Resets all values for a new run.
    /// </summary>
    public void ResetScores()
    {
        currentCoins = 0;
        currentGems = 0;
        currentDistanceMeters = 0;
        currentTotalScore = 0;

        OnCoinsChanged?.Invoke(0);
        OnGemsChanged?.Invoke(0);
        OnDistanceChanged?.Invoke(0);
        OnScoreChanged?.Invoke(0);
    }
}
