using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventUI : MonoBehaviour {
    public static EventUI Instance { get; set; }

    [Header("Event UI Configuration")]
    [SerializeField] private GameObject eventUIPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private AudioClip buttonClickSoundEffect;
    [SerializeField] private GameObject optionButtonPrefab;
    [SerializeField] private GameObject continueButtonPrefab;

    private System.Action onContinueCallback;

    private void Awake() {
        if (eventUIPanel != null) eventUIPanel.SetActive(false);
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        eventUIPanel.SetActive(false);
    }

    private void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }

    public void Show(EventData eventData) {
        eventUIPanel.SetActive(true);
        Map.Instance.CloseMap();
        titleText.text = eventData.Title;
        descriptionText.text = eventData.Description;
        ClearContainer();

        foreach (EventOption option in eventData.Options) {
            GameObject buttonGameObject = Instantiate(optionButtonPrefab, optionsContainer);
            TextMeshProUGUI buttonText = buttonGameObject.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = option.Text;
            Button button = buttonGameObject.GetComponent<Button>();
            button.onClick.AddListener(() => OnOptionSelected(option));
        }
    }

    public void Hide() {
        eventUIPanel.SetActive(false);
    }

    private void ClearContainer() {
        foreach (Transform child in optionsContainer) {
            Destroy(child.gameObject);
        }
    }

    public void ShowResult(string resultText, System.Action onContinue) {
        eventUIPanel.SetActive(true);
        descriptionText.text = resultText;
        ClearContainer();
        optionsContainer.gameObject.SetActive(true);
        GameObject buttonGameObject = Instantiate(continueButtonPrefab, optionsContainer);
        buttonGameObject.SetActive(true);
        Button continueButton = buttonGameObject.GetComponent<Button>();
        continueButton.onClick.AddListener(() => OnContinueClicked());
        onContinueCallback = onContinue;
    }

    public void OnContinueClicked() {
        eventUIPanel.SetActive(false);
        onContinueCallback?.Invoke();
    }

    public void OnOptionSelected(EventOption selectedOption) {
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        EventManager.Instance.ResolveEvent(selectedOption);
    }
}