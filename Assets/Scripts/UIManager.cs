using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Controls the in-game HUD (Score, Coins, Gems, Distance)
/// and the Game Over overlay window with final stats and restart buttons.
/// Supports both button clicks and screen taps/keyboard to restart.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("In-Game HUD Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text distanceText;
    [SerializeField] private Text coinsText;
    [SerializeField] private Text gemsText;

    [Header("Game Over Window")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text finalDistanceText;
    [SerializeField] private Text finalCoinsText;
    [SerializeField] private Text finalGemsText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    private float gameOverShownTime = -1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        EnsureEventSystem();
    }

    private void Start()
    {
        EnsureEventSystem();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
            Text btnTxt = restartButton.GetComponentInChildren<Text>();
            if (btnTxt != null) btnTxt.raycastTarget = false;
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            Text btnTxt = mainMenuButton.GetComponentInChildren<Text>();
            if (btnTxt != null) btnTxt.raycastTarget = false;
        }

        SubscribeEvents();
        UpdateAllHUD(0, 0, 0, 0);
    }

    private void Update()
    {
        if (gameOverPanel != null && gameOverPanel.activeInHierarchy)
        {
            // Debounce for 0.25s after game over screen appears to avoid accidental taps
            if (gameOverShownTime > 0f && (Time.unscaledTime - gameOverShownTime > 0.25f))
            {
                bool restartTriggered = false;

#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                        Keyboard.current.enterKey.wasPressedThisFrame ||
                        Keyboard.current.numpadEnterKey.wasPressedThisFrame ||
                        Keyboard.current.rKey.wasPressedThisFrame)
                    {
                        restartTriggered = true;
                    }
                }
#endif
                try
                {
                    if (Input.GetKeyDown(KeyCode.Space) ||
                        Input.GetKeyDown(KeyCode.Return) ||
                        Input.GetKeyDown(KeyCode.KeypadEnter) ||
                        Input.GetKeyDown(KeyCode.R))
                    {
                        restartTriggered = true;
                    }
                }
                catch { }

                // Screen tap, touch, or mouse click anywhere on the screen
                if (SwipeManager.tap)
                {
                    restartTriggered = true;
                }

#if ENABLE_INPUT_SYSTEM
                if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                {
                    restartTriggered = true;
                }
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    restartTriggered = true;
                }
#endif
                try
                {
                    if (Input.GetMouseButtonDown(0) || (Input.touches.Length > 0 && Input.touches[0].phase == UnityEngine.TouchPhase.Began))
                    {
                        restartTriggered = true;
                    }
                }
                catch { }

                if (restartTriggered)
                {
                    // If user is clicking/tapping on the Main Menu button, let that button handle it
                    if (!IsPointerOverButton(mainMenuButton))
                    {
                        OnRestartClicked();
                    }
                }
            }
        }
    }

    private bool IsPointerOverButton(Button btn)
    {
        if (btn == null || !btn.gameObject.activeInHierarchy) return false;
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == btn.gameObject) return true;

        Vector2 pointerPos = Vector2.zero;
        bool hasPointer = false;

#if ENABLE_INPUT_SYSTEM
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            pointerPos = Touchscreen.current.primaryTouch.position.ReadValue();
            hasPointer = true;
        }
        else if (Mouse.current != null)
        {
            pointerPos = Mouse.current.position.ReadValue();
            hasPointer = true;
        }
#endif
        if (!hasPointer)
        {
            try
            {
                if (Input.touches.Length > 0)
                {
                    pointerPos = Input.touches[0].position;
                    hasPointer = true;
                }
                else
                {
                    pointerPos = Input.mousePosition;
                    hasPointer = true;
                }
            }
            catch { }
        }

        if (hasPointer)
        {
            RectTransform rt = btn.GetComponent<RectTransform>();
            if (rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, pointerPos))
            {
                return true;
            }
        }

        return false;
    }

    public static void EnsureEventSystem()
    {
        if (EventSystem.current == null && Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            esObj.AddComponent<InputSystemUIInputModule>();
#else
            esObj.AddComponent<StandaloneInputModule>();
#endif
        }
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScore;
            ScoreManager.Instance.OnDistanceChanged += UpdateDistance;
            ScoreManager.Instance.OnCoinsChanged += UpdateCoins;
            ScoreManager.Instance.OnGemsChanged += UpdateGems;
        }
    }

    private void UnsubscribeEvents()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScore;
            ScoreManager.Instance.OnDistanceChanged -= UpdateDistance;
            ScoreManager.Instance.OnCoinsChanged -= UpdateCoins;
            ScoreManager.Instance.OnGemsChanged -= UpdateGems;
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = $"SCORE: {score:N0}";
    }

    public void UpdateDistance(int distance)
    {
        if (distanceText != null) distanceText.text = $"DISTANCE: {distance} m";
    }

    public void UpdateCoins(int coins)
    {
        if (coinsText != null) coinsText.text = $"COINS: {coins}";
    }

    public void UpdateGems(int gems)
    {
        if (gemsText != null) gemsText.text = $"GEMS: {gems}";
    }

    public void UpdateAllHUD(int score, int distance, int coins, int gems)
    {
        UpdateScore(score);
        UpdateDistance(distance);
        UpdateCoins(coins);
        UpdateGems(gems);
    }

    /// <summary>
    /// Displays the Game Over modal overlay populated with the final run statistics.
    /// </summary>
    public void ShowGameOver(int score, int distance, int coins, int gems)
    {
        gameOverShownTime = Time.unscaledTime;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null) finalScoreText.text = $"FINAL SCORE\n{score:N0}";
        if (finalDistanceText != null) finalDistanceText.text = $"DISTANCE: {distance} m";
        if (finalCoinsText != null) finalCoinsText.text = $"COINS: {coins}";
        if (finalGemsText != null) finalGemsText.text = $"GEMS: {gems}";
    }

    /// <summary>
    /// Hides the Game Over modal.
    /// </summary>
    public void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void OnRestartClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    private void OnMainMenuClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }
}
