using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public List<SoldierData> squad = new List<SoldierData>();
    public List<MapNodeData> campaignNodes = new List<MapNodeData>();

    private int supplies = 5;
    public int Supplies => supplies;

    private int nodesVisited = -1;
    public int NodesVisited => nodesVisited;

    private bool hasInitializedCampaign = false;
    public bool HasInitializedCampaign => hasInitializedCampaign;

    public bool PlayerWin { get; set; }
    public int TotalEnemiesKilled { get; set; } = 0;
    public int TotalAlliesLost { get; set; } = 0;
    public int TotalSuppliesCollected { get; set; } = 0;
    public int TotalScore { get; set; } = 0;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetSquad(List<SoldierData> newSquad) {
        squad = newSquad;
    }

    public void ModifySupplies(int amount) {
        supplies += amount;
        supplies = Mathf.Max(0, supplies);
        if (amount > 0) TotalSuppliesCollected += amount;
    }

    public void IncrementNodesVisited() {
        nodesVisited++;
    }

    public void SetCampaignFlagTrue() {
        hasInitializedCampaign = true;
    }

    public void AddSoldier() {
        if (squad.Count < 4) {
            SoldierRoleData randomRole = SquadGenerator.Instance.AvailableRoles[Random.Range(0, SquadGenerator.Instance.AvailableRoles.Count)];
            squad.Add(SquadGenerator.Instance.GenerateSoldier(randomRole));
        }
    }

    public void RemoveSoldier() {
        TotalAlliesLost++;
        if (squad.Count > 0) {
            int soldierIndex = Random.Range(0, squad.Count);
            squad.RemoveAt(soldierIndex);
            if (squad.Count == 0) {
                PlayerWin = false;
                HandleGameOver();
            }
        } else {
            PlayerWin = false;
            HandleGameOver();
        }
    }

    public void DamageSquad() {
        int damage = Random.Range(5, 51);
        int targetsAffected = Random.Range(1, squad.Count + 1);
        
        for (int i = 0; i < targetsAffected; i++) {
            int soldierIndex = Random.Range(0, squad.Count);
            squad[soldierIndex].currentHealth -= damage;
            if (squad[soldierIndex].currentHealth <= 0) {
                squad.RemoveAt(i);
                TotalAlliesLost++;
            }
            if (squad.Count == 0) {
                PlayerWin = false;
                HandleGameOver();
            }
        }
    }

    public void HandleGameOver() {
        TotalScore = TotalSuppliesCollected + 10 * nodesVisited + 10 * TotalEnemiesKilled;
        SceneTransitionManager.Instance.LoadScene("EndingScene");
    }
}