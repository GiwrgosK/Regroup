using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SquadManagerUI : MonoBehaviour {
    [Header("Squad Manager UI Configuration")]
    [SerializeField] private Transform cardParent;
    [SerializeField] private GameObject soldierCardPrefab;
    [SerializeField] private TextMeshProUGUI counterText;

    [Header("Squad Manager UI Soldier Information")]
    [SerializeField] private Image portrait;
    [SerializeField] private TMP_InputField firstNameInput;
    [SerializeField] private TMP_InputField lastNameInput;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI weaponInformation;
    [SerializeField] private TextMeshProUGUI movementRange;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI grenadeAmountText;
    [SerializeField] private TextMeshProUGUI bioText;

    [Header("Squad Manager UI Buttons & Sounds Effects")]
    [SerializeField] private Button deployButton;
    [SerializeField] private Button randomizeButton;
    [SerializeField] private Button backToMainMenuButton;
    [SerializeField] private AudioClip buttonClickSoundEffect;
    [SerializeField] private AudioClip squadFullSoundEffect;

    private List<SoldierCardUI> allSoldierCards = new List<SoldierCardUI>();
    private List<SoldierData> draftedSquad = new List<SoldierData>();
    private SoldierData selectedSoldierForViewing;

    private const int MAX_SQUAD_SIZE = 4;

    private void Start() {
        deployButton.interactable = false;
        randomizeButton.interactable = true;
        backToMainMenuButton.interactable = true;
        firstNameInput.characterLimit = 10;
        lastNameInput.characterLimit = 10;
        PopulateRoster();
        deployButton.onClick.AddListener(Deploy);
        randomizeButton.onClick.AddListener(RandomizeSelectedUnit);
        backToMainMenuButton.onClick.AddListener(BackToMainMenu);
        firstNameInput.onSelect.AddListener(delegate { PlaySoundEffect(); });
        lastNameInput.onSelect.AddListener(delegate { PlaySoundEffect(); });
        firstNameInput.onEndEdit.AddListener(delegate { OnNameChanged(); });
        lastNameInput.onEndEdit.AddListener(delegate { OnNameChanged(); });
        UpdateCounterText();
    }

    private void PopulateRoster() {
        foreach(Transform child in cardParent) Destroy(child.gameObject);
        allSoldierCards.Clear();
        foreach (SoldierData soldierData in GameManager.Instance.squad) {
            GameObject cardGO = Instantiate(soldierCardPrefab, cardParent);
            SoldierCardUI card = cardGO.GetComponent<SoldierCardUI>();
            card.Initialize(soldierData, this);
            allSoldierCards.Add(card);
        }
        if(GameManager.Instance.squad.Count > 0) ViewSoldierDetails(GameManager.Instance.squad[0]);
    }

    private void RandomizeSelectedUnit() {
        if (selectedSoldierForViewing == null) return;
        PlaySoundEffect();

        List<SoldierRoleData> availableRoles = SquadGenerator.Instance.AvailableRoles;
        SoldierRoleData randomRole = availableRoles[Random.Range(0, availableRoles.Count)];

        SoldierData randomizedData = SquadGenerator.Instance.GenerateSoldier(randomRole);

        selectedSoldierForViewing.firstName = randomizedData.firstName;
        selectedSoldierForViewing.lastName = randomizedData.lastName;
        selectedSoldierForViewing.bio = randomizedData.bio;
        selectedSoldierForViewing.roleData = randomizedData.roleData;
        selectedSoldierForViewing.serialNumber = randomizedData.serialNumber;
        selectedSoldierForViewing.portrait = randomizedData.portrait;
        selectedSoldierForViewing.currentHealth = randomizedData.currentHealth;

        ViewSoldierDetails(selectedSoldierForViewing);

        foreach (SoldierCardUI card in allSoldierCards) {
            card.Refresh();
        }
    }

    public void ViewSoldierDetails(SoldierData soldierData) {
        selectedSoldierForViewing = soldierData;
        portrait.sprite = soldierData.portrait;
        firstNameInput.text = soldierData.firstName;
        lastNameInput.text = soldierData.lastName;
        rankText.text = $"Role & Weapon: {soldierData.roleData.roleName} - {soldierData.roleData.weaponName}";
        weaponInformation.text = $"Damage: {soldierData.roleData.weaponDamage} - Range: {soldierData.roleData.weaponRange} Tiles";
        movementRange.text = $"Movement Range: {soldierData.roleData.movementRange} Tiles";
        healthText.text = $"Health: {soldierData.roleData.maxHealth}";
        grenadeAmountText.text = $"Grenades: {soldierData.roleData.grenadeAmount}";
        bioText.text = soldierData.bio;
    }

    public bool ToggleDraftSelection(SoldierData soldier) {
        PlaySoundEffect();

        if (draftedSquad.Contains(soldier)) {
            draftedSquad.Remove(soldier);
            UpdateDeployButtonState();
            return false;
        } else {
            if (draftedSquad.Count >= MAX_SQUAD_SIZE) {
                AudioManager.Instance.PlayClip(squadFullSoundEffect);
                return false;
            }
            
            draftedSquad.Add(soldier);
            UpdateDeployButtonState();
            return true;
        }
    }

    private void OnNameChanged() {
        if (selectedSoldierForViewing == null) return;

        string firstName = firstNameInput.text.Trim();
        string lastName = lastNameInput.text.Trim();
        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName)) return;

        selectedSoldierForViewing.firstName = firstName;
        selectedSoldierForViewing.lastName = lastName;

        foreach (SoldierCardUI card in allSoldierCards) {
            card.Refresh();
        }
    }

    private void PlaySoundEffect() {
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
    }

    private void UpdateDeployButtonState() {
        deployButton.interactable = draftedSquad.Count == MAX_SQUAD_SIZE;
        UpdateCounterText();
    }

    private void UpdateCounterText() {
        if(counterText != null)     counterText.text = $"{draftedSquad.Count} / {MAX_SQUAD_SIZE} Selected";
    }

    private void Deploy() {
        GameManager.Instance.SetSquad(draftedSquad);
        deployButton.interactable = false;
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        AudioManager.Instance.PlaySceneChangeSoundEffect();
        SceneTransitionManager.Instance.LoadScene("CampaignMapScene");
    }

    private void BackToMainMenu() {
        backToMainMenuButton.interactable = false;
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        AudioManager.Instance.PlaySceneChangeSoundEffect();
        SceneTransitionManager.Instance.LoadScene("MainMenuScene");
    }
}