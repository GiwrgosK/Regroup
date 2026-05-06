using UnityEngine;
using System.Collections.Generic;

public class SquadGenerator : MonoBehaviour {
    public static SquadGenerator Instance { get; private set; }

    [Header("Squad Generator Configuration")]
    [SerializeField] private List<Sprite> portraitPool;
    [SerializeField] private TextAsset soldierDataJSON;
    [SerializeField] private List<SoldierRoleData> availableRoles;

    private SoldierInformationData soldierInformationData;
    private readonly int draftPoolSize = 7;

    public List<SoldierRoleData> AvailableRoles => availableRoles;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        LoadDatabase();
    }

    private void Start() {
        if (GameManager.Instance.squad == null || GameManager.Instance.squad.Count == 0) {
            GenerateStartingSquad();
        }
    }

    private void LoadDatabase() {
        if (soldierDataJSON != null) {
            soldierInformationData = JsonUtility.FromJson<SoldierInformationData>(soldierDataJSON.text);
        } else {
            Debug.LogError("Soldier Data JSON file is missing in SquadGenerator!");
            soldierInformationData = new SoldierInformationData {
                firstNames = new string[] { "John" },
                lastNames = new string[] { "Doe" },
                bios = new string[] { "Data missing." }
            };
        }
    }

    private void GenerateStartingSquad() {
        List<SoldierData> squadCandidates = new List<SoldierData>();
        for (int i = 0; i < draftPoolSize; i++) {
            SoldierRoleData randomRole = availableRoles[Random.Range(0, availableRoles.Count)];
            squadCandidates.Add(GenerateSoldier(randomRole));
        }
        GameManager.Instance.SetSquad(squadCandidates);
    }

    public SoldierData GenerateSoldier(SoldierRoleData roleData) {
        SoldierData soldier = new SoldierData {
            firstName = soldierInformationData.firstNames[Random.Range(0, soldierInformationData.firstNames.Length)],
            lastName = soldierInformationData.lastNames[Random.Range(0, soldierInformationData.lastNames.Length)],
            bio = soldierInformationData.bios[Random.Range(0, soldierInformationData.bios.Length)],
            roleData = roleData,
            serialNumber = "Serial Number: SN-" + Random.Range(10000, 99999),
            portrait = portraitPool[Random.Range(0, portraitPool.Count)],
            currentHealth = roleData.maxHealth
        };
        return soldier;
    }
}