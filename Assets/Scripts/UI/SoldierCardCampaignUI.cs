using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoldierCardCampaignUI : MonoBehaviour {
    [Header("Soldier Card Campaign UI Configuration")]
    [SerializeField] private Image portrait;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI roleText;
    [SerializeField] private TextMeshProUGUI healthText;
    
    [Header("Soldier Card Campaign UI Button & Sound Effect")]
    [SerializeField] private Button healButton;
    [SerializeField] private AudioClip buttonClickSoundEffect;

    public void Setup(SoldierData soldierData) {
        portrait.sprite = soldierData.portrait;
        nameText.text = $"{soldierData.firstName} {soldierData.lastName}";
        roleText.text = soldierData.roleData.roleName;
        healthText.text = $"Health: {soldierData.currentHealth}";
        healButton.onClick.AddListener(() => HealSoldier(soldierData));
    }

    private void HealSoldier(SoldierData soldierData) {
        if (GameManager.Instance.Supplies < 2 || soldierData.currentHealth == soldierData.roleData.maxHealth) return;
        AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        soldierData.currentHealth += 25;
        if (soldierData.currentHealth > soldierData.roleData.maxHealth)  soldierData.currentHealth = soldierData.roleData.maxHealth;
        GameManager.Instance.ModifySupplies(-2);
        CampaignMapManager.Instance.RefreshUI();
    }
}