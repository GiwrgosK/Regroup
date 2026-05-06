using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingSceneManager : MonoBehaviour {
    [Header("Ending Scene Configuration")]
    [SerializeField] private GameObject endingTextPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI endingTextWin;
    [SerializeField] private TextMeshProUGUI endingTextLoss;
    [SerializeField] private GameObject statisticsScrollView;

    [Header("Ending Scene Buttons")]
    [SerializeField] private Button quitGameButton;
    [SerializeField] private Button showStatisticsButton;
    [SerializeField] private Button showEndingTextButton;
    [SerializeField] private AudioClip buttonClickSoundEffect;

    [Header("Ending Scene Total Statistics")]
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI totalNodesVisitedText;
    [SerializeField] private TextMeshProUGUI totalEnemiesKilledText;
    [SerializeField] private TextMeshProUGUI totalAlliesLostText;
    [SerializeField] private TextMeshProUGUI totalSuppliesCollectedText;

    private void Start() {
        totalScoreText.text = "Total Score: " + GameManager.Instance.TotalScore;
        totalNodesVisitedText.text = "Nodes Visited: " + GameManager.Instance.NodesVisited;
        totalEnemiesKilledText.text = "Total Enemies Killed: " + GameManager.Instance.TotalEnemiesKilled;
        totalAlliesLostText.text = "Total Allies Lost: " + GameManager.Instance.TotalAlliesLost;
        totalSuppliesCollectedText.text = "Supplies Collected: " + GameManager.Instance.Supplies;
        quitGameButton.onClick.AddListener(QuitGame);
        showStatisticsButton.onClick.AddListener(ShowStatistics);
        showEndingTextButton.onClick.AddListener(ShowEndingText);
        if (GameManager.Instance.PlayerWin) {
            titleText.text = "Victory";
            endingTextWin.gameObject.SetActive(true);
        } else {
            titleText.text = "Defeat";
            endingTextLoss.gameObject.SetActive(true);
        }
    }

    private void ShowStatistics() {
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        statisticsScrollView.SetActive(true);
        endingTextPanel.SetActive(false);
        showStatisticsButton.gameObject.SetActive(false);
        showEndingTextButton.gameObject.SetActive(true);
    }

    private void ShowEndingText() {
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        endingTextPanel.SetActive(true);
        statisticsScrollView.SetActive(false);
        showEndingTextButton.gameObject.SetActive(false);
        showStatisticsButton.gameObject.SetActive(true);
    }

    private void QuitGame() {
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        Application.Quit();
    }
}