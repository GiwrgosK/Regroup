using UnityEngine;

public class PathfindingHandler : MonoBehaviour {
    private void Start() {
        DestructibleObject.OnAnyDestroyed += DestructibleObject_OnAnyDestroyed;
    }

    private void OnDestroy() {
        DestructibleObject.OnAnyDestroyed -= DestructibleObject_OnAnyDestroyed;
    }

    private void DestructibleObject_OnAnyDestroyed(DestructibleObject destructibleObject) {
        Pathfinding.Instance.SetGridPositionWalkable(destructibleObject.GetGridPosition(), true);
    }
}