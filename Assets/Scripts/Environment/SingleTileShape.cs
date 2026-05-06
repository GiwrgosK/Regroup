using UnityEngine;

public class SingleTileShape : ICoverShape {

    public void BlockAdjacentTiles(Transform coverTransform, CoverObject.CoverType coverType) {
        GridPosition centerGridPosition = LevelGrid.Instance.GetGridPosition(coverTransform.position);
    
        Vector2Int[] directions = new[] {
            Vector2Int.up,    // (0, 1)
            Vector2Int.down,  // (0, -1)
            Vector2Int.left,  // (-1, 0)
            Vector2Int.right  // (1, 0)
        };

        foreach (Vector2Int direction in directions) {
            GridPosition neighborPosition = new GridPosition(centerGridPosition.x + direction.x, centerGridPosition.z + direction.y);

            if (LevelGrid.Instance.IsValidGridPosition(neighborPosition)) {
                Vector2Int directionFromNeighborToCenter = new Vector2Int(-direction.x, -direction.y);
                LevelGrid.Instance.GetGridObjectAtGridPosition(neighborPosition).SetCoverManual(directionFromNeighborToCenter, coverType);
            }
        }
    }
}