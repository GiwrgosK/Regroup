using UnityEngine;
using UnityEngine.UI;

public class EscapeMenuManager : MonoBehaviour {
    [Header("Escape Menu Manager Configuration")]
    [SerializeField] private GameObject escapeMenuPanel;
    [SerializeField] private GameObject mainButtonsContainer;
    [SerializeField] private GameObject settingsUIContainer;

    [Header("Escape Menu Manager Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;
    [SerializeField] private AudioClip buttonClickSoundEffect;

    private void Awake() {
        resumeButton.onClick.AddListener(ToggleEscapeMenu);
        quitButton.onClick.AddListener(QuitGame);
        settingsButton.onClick.AddListener(OpenSettingsPanel);
        if(backButton != null) backButton.onClick.AddListener(CloseSettingsPanel);
    }

    private void Update() {
        if (InputManager.Instance.IsEscapePressed()) {
            if (escapeMenuPanel.activeSelf && settingsUIContainer.activeSelf) {
                CloseSettingsPanel();
            } else {
                ToggleEscapeMenu();
            }
        }
    }

    private void ToggleEscapeMenu() {
        if (!escapeMenuPanel.activeSelf) {
            ShowMainButtons(); 
        }

        TimeManager.Instance.TogglePause();
        escapeMenuPanel.SetActive(TimeManager.Instance.IsPaused);
    }

    private void ShowMainButtons() {
        mainButtonsContainer.SetActive(true);
        settingsUIContainer.SetActive(false);
    }

    private void OpenSettingsPanel() {
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        mainButtonsContainer.SetActive(false);
        settingsUIContainer.SetActive(true); 
    }

    private void CloseSettingsPanel() {
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        mainButtonsContainer.SetActive(true);
        settingsUIContainer.SetActive(false);
    }

    private void QuitGame() {
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        Application.Quit();
    }
}