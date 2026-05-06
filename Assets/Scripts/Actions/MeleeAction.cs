using UnityEngine;
using System;
using System.Collections.Generic;

public class MeleeAction : BaseAction {
    public static event Action OnAnyMeleeHit;
    public event Action OnMeleeActionStart;
    public event Action OnMeleeActionEnd;
    
    private enum State {
        beforeHit,
        afterHit
    }

    [Header("Melee Action Configuration")]
    [SerializeField] private AudioClip meleeSoundEffect;

    protected override string ActionName => "Knife";
    protected override string ActionDescription => "Engages the target in close quarters with a trench knife. Ignores cover and never misses.";
    protected override int ActionCost => base.ActionCost;
    protected override int Range => base.Range;

    private State state;
    private Unit targetUnit;
    private float stateTimer;
    private readonly int damage = 999;

    private void Update() {
        if (!isActive) return;

        stateTimer -= Time.deltaTime;
		switch (state) {
			case State.beforeHit:
                Vector3 aimingDirection = (targetUnit.GetWorldPosition() - Unit.GetWorldPosition()).normalized;
				float rotateSpeed = 10f;
			    transform.forward = Vector3.Lerp(transform.forward, aimingDirection, Time.deltaTime * rotateSpeed);
			    break;
			case State.afterHit:
			    break;
		}

		if (stateTimer <= 0f) {
			NextState();
		}
    }

    private void NextState() {
		switch (state) {
			case State.beforeHit:
                AudioManager.Instance.PlayClip(meleeSoundEffect);
				state = State.afterHit;
				float afterHitStateTime = 0.1f;
				stateTimer = afterHitStateTime;
                targetUnit.Damage(damage, transform.position, "melee");
                OnAnyMeleeHit?.Invoke();
				break;
			case State.afterHit:
                OnMeleeActionEnd?.Invoke();
                ActionComplete();
				break;
		}
	}

    public override List<GridPosition> GetValidActionGridPositionList() {
        List<GridPosition>  validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = Unit.UnitGridPosition;

		for (int i = -Range; i <= Range; i++) {
			for (int j = -Range; j<= Range; j++) {
				GridPosition offsetGridPosition = new GridPosition(i, j);
				GridPosition testGridPosition = unitGridPosition + offsetGridPosition;
				
				if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) {
					continue;
				}

                int testDistance = Mathf.Abs(i) + Mathf.Abs(j);
                if (testDistance > Range) continue;

                if (!LevelGrid.Instance.IsGridPositionOccupied(testGridPosition)) {
					continue;
				}

				Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);
				if (targetUnit.IsEnemy == Unit.IsEnemy) {
					continue;
				}

				validGridPositionList.Add(testGridPosition);
			}
		}

		return validGridPositionList;
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition) {
        Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        if (targetUnit == null) return null;

        int actionValue = 250;
        if (targetUnit.GetHealth() <= damage) {
            actionValue += 50;
        }
        
        return new EnemyAIAction {
            gridPosition = gridPosition,
            actionValue = actionValue
        };
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete) {
        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        state = State.beforeHit;
		float beforeHitStateTime = 0.7f;
		stateTimer = beforeHitStateTime;
        OnMeleeActionStart?.Invoke();
        ActionStart(onActionComplete);
    }
}