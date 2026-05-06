using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoldierCardUI : MonoBehaviour {
    [Header("Soldier Card UI Configuration")]
    [SerializeField] private Image portrait;
    [SerializeField] private GameObject checkboxImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI roleText;

    [Header("Soldier Card UI Buttons & Buttons Sound Effect")]
    [SerializeField] private Button soldierCardButton;
    [SerializeField] private Button selectedCheckboxButton;
    [SerializeField] private AudioClip buttonClickSoundEffect;

    private SoldierData soldierData;
    private SquadManagerUI squadManagerUI;
    private bool isSelected = false;

    public void Initialize(SoldierData soldierData, SquadManagerUI squadManagerUI) {
        this.soldierData = soldierData;
        this.squadManagerUI = squadManagerUI;
        portrait.sprite = soldierData.portrait;
        nameText.text = $"{soldierData.firstName} {soldierData.lastName}";
        roleText.text = soldierData.roleData.roleName;
        
        
        selectedCheckboxButton.onClick.RemoveAllListeners();
        selectedCheckboxButton.onClick.AddListener(SelectSoldier);
        soldierCardButton.onClick.RemoveAllListeners();
        soldierCardButton.onClick.AddListener(ShowSoldierStatistics);
    }

    private void SelectSoldier() {
        isSelected = squadManagerUI.ToggleDraftSelection(soldierData);
        checkboxImage.SetActive(isSelected);
    }

    private void ShowSoldierStatistics() {
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        squadManagerUI.ViewSoldierDetails(soldierData);
    }

    public void Refresh() {
        nameText.text = $"{soldierData.firstName} {soldierData.lastName}";
        roleText.text = $"{soldierData.roleData.roleName}";
        portrait.sprite = soldierData.portrait;
    }
}