using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class BaseAction : MonoBehaviour, IAction {
	public static event Action<BaseAction> OnAnyActionStart;
	public static event Action<BaseAction> OnAnyActionEnd;

	public Unit Unit { get; private set; }

	protected virtual string ActionName => "";
	protected virtual string ActionDescription => "";
	protected virtual int ActionCost => 1;
	protected virtual int Range => 1;

	protected Action onActionComplete;
	protected bool isActive;
	protected bool isOfficer;

    Unit IAction.Unit => Unit;
	string IAction.ActionName => ActionName;
    string IAction.ActionDescription => ActionDescription;
	int IAction.ActionCost => ActionCost;
    int IAction.Range => Range;

	protected virtual void Awake() {
		Unit = GetComponent<Unit>();
	}

	public virtual bool IsValidActionGridPosition(GridPosition gridPosition) {
		List<GridPosition> validGridPositionList = GetValidActionGridPositionList();
		return validGridPositionList.Contains(gridPosition);
	}

	public virtual bool IsActionAvailable() {
		return true;
	}

	public EnemyAIAction GetBestEnemyAIAction() {
		List<GridPosition> validGridPositionList = GetValidActionGridPositionList();
		EnemyAIAction bestAIAction = null;

		foreach (GridPosition gridPosition in validGridPositionList) {
			EnemyAIAction testEnemyAIAction = GetEnemyAIAction(gridPosition);

			if (testEnemyAIAction != null) {
				if (bestAIAction == null || testEnemyAIAction.actionValue > bestAIAction.actionValue) {
					bestAIAction = testEnemyAIAction;
				}
			}
		}

		return bestAIAction;
	}

	protected void ActionStart(Action onActionComplete) {
		isActive = true;
		this.onActionComplete = onActionComplete;
		OnAnyActionStart?.Invoke(this);
	}
	
	protected void ActionComplete() {
		isActive = false;
		onActionComplete?.Invoke();
		OnAnyActionEnd?.Invoke(this);
	}

	public virtual void OnSelected() {
	}

	public virtual void OnDeselected() {
	}

	public abstract List<GridPosition> GetValidActionGridPositionList();
	public abstract EnemyAIAction GetEnemyAIAction(GridPosition gridPosition);
	public abstract void TakeAction(GridPosition gridPosition, Action onActionComplete);
}