using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {
    [Header("Main Menu Manager Configuration")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Main Menu Manager Buttons")]
    [SerializeField] private Button startCampaignButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Button creditsBackButton;
    [SerializeField] private AudioClip buttonClickSoundEffect;

    private void Awake() {
        startCampaignButton.interactable = true;
        startCampaignButton.onClick.AddListener(StartNewGame);
        settingsButton.onClick.AddListener(OpenSettings);
        creditsButton.onClick.AddListener(OpenCredits);
        quitButton.onClick.AddListener(QuitGame);

        settingsBackButton.onClick.AddListener(CloseSettings);
        creditsBackButton.onClick.AddListener(CloseCredits);

        ShowMainMenu();
    }

    private void ShowMainMenu() {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    private void StartNewGame() {
        startCampaignButton.interactable = false;
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        AudioManager.Instance.PlaySceneChangeSoundEffect();
        SceneTransitionManager.Instance.LoadScene("SquadSelectionScene");
    }

    private void OpenSettings() {
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    private void CloseSettings() {
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    private void OpenCredits() {
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    private void CloseCredits() {
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    private void QuitGame() {
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        Application.Quit();
    }
}