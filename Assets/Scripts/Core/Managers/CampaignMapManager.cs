using TMPro;
using UnityEngine;

public class CampaignMapManager : MonoBehaviour {
    public static CampaignMapManager Instance { get; private set; }

    [Header("Campaign Map Manager Configuration")]
    [SerializeField] private TextMeshProUGUI suppliesText;
    [SerializeField] private Transform bottomPart;
    [SerializeField] private GameObject soldierCardPrefab;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() {
        RefreshUI();
    }

    public void RefreshUI() {
        suppliesText.text = $"Remaining Supplies: {GameManager.Instance.Supplies}";

        foreach (Transform soldierCard in bottomPart) {
            Destroy(soldierCard.gameObject);
        }

         foreach (var soldier in GameManager.Instance.squad) {
            GameObject cardGO = Instantiate(soldierCardPrefab, bottomPart);
            SoldierCardCampaignUI soldierCardCampaignUI = cardGO.GetComponent<SoldierCardCampaignUI>();
            if (soldierCardCampaignUI != null) {
               soldierCardCampaignUI.Setup(soldier);
            } 
        }
    }
}