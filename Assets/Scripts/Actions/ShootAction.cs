using UnityEngine;
using System;
using System.Collections.Generic;

public class ShootAction : BaseAction {
	public static event Action<OnShootEventArgs> OnAnyShoot;
	public event Action<OnShootEventArgs> OnShoot;

	public class OnShootEventArgs : EventArgs {
		public Unit target;
		public Unit shooter;
		public bool hit;
	}

	private enum State {
		Aiming,
		Shooting,
		Cooldown
	}

	[Header("Shoot Action Configuration")]
	[SerializeField] private LayerMask obstacleLayerMask;
	[SerializeField] private AudioClip gunshotSoundEffect;

    protected override string ActionName => "Fire";
	protected override string ActionDescription => "Fire primary weapon at a designated target. Hit chance depends on cover and range.";
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
					Shoot();
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
				if (stateTimer <= 0f) {
					state = State.Shooting;
					float shootingStateTime = 0.2f;
					stateTimer = shootingStateTime;
				}
				break;
			case State.Shooting:
				if (stateTimer <= 0f) {
					state = State.Cooldown;
					float cooldownStateTime = 0.5f;
					stateTimer = cooldownStateTime;
				}
				break;
			case State.Cooldown:
				ActionComplete();
				break;
		}
	}

	public override List<GridPosition> GetValidActionGridPositionList() {
		List<GridPosition>  validGridPositionList = new List<GridPosition>();

		int effectiveRange = Unit.IsSuppressed ? Range / 2 : Range;
		Vector3 shooterWorldPosition = Unit.GetWorldPosition();
		float unitShoulderHeight = 1.7f;
        Vector3 raycastOrigin = shooterWorldPosition + Vector3.up * unitShoulderHeight;
		
		for (int i = -effectiveRange; i <= effectiveRange; i++) {
			for (int j = -effectiveRange; j<= effectiveRange; j++) {
				GridPosition offsetGridPosition = new GridPosition(i, j);
				GridPosition testGridPosition = Unit.UnitGridPosition + offsetGridPosition;
				
				if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) {
					continue;
				}

				int testDistance = Mathf.Abs(i) + Mathf.Abs(j);
				if (testDistance > effectiveRange) {
					continue;
				}

				if (!LevelGrid.Instance.IsGridPositionOccupied(testGridPosition)) {
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

	public override void OnSelected() {
        foreach (GridPosition gridPos in GetValidActionGridPositionList()) {
			Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPos);
			if (targetUnit != null) {
				int hitChance = GetHitChance(targetUnit);
				targetUnit.GetUnitUI().SetHitChance(hitChance);
			}
		}
    }

    public override void OnDeselected() {
        base.OnDeselected();
		foreach (Unit unit in SpawnedUnitHandler.Instance.EnemyUnits) {
			unit.GetUnitUI().SetHitChance(null);
		}
    }
	
	private void Shoot() {
		if (targetUnit == null || targetUnit.IsDead()) {
        	ActionComplete(); 
        	return; 
    	}

		damage = Unit.Data.roleData.weaponDamage;
		int hitChance = GetHitChance(targetUnit);
		int roll = UnityEngine.Random.Range(0, 100);
		bool shotHit = roll < hitChance;

		OnAnyShoot?.Invoke(new OnShootEventArgs {
			target = targetUnit,
			shooter = Unit,
			hit = shotHit
		});
		OnShoot?.Invoke(new OnShootEventArgs {
			target = targetUnit,
			shooter = Unit,
			hit = shotHit
		});

		AudioManager.Instance.PlayClip(gunshotSoundEffect);
		if (shotHit) {
			targetUnit.Damage(damage, transform.position, "bullet");
		}
	}

	public int GetHitChance(Unit targetUnit) {
		int baseHitChance = 100;

		int coverPenalty = CoverSystem.GetCoverPenalty(Unit, targetUnit);
		//Debug.Log("Cover Penalty: " + coverPenalty);

		float distanceToTarget = Vector3.Distance(Unit.GetWorldPosition(), targetUnit.GetWorldPosition());
        float optimalRangeEnd = Range * 0.4f;
		int distancePenalty = 0;

		if (distanceToTarget > optimalRangeEnd) {
            distancePenalty = Mathf.RoundToInt((distanceToTarget - optimalRangeEnd) * 3);
        }

		//Debug.Log("Distance Penalty: " + distancePenalty);

		int flankingBonus = GetFlankingBonus(Unit, targetUnit);
		//Debug.Log("Flanking Bonus: " + flankingBonus);

		int finalHitChance = Mathf.Clamp(baseHitChance - coverPenalty - distancePenalty + flankingBonus, 10, 95);
		//Debug.Log("Final Hit Chance: " + finalHitChance);

		return finalHitChance;
	}

	private int GetFlankingBonus(Unit attacker, Unit target) {
		Vector3 toAttacker = (attacker.transform.position - target.transform.position).normalized;
		Vector3 targetForward = target.GetForwardDirection();

		float dot = Vector3.Dot(targetForward, toAttacker); // -1 = behind, 0 = side, 1 = front
		if (dot < -0.5f) {
			return 25; // Behind
		} else if (dot < 0.5f) {
			return 10; // Side
		}
		return 0; // In front
	}

	public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition) {
		Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);

		if (targetUnit == null) {
			return new EnemyAIAction {
				gridPosition = gridPosition,
				actionValue = 0
			};
		}

		int hitChance = GetHitChance(targetUnit);
		int actionValue = hitChance;
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
		state = State.Aiming;
		float aimingStateTime = 1f;
		stateTimer = aimingStateTime;
		canShoot = true;
		ActionStart(onActionComplete);
	}
}