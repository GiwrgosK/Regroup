using System;
using System.Collections.Generic;

public interface IAction {
    Unit Unit { get; }
    string ActionName { get; }
    string ActionDescription { get; }
    int ActionCost { get; }
    int Range { get; }

    bool IsValidActionGridPosition(GridPosition gridPosition);
    void TakeAction(GridPosition gridPosition, Action onActionComplete);
    List<GridPosition> GetValidActionGridPositionList();
    EnemyAIAction GetEnemyAIAction(GridPosition gridPosition);
    EnemyAIAction GetBestEnemyAIAction();
}