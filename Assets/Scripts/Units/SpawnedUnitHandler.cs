using UnityEngine;
using System.Collections.Generic;

public class SpawnedUnitHandler : MonoBehaviour {
    public static SpawnedUnitHandler Instance { get; private set; }

    public IReadOnlyList<Unit> AllUnits => allUnitList;
    public IReadOnlyList<Unit> FriendlyUnits => friendlyUnitList;
    public IReadOnlyList<Unit> EnemyUnits => enemyUnitList;

    private List<Unit> allUnitList;
    private List<Unit> friendlyUnitList;
    private List<Unit> enemyUnitList;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        allUnitList = new List<Unit>();
        friendlyUnitList = new List<Unit>();
        enemyUnitList = new List<Unit>();
    }

    private void Start() {
        Unit.OnAnyUnitSpawn += Unit_OnAnyUnitSpawn;
        Unit.OnAnyUnitDeath += Unit_OnAnyUnitDeath;
    }

    private void OnDestroy() {
        Unit.OnAnyUnitSpawn -= Unit_OnAnyUnitSpawn;
        Unit.OnAnyUnitDeath -= Unit_OnAnyUnitDeath;
    }

    private void Unit_OnAnyUnitSpawn(Unit unitSpawned) {
        allUnitList.Add(unitSpawned);
        if (unitSpawned.IsEnemy) {
            enemyUnitList.Add(unitSpawned);
        } else {
            friendlyUnitList.Add(unitSpawned);
            if (friendlyUnitList.Count == 1 && UnitHandler.Instance != null) {
                UnitHandler.Instance.SetSelectedUnit(unitSpawned);
            }
        }
    }

    private void Unit_OnAnyUnitDeath(Unit unitSpawned) {
        allUnitList.Remove(unitSpawned);
        if (unitSpawned.IsEnemy) {
            enemyUnitList.Remove(unitSpawned);
            GameManager.Instance.TotalEnemiesKilled++;
        } else {
            friendlyUnitList.Remove(unitSpawned);
            GameManager.Instance.TotalAlliesLost++;
        }
        //GridSystemVisual.Instance.
    }
}