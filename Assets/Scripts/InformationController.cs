using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the How To Play / Information screen and allows returning to Main Menu.
/// </summary>
public class InformationController : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        UIManager.EnsureEventSystem();
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    public void OnBackClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
