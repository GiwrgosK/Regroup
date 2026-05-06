using UnityEngine;
using System;
using System.Collections.Generic;

public class SuppressAction : BaseAction {
    public event Action<Unit> OnSuppressing;

    private enum State {
		Aiming,
		Shooting,
		Cooldown
	}

    [Header("Suppress Action Configuration")]
    [SerializeField] private LayerMask obstacleLayerMask;
	[SerializeField] private AudioClip gunshotSoundEffect;

    protected override string ActionName => "Tactical Suppression - Covering Fire";
    protected override string ActionDescription => "Fire a barrage to pin the enemy. Does 5-10 damage and makes them unable to perform most actions.";
    protected override int ActionCost => base.ActionCost;
    protected override int Range => Unit.Data.roleData.weaponRange;

    private State state;
    private Unit targetUnit;
	private bool canShoot;
	private float stateTimer;
    private int damage;

    public Unit TargetUnit => targetUnit;

    private void Update() {
        if (!isActive) return;

        stateTimer -= Time.deltaTime;
		switch (state) {
			case State.Aiming:
				Vector3 aimingDirection = (targetUnit.GetWorldPosition() - Unit.GetWorldPosition()).normalized;
				float rotateSpeed = 10f;
				transform.forward = Vector3.Lerp(transform.forward, aimingDirection, Time.deltaTime * rotateSpeed);
				break;
			case State.Shooting:
				if (canShoot) {
					Suppress();
					canShoot = false;
				} 
				break;
			case State.Cooldown:
				break;
		}

		if (stateTimer <= 0f) {
			NextState();
		}
    }

    private void NextState() {
		switch (state) {
			case State.Aiming:
				state = State.Shooting;
                float shootingStateTime = 0.5f; 
                stateTimer = shootingStateTime;
                break;
			case State.Shooting:
				state = State.Cooldown;
                float cooldownStateTime = 0.5f;
                stateTimer = cooldownStateTime;
                break;
			case State.Cooldown:
				ActionComplete();
				break;
		}
	}

    private void Suppress() {
        OnSuppressing?.Invoke(targetUnit);
        AudioManager.Instance.PlayClip(gunshotSoundEffect);
        targetUnit.ApplySuppression();
        damage = Unit.Data.roleData.suppressDamage;
        targetUnit.Damage(damage, transform.position, "bullet");
    }

    public override List<GridPosition> GetValidActionGridPositionList() {
        if (Unit.IsSuppressed) return new List<GridPosition>();

        List<GridPosition>  validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = Unit.UnitGridPosition;
        float unitShoulderHeight = 1.7f;
        Vector3 raycastOrigin = Unit.GetWorldPosition() + Vector3.up * unitShoulderHeight;

		for (int i = -Range; i <= Range; i++) {
			for (int j = -Range; j<= Range; j++) {
				GridPosition offsetGridPosition = new GridPosition(i, j);
				GridPosition testGridPosition = unitGridPosition + offsetGridPosition;
				
				if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) {
					continue;
				}

                if (!LevelGrid.Instance.IsGridPositionOccupied(testGridPosition)) {
					continue;
				}

                int testDistance = Mathf.Abs(i) + Mathf.Abs(j);
                if (testDistance > Range) {
                    continue;
                }

				Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);
				if (targetUnit.IsEnemy == Unit.IsEnemy) {
					continue;
				}

                Vector3 targetRaycastOrigin = targetUnit.GetWorldPosition() + Vector3.up * unitShoulderHeight;
				Vector3 targetDirection = (targetRaycastOrigin - raycastOrigin).normalized;
                float distanceToTarget = Vector3.Distance(raycastOrigin, targetRaycastOrigin);
				if (Physics.Raycast(raycastOrigin, targetDirection, distanceToTarget, obstacleLayerMask)) {
					continue;
				}

				validGridPositionList.Add(testGridPosition);
			}
		}

		return validGridPositionList;
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition) {
        Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        if (targetUnit.IsSuppressed) {
            return new EnemyAIAction {
                gridPosition = gridPosition,
                actionValue = 0
            };
        }

        int normalDamage = Unit.Data.roleData.weaponDamage;
        if (targetUnit.GetHealth() <= normalDamage) {
            return new EnemyAIAction {
                gridPosition = gridPosition,
                actionValue = 0
            }; 
        }

        int actionValue = 0;
        ShootAction shootAction = Unit.GetAction<ShootAction>();
        if (shootAction != null) {
            int hitChance = shootAction.GetHitChance(targetUnit);
            if (hitChance < 40) {
                actionValue = 100;
            } else if (hitChance < 60) {
                actionValue = 50;
            } else {
                actionValue = 10;
            }
        }
        
        return new EnemyAIAction {
            gridPosition = gridPosition,
            actionValue = actionValue
        };
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete) {
        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        state = State.Aiming;
        float aimingStateTime = 1f;
        stateTimer = aimingStateTime;
        canShoot = true;
        ActionStart(onActionComplete);
    }
}