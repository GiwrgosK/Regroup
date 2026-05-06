using UnityEngine;

public class PiSandbagShape : ICoverShape {
    public void BlockAdjacentTiles(Transform coverTransform, CoverObject.CoverType coverType) {
        float offset = 2f;

        Vector3 centerWorldPosition = coverTransform.position;
        Vector3 topWorldPosition = coverTransform.TransformPoint(new Vector3(0, 0, offset));
        Vector3 leftWorldPosition = coverTransform.TransformPoint(new Vector3(-offset, 0, 0));
        Vector3 rightWorldPosition = coverTransform.TransformPoint(new Vector3(offset, 0, 0));

        GridPosition centerGridPosition = LevelGrid.Instance.GetGridPosition(centerWorldPosition);
        GridPosition topGridPosition = LevelGrid.Instance.GetGridPosition(topWorldPosition);
        GridPosition leftGridPosition = LevelGrid.Instance.GetGridPosition(leftWorldPosition);
        GridPosition rightGridPosition = LevelGrid.Instance.GetGridPosition(rightWorldPosition);

        ApplyCoverToTiles(centerGridPosition, topGridPosition, coverType);
        ApplyCoverToTiles(centerGridPosition, leftGridPosition, coverType);
        ApplyCoverToTiles(centerGridPosition, rightGridPosition, coverType);
    }

    private void ApplyCoverToTiles(GridPosition a, GridPosition b, CoverObject.CoverType type) {
        if (!LevelGrid.Instance.IsValidGridPosition(a) || !LevelGrid.Instance.IsValidGridPosition(b)) return;
        if (a.x == b.x && a.z == b.z) return; 

        int diffX = b.x - a.x;
        int diffZ = b.z - a.z;

        Vector2Int directionAToB;
        if (Mathf.Abs(diffX) > Mathf.Abs(diffZ)) {
            directionAToB = new Vector2Int(System.Math.Sign(diffX), 0);
        } else {
            directionAToB = new Vector2Int(0, System.Math.Sign(diffZ));
        }

        Vector2Int directionBToA = new Vector2Int(-directionAToB.x, -directionAToB.y);
        LevelGrid.Instance.GetGridObjectAtGridPosition(a).SetCoverManual(directionAToB, type);
        LevelGrid.Instance.GetGridObjectAtGridPosition(b).SetCoverManual(directionBToA, type);
    }
}