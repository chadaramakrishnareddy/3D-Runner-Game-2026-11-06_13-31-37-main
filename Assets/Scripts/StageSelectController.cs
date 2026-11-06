using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the Stage Selection screen.
/// Allows starting the Desert Run stage, and displays future locked stages (Canyon, Night).
/// </summary>
public class StageSelectController : MonoBehaviour
{
    [Header("Stage Buttons")]
    [SerializeField] private Button desertRunButton;
    [SerializeField] private Button canyonRunButton;
    [SerializeField] private Button nightRunButton;
    [SerializeField] private Button backButton;

    [Header("Target Gameplay Scene")]
    [SerializeField] private string gameplaySceneName = "GamePlay";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        UIManager.EnsureEventSystem();
        if (desertRunButton != null)
        {
            desertRunButton.onClick.AddListener(OnDesertRunSelected);
        }

        if (canyonRunButton != null)
        {
            canyonRunButton.onClick.AddListener(OnLockedStageClicked);
        }

        if (nightRunButton != null)
        {
            nightRunButton.onClick.AddListener(OnLockedStageClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    public void OnDesertRunSelected()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OnLockedStageClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();
        Debug.Log("This stage is currently LOCKED. Complete high distance challenges in Desert Run to unlock!");
    }

    public void OnBackClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
