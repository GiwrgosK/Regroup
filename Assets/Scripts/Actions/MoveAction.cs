using UnityEngine;
using System;
using System.Collections.Generic;

public class MoveAction : BaseAction {
	public event Action OnStartMoving;
	public event Action OnStopMoving;
	public event Action OnGotBehindCover;

	[Header("Move Action Configuration")]
	[SerializeField] private AudioClip movingSoundEffect;

	protected override string ActionName => "Marching Forward";
    protected override string ActionDescription => "Relocate to a new position. Triggers an enemy's ambush.";
    protected override int ActionCost => base.ActionCost;
	protected override int Range => Unit.Data.roleData.movementRange;

	private List<Vector3> positionList;
	private AudioSource movingAudioSource;
	private int currentPositionIndex;

	private void Start() {
        OnStartMoving += MoveAction_OnStartMoving;
        OnStopMoving += MoveAction_OnStopMoving;
    }

	private void Update() {
		if (!isActive) return;
		if (Unit == null || Unit.IsDead()) {
			OnStopMoving?.Invoke();
        	ActionComplete();
			return;
		}

		AmbushManager.Instance.CheckAmbush(Unit);

		Vector3 targetPosition = positionList[currentPositionIndex];
		Vector3 moveDirection = (targetPosition - transform.position).normalized;

		float rotateSpeed = 720f;
		float stoppingDistance = 0.1f;
		float moveSpeed = 4f;

		if (moveDirection != Vector3.zero) {
			Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
		}

		if (Vector3.Distance(transform.position, targetPosition) > stoppingDistance) {
			transform.position += moveSpeed * Time.deltaTime * moveDirection;
		} else {
			currentPositionIndex++;
			if (currentPositionIndex >= positionList.Count) {
				GetBehindCover();
				OnStopMoving?.Invoke();
				ActionComplete();
			}
		}
	}

	private void OnDestroy() {
		OnStartMoving -= MoveAction_OnStartMoving;
        OnStopMoving -= MoveAction_OnStopMoving;
	}

	public override List<GridPosition> GetValidActionGridPositionList() {
        if (Unit.IsSuppressed) return new List<GridPosition>();

		List<GridPosition> reachableGridPositions = Pathfinding.Instance.GetReachableGridPositions(Unit.UnitGridPosition, Range * 10);
		List<GridPosition> validGridPositions = new List<GridPosition>();
		foreach (GridPosition gridPosition in reachableGridPositions) {
			if (!LevelGrid.Instance.IsGridPositionOccupied(gridPosition)) {
				validGridPositions.Add(gridPosition);
			}		
		}

		return validGridPositions;
    }

