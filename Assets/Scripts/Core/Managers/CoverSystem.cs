using UnityEngine;

public static class CoverSystem {
    public static int GetCoverPenalty(Unit shooter, Unit target) {
        GridObject targetGridObject = LevelGrid.Instance.GetGridObjectAtGridPosition(target.UnitGridPosition);
        if (targetGridObject == null) return 0;

        Vector3 directionFromTarget = (shooter.GetWorldPosition() - target.GetWorldPosition()).normalized;
        int bestCoverPenalty = 0;

        Vector2Int[] coverDirections = new[] {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        foreach (Vector2Int direction in coverDirections) {
            CoverObject.CoverType coverType = targetGridObject.GetCoverInDirection(direction);

            if (coverType != CoverObject.CoverType.None) {
                Vector3 coverWorldDir = new Vector3(direction.x, 0, direction.y).normalized;
                float dot = Vector3.Dot(coverWorldDir, directionFromTarget); // -1 = behind, 0 = side, 1 = front
                if (dot > 0.1f) {
                    int penalty = (coverType == CoverObject.CoverType.Full) ? 40 : 20;
                    if (penalty > bestCoverPenalty) {
                        bestCoverPenalty = penalty;
                    }
                }
            }
        }

        return bestCoverPenalty;
    }

    public static bool HasCover(GridPosition targetPosition) {
        GridObject gridObject = LevelGrid.Instance.GetGridObjectAtGridPosition(targetPosition);
        if (gridObject == null) return false;

        Vector2Int[] coverDirections = new[] {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        foreach (Vector2Int dir in coverDirections) {
            var cover = gridObject.GetCoverInDirection(dir);
            if (cover == CoverObject.CoverType.Half || cover == CoverObject.CoverType.Full) {
                return true;
            }
        }
        return false;
    }
}