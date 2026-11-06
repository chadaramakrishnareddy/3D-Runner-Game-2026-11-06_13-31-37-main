using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls Main Menu interactions and navigation.
/// Handles Play (opens StageSelect), How To Play (Information scene), and Quit.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button quitButton;

    [Header("Scene Names")]
    [SerializeField] private string stageSelectSceneName = "StageSelect";
    [SerializeField] private string infoSceneName = "Information";

    private void Start()
    {
        UIManager.EnsureEventSystem();
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (howToPlayButton != null)
        {
            howToPlayButton.onClick.AddListener(OnHowToPlayClicked);
        }

        if (quitButton != null)
        {
#if UNITY_WEBGL
            // WebGL builds cannot reliably close browser windows, so hide the button
            quitButton.gameObject.SetActive(false);
#else
            quitButton.onClick.AddListener(OnQuitClicked);
#endif
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDesertMusic();
        }
    }

    public void OnPlayClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();
        SceneManager.LoadScene(stageSelectSceneName);
    }

    public void OnHowToPlayClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();
        SceneManager.LoadScene(infoSceneName);
    }

    public void OnQuitClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();
        Debug.Log("Exiting Desert Runner...");
        Application.Quit();
    }
}