	public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition) {
		if (Unit.IsSuppressed) return null;

		Unit closestTarget = GetClosestPlayerUnit(gridPosition);
		if (closestTarget == null) { 
			return new EnemyAIAction{ 
				gridPosition = gridPosition, 
				actionValue = 0
			};
		}

		int currentPositionScore = EvaluatePosition(Unit.UnitGridPosition, closestTarget);
		int destinationScore = EvaluatePosition(gridPosition, closestTarget);
		int scoreImprovement = destinationScore - currentPositionScore;
		if (scoreImprovement <= 0) {
			return new EnemyAIAction{ 
				gridPosition = gridPosition, 
				actionValue = 0
			};
		}

		int actionValue = 20 + scoreImprovement;
		actionValue = Mathf.Clamp(actionValue, 10, 150);

		return new EnemyAIAction {
			gridPosition = gridPosition,
			actionValue = actionValue
		};
	}

	private int EvaluatePosition(GridPosition testGridPosition, Unit targetUnit) {
		int score = 0;
		Vector3 testWorldPosition = LevelGrid.Instance.GetWorldPosition(testGridPosition);
		Vector3 targetUnitWorldPosition = targetUnit.GetWorldPosition();
		float distance = Vector3.Distance(testWorldPosition, targetUnitWorldPosition);

		score -= Mathf.RoundToInt(distance * 4);

		float weaponRange = Unit.Data.roleData.weaponRange;
        float sprintRange = Unit.Data.roleData.movementRange * 2f;
		bool isInCombatZone = distance <= (weaponRange + sprintRange);

		if (isInCombatZone) {
			CoverObject.CoverType cover = EvaluateCoverType(testGridPosition, targetUnit.UnitGridPosition);
            if (cover == CoverObject.CoverType.Full) {
				score += 60;
			} else if (cover == CoverObject.CoverType.Half) {
				score += 30;
			}
		}

		if (distance <= weaponRange) {
        	score += 20;
    	}

		return score;
	}

	private CoverObject.CoverType EvaluateCoverType(GridPosition unitGridPosition, GridPosition targetGridPosition) {
    	Vector2Int directionToTarget = new Vector2Int(
            Mathf.Clamp(targetGridPosition.x - unitGridPosition.x, -1, 1),
            Mathf.Clamp(targetGridPosition.z - unitGridPosition.z, -1, 1)
        );

		if (Mathf.Abs(targetGridPosition.x - unitGridPosition.x) > Mathf.Abs(targetGridPosition.z - unitGridPosition.z)) {
            directionToTarget.y = 0;
        } else {
            directionToTarget.x = 0;
        }

		GridObject gridObject = LevelGrid.Instance.GetGridObjectAtGridPosition(unitGridPosition);
        return gridObject.GetCoverInDirection(directionToTarget);
    }

	private Unit GetClosestPlayerUnit(GridPosition fromPosition) {
        Unit closest = null;
        int bestDistance = int.MaxValue;

        foreach (Unit playerUnit in SpawnedUnitHandler.Instance.FriendlyUnits) {
            if (playerUnit.IsDead()) continue;

			int distance = Mathf.Abs(fromPosition.x - playerUnit.UnitGridPosition.x) + Mathf.Abs(fromPosition.z - playerUnit.UnitGridPosition.z);

            if (distance < bestDistance) {
                bestDistance = distance;
                closest = playerUnit;
            }
        }
        return closest;
    }

	private void GetBehindCover() {
		Vector2Int[] directions = {
			Vector2Int.up,
			Vector2Int.down,
			Vector2Int.left,
			Vector2Int.right
		};

		GridPosition currentGridPosition = Unit.UnitGridPosition;
		GridObject gridObject = LevelGrid.Instance.GetGridObjectAtGridPosition(currentGridPosition);

		foreach (Vector2Int direction in directions) {
			CoverObject.CoverType type = gridObject.GetCoverInDirection(direction);
			if (type != CoverObject.CoverType.None) {
            	//Debug.Log($"<color=green>Unit {Unit.name}</color> is now in {type} Cover at {currentGridPosition}");
				Vector3 forwardDirection = new Vector3(direction.x, 0, direction.y);
				transform.rotation = Quaternion.LookRotation(forwardDirection);
				OnGotBehindCover?.Invoke();
				return;
			}
		}
	}

	public override void TakeAction(GridPosition gridPosition, Action onActionComplete) {
		if (Unit.IsSuppressed) {
            ActionStart(onActionComplete);
            ActionComplete();
            return;
        }
        List<GridPosition> pathGridPositionList = Pathfinding.Instance.FindPath(Unit.UnitGridPosition, gridPosition, out int pathLength);
		if (pathGridPositionList == null) {
            ActionComplete();
            return;
        }
		
		currentPositionIndex = 0;
        positionList = new List<Vector3>();
        foreach (GridPosition pathGridPosition in pathGridPositionList) {
            positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPosition));
        }

        OnStartMoving?.Invoke();
        ActionStart(onActionComplete);
    }

	private void MoveAction_OnStartMoving() {
        movingAudioSource = AudioManager.Instance.PlayLoopingSoundEffect(movingSoundEffect, transform);
    }

	private void MoveAction_OnStopMoving() {
        if (movingAudioSource != null) {
            movingAudioSource.Stop();
			Destroy(movingAudioSource.gameObject);
			movingAudioSource = null;
        }
    }
}