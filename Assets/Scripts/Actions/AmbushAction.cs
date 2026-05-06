using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AmbushAction : BaseAction {
    public static event Action<Unit> OnAmbushSet;
    public static event Action<Unit> OnAmbushEnded;

    protected override string ActionName => "Ambush - Watching Post";
    protected override string ActionDescription => "Unit ends its turn and waits to fire upon the first enemy that moves within sight.";
    protected override int ActionCost => base.ActionCost;
	protected override int Range => 0;

    public override List<GridPosition> GetValidActionGridPositionList() {
        if (Unit.IsSuppressed || Unit.IsInAmbush) return new List<GridPosition>();
        return new List<GridPosition> { Unit.UnitGridPosition };
    }

    public void EndAmbush() {
        if (!Unit.IsInAmbush) return;
        Unit.IsInAmbush = false;
        OnAmbushEnded?.Invoke(Unit);
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition) {
        if (!CoverSystem.HasCover(Unit.UnitGridPosition)) {
            return new EnemyAIAction {
                gridPosition = gridPosition,
                actionValue = 0
            };
        }

        float closestDistance = float.MaxValue;
        foreach (Unit playerUnit in SpawnedUnitHandler.Instance.FriendlyUnits) {
            if (playerUnit.IsDead()) continue;
            float distance = Vector3.Distance(Unit.GetWorldPosition(), playerUnit.GetWorldPosition());
            if (distance < closestDistance) {
                closestDistance = distance;
            }
        }

        float maxEffectiveRange = Unit.Data.roleData.weaponRange + (Unit.Data.roleData.movementRange * 2);
        if (closestDistance > maxEffectiveRange) {
            return new EnemyAIAction {
                gridPosition = gridPosition,
                actionValue = 0
            };
        }

        int actionValue = 50;
        return new EnemyAIAction {
            gridPosition = gridPosition,
            actionValue = actionValue
        };
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete) {
        StartCoroutine(ExecuteAmbushAction(onActionComplete));
    }

    private IEnumerator ExecuteAmbushAction(Action onActionComplete) {
        ActionStart(onActionComplete);
        Unit.IsInAmbush = true;
        Unit.EmptyActionPoints();
        AmbushManager.Instance.RegisterAmbusher(Unit);
        OnAmbushSet?.Invoke(Unit);
        yield return null; 
        ActionComplete();
    }
}