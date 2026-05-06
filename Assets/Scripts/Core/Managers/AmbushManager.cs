using UnityEngine;
using System.Collections.Generic;

public class AmbushManager : MonoBehaviour {
    public static AmbushManager Instance { get; private set; }

    [Header("Ambush Manager Configuration")]
    [SerializeField] private LayerMask obstacleLayerMask;

    private List<Unit> ambushers = new List<Unit>();

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() {
        TurnHandler.Instance.OnTurnChanged += TurnHandler_OnTurnChanged;
    }

    private void OnDestroy() {
        TurnHandler.Instance.OnTurnChanged -= TurnHandler_OnTurnChanged;
    }

    public void RegisterAmbusher(Unit unit) {
        ambushers.Add(unit);
    }

    public void CheckAmbush(Unit movingUnit) {
        if (movingUnit == null || movingUnit.IsDead()) return;

        foreach (Unit ambusher in new List<Unit>(ambushers)) {
            if (ambusher.IsDead() || !ambusher.IsInAmbush) continue;
            if (ambusher.IsEnemy == movingUnit.IsEnemy) continue;

            GridPosition ambusherGridPosition = LevelGrid.Instance.GetGridPosition(ambusher.GetWorldPosition());
            GridPosition movingUnitGridPosition = LevelGrid.Instance.GetGridPosition(movingUnit.GetWorldPosition());

            float tileDistance = Mathf.Max(Mathf.Abs(ambusherGridPosition.x - movingUnitGridPosition.x), Mathf.Abs(ambusherGridPosition.z - movingUnitGridPosition.z));
            float attackRange = ambusher.Data.roleData.weaponRange;
            if (tileDistance > attackRange) continue;

            float shoulderHeight = 1.7f;
            Vector3 ambusherWorldPosition = ambusher.GetWorldPosition() + Vector3.up * shoulderHeight;
            Vector3 targetWorldPosition = movingUnit.GetWorldPosition() + Vector3.up * shoulderHeight;
            Vector3 direction = (targetWorldPosition - ambusherWorldPosition).normalized;
            float trueDistance = Vector3.Distance(ambusherWorldPosition, targetWorldPosition);

            Debug.DrawRay(ambusherWorldPosition, direction * trueDistance, Color.red, 2000f);

            if (Physics.Raycast(ambusherWorldPosition, direction, trueDistance, obstacleLayerMask)) {
                continue; 
            }

            TimeManager.Instance.SlowMotion(0.25f, 2.5f);
            GridPosition enemyGridPosition = LevelGrid.Instance.GetGridPosition(movingUnit.transform.position);
            ShootAction shootAction = ambusher.GetAction<ShootAction>();
            shootAction.TakeAction(enemyGridPosition, () => {});
            ambusher.GetAction<AmbushAction>().EndAmbush();
            ambushers.Remove(ambusher);
            return;
        }

        return;
    }

    private void TurnHandler_OnTurnChanged() {
        List<Unit> unitsToRemove = new List<Unit>();
        foreach (Unit ambusher in ambushers) {
            if (TurnHandler.Instance.IsPlayersTurn && !ambusher.IsEnemy) {
                unitsToRemove.Add(ambusher);
            } else if (!TurnHandler.Instance.IsPlayersTurn && ambusher.IsEnemy) {
                unitsToRemove.Add(ambusher);
            }
        }

        foreach (Unit unit in unitsToRemove) {
            unit.GetAction<AmbushAction>().EndAmbush();
            ambushers.Remove(unit);
        }
    }
}