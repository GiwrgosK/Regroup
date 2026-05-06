using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour {
    public static CombatManager Instance { get; private set; }

    [Serializable] private struct UnitRoleConfig {
        public SoldierRoleData roleData;
        public GameObject rolePrefab;
    }

    [Header("Player Units Configuration")]
    [SerializeField] private List<UnitRoleConfig> playerUnitMappings;
    
    [Header("Enemy Units Configuration")]
    [SerializeField] private List<UnitRoleConfig> enemyUnitMappings;

    [Header("Player Units Starting Spawn Position")]
    [SerializeField] private float playerUnitsXSpawnPosition;

    [Header("Enemy Units Starting Spawn Position")]
    [SerializeField] private float minEnemyZ;

    private Dictionary<string, UnitRoleConfig> playerPrefabLookupDictionary;
    private Dictionary<string, UnitRoleConfig> enemyPrefabLookupDictionary;
    private Vector3 cameraPosition;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializeLookupDictionaries();
        cameraPosition = Camera.main.transform.position;
    }

    private void Start() {
        Unit.OnAnyUnitDeath += Unit_OnAnyUnitDeath;
    }

    private void OnDestroy() {
        Unit.OnAnyUnitDeath -= Unit_OnAnyUnitDeath;
    }

    private void InitializeLookupDictionaries() {
        playerPrefabLookupDictionary = new Dictionary<string, UnitRoleConfig>();
        foreach (UnitRoleConfig unitRoleConfig in playerUnitMappings) {
            if (!playerPrefabLookupDictionary.ContainsKey(unitRoleConfig.roleData.roleName)) {
                playerPrefabLookupDictionary.Add(unitRoleConfig.roleData.roleName, unitRoleConfig);
            }
        }

        enemyPrefabLookupDictionary = new Dictionary<string, UnitRoleConfig>();
        foreach (UnitRoleConfig enemyRoleConfig in enemyUnitMappings) {
            if (!enemyPrefabLookupDictionary.ContainsKey(enemyRoleConfig.roleData.roleName)) {
                enemyPrefabLookupDictionary.Add(enemyRoleConfig.roleData.roleName, enemyRoleConfig);
            }
        }
    }

    public void SetupCombat() {
        float xSpawnPosition = playerUnitsXSpawnPosition;

        foreach (SoldierData soldier in GameManager.Instance.squad) {
            if (!playerPrefabLookupDictionary.TryGetValue(soldier.roleData.roleName, out UnitRoleConfig playerRoleConfig)) {
                Debug.LogError($"CombatManager: No Unit config defined for role '{soldier.roleData.roleName}'");
                return; 
            } else {
                GameObject unitGameObject = Instantiate(playerRoleConfig.rolePrefab, new Vector3(xSpawnPosition, 0f, 0f), Quaternion.identity);
                Unit unit = unitGameObject.GetComponent<Unit>();
                unit.Initialize(soldier);
                xSpawnPosition += 2f;
            }
        }

        MapNode mapNode = Map.Instance.GetCurrentNode();
        if (mapNode.Type == MapNode.NodeType.Ending) {
            xSpawnPosition = playerUnitsXSpawnPosition;
            
            if (playerPrefabLookupDictionary.TryGetValue("Rifleman", out UnitRoleConfig playerRoleConfig)) {
                for (int i = 0; i < 4; i++) {
                    GameObject bonusUnitGameObject = Instantiate(playerRoleConfig.rolePrefab , new Vector3(xSpawnPosition, 0f, 2f), Quaternion.identity);
                    Unit bonusUnit = bonusUnitGameObject.GetComponent<Unit>();

                    SoldierData bonusData = new SoldierData {
                        firstName = "Bonus",
                        lastName = "Reinforcement",
                        roleData = playerRoleConfig.roleData,
                        currentHealth = playerRoleConfig.roleData.maxHealth,
                    };

                    bonusUnit.Initialize(bonusData);
                    xSpawnPosition += 2f;
                }
            }
        }

        if (SpawnedUnitHandler.Instance != null && SpawnedUnitHandler.Instance.FriendlyUnits.Count > 0) {
            UnitHandler.Instance.SetSelectedUnit(SpawnedUnitHandler.Instance.FriendlyUnits[0]);
        }
        SpawnEnemiesBasedOnDifficulty();
    }

    private void SpawnEnemiesBasedOnDifficulty() {
        int visitedNodesCount = GameManager.Instance.NodesVisited;
        int officerCount;
        int riflemanCount;
        int sniperCount;
        int heavyGunnerCount;
        
        if (visitedNodesCount <= 10) {
            officerCount = UnityEngine.Random.Range(0, 1);
            riflemanCount = UnityEngine.Random.Range(1, 3);
            sniperCount = 0;
            heavyGunnerCount = 0;
        } else if (visitedNodesCount <= 20) {
            officerCount = UnityEngine.Random.Range(1, 4);
            riflemanCount = UnityEngine.Random.Range(4, 8);
            sniperCount = UnityEngine.Random.Range(0, 1);
            heavyGunnerCount = UnityEngine.Random.Range(0, 2);
        } else {
            officerCount = UnityEngine.Random.Range(3, 5);
            riflemanCount = UnityEngine.Random.Range(5, 10);
            sniperCount = UnityEngine.Random.Range(1, 2);
            heavyGunnerCount = UnityEngine.Random.Range(2, 4);
        }

        //Debug.Log($"Officer Count: {officerCount}");
        //Debug.Log($"Rifleman Count: {riflemanCount}");
        //Debug.Log($"Sniper Count: {sniperCount}");
        //Debug.Log($"Heavy Gunner Count: {heavyGunnerCount}");

        SpawnEnemyBatch("Officer", officerCount);
        SpawnEnemyBatch("Rifleman", riflemanCount);
        SpawnEnemyBatch("Sniper", sniperCount);
        SpawnEnemyBatch("Heavy Gunner", heavyGunnerCount);
    }

    private void SpawnEnemyBatch(string roleName, int count) {
        if (!enemyPrefabLookupDictionary.TryGetValue(roleName, out UnitRoleConfig enemyRoleConfig)) {
            Debug.LogError($"CombatManager: No Enemy config defined for role '{roleName}'");
            return; 
        }

        for (int i = 0; i < count; i++) {
            GridPosition? randomGridPosition = FindRandomEnemyPosition();
            if (randomGridPosition != null) {
                Vector3 spawnPosition = LevelGrid.Instance.GetWorldPosition((GridPosition)randomGridPosition);
                Vector3 directionToCamera = cameraPosition - spawnPosition;
                directionToCamera.y = 0;
                Quaternion lookRotation = (directionToCamera != Vector3.zero) ? Quaternion.LookRotation(directionToCamera) : Quaternion.identity;

                GameObject spawnedEnemy = Instantiate(enemyRoleConfig.rolePrefab, spawnPosition, lookRotation);

                SoldierData enemyData = new SoldierData {
                    firstName = "German",
                    lastName = roleName,
                    roleData = enemyRoleConfig.roleData,
                    currentHealth = enemyRoleConfig.roleData.maxHealth,
                };

                Unit spawnedEnemyUnit = spawnedEnemy.GetComponent<Unit>();
                spawnedEnemyUnit.Initialize(enemyData);
            }
        }
    }

    public void EndCombat() {
        MapNode mapNode = Map.Instance.GetCurrentNode();
        if (mapNode.Type == MapNode.NodeType.Ending) {
            GameManager.Instance.PlayerWin = true;
            GameManager.Instance.HandleGameOver();
        } else {
            SceneTransitionManager.Instance.LoadScene("CampaignMapScene");   
        }
    }

    private GridPosition? FindRandomEnemyPosition() {
        int maxGridWidth = LevelGrid.Instance.Width;
        int maxGridHeight = LevelGrid.Instance.Height;
        int maxAttempts = 100;

        for (int i = 0; i < maxAttempts; i++) {
            int randomX = UnityEngine.Random.Range(0, maxGridWidth);
            int randomZ = UnityEngine.Random.Range((int) minEnemyZ, maxGridHeight);
            GridPosition testGridPosition = new GridPosition(randomX, randomZ);
            
            if (LevelGrid.Instance.IsValidGridPosition(testGridPosition)) {
                GridObject testGridObject = LevelGrid.Instance.GetGridObjectAtGridPosition(testGridPosition);

                if (testGridObject != null && !testGridObject.IsOccupied() && testGridObject.IsWalkable()) {
                    return testGridPosition;
                }
            }
        }
        return null;
    }

    private void SaveFriendlyHealth() {
        foreach (Unit unit in SpawnedUnitHandler.Instance.FriendlyUnits) {
            HealthHandler healthHandler = unit.GetComponent<HealthHandler>();
            
            foreach (SoldierData soldier in GameManager.Instance.squad) {
                if (soldier.serialNumber == unit.Data.serialNumber) {
                    soldier.currentHealth = healthHandler.CurrentHealth;
                    break;
                }
            }
        }
    }

    private void Unit_OnAnyUnitDeath(Unit _) {
        if (SpawnedUnitHandler.Instance.EnemyUnits.Count == 0) {
            SaveFriendlyHealth();
            EndCombat();
        } else if (SpawnedUnitHandler.Instance.FriendlyUnits.Count == 0) {
            GameManager.Instance.PlayerWin = false;
            GameManager.Instance.HandleGameOver();
        }
    }
}