using System.Collections.Generic;
using UnityEngine;

public class HorizontalSandbagShape : ICoverShape {
    public void BlockAdjacentTiles(Transform coverTransform, CoverObject.CoverType coverType) {
        float offset1 = 3f;
        float offset2 = 1f;

        List<Vector3> adjacentTiles = new List<Vector3> {
            coverTransform.TransformPoint(new Vector3(-offset1, 0, offset2)),   // 0: Top Far Left
            coverTransform.TransformPoint(new Vector3(-offset2, 0, offset2)),   // 1: Top Mid Left
            coverTransform.TransformPoint(new Vector3(offset2, 0, offset2)),    // 2: Top Mid Right
            coverTransform.TransformPoint(new Vector3(offset1, 0, offset2)),    // 3: Top Far Right
            coverTransform.TransformPoint(new Vector3(-offset1, 0, -offset2)),  // 4: Bottom Far Left
            coverTransform.TransformPoint(new Vector3(-offset2, 0, -offset2)),  // 5: Bottom Mid Left
            coverTransform.TransformPoint(new Vector3(offset2, 0, -offset2)),   // 6: Bottom Mid Right
            coverTransform.TransformPoint(new Vector3(offset1, 0, -offset2)),   // 7: Bottom Far Right
        };

        for (int i = 0; i < 4; i++) {
            GridPosition top = LevelGrid.Instance.GetGridPosition(adjacentTiles[i]);
            GridPosition bottom = LevelGrid.Instance.GetGridPosition(adjacentTiles[i + 4]);
            ApplyCoverToTiles(bottom, top, coverType);
        }
    }

    private void ApplyCoverToTiles(GridPosition a, GridPosition b, CoverObject.CoverType type) {
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

        if (LevelGrid.Instance.IsValidGridPosition(a)) {
            LevelGrid.Instance.GetGridObjectAtGridPosition(a).SetCoverManual(directionAToB, type);
        }

        if (LevelGrid.Instance.IsValidGridPosition(b)) {
            LevelGrid.Instance.GetGridObjectAtGridPosition(b).SetCoverManual(directionBToA, type);
        }
    }
}