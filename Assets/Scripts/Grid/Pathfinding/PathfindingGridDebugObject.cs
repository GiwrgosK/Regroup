using TMPro;
using UnityEngine;

public class PathfindingGridDebugObject : GridDebugObject {
    [Header("Pathfinding Grid Debug Object Configuration")]
    [SerializeField] private TextMeshPro gCostText;
    [SerializeField] private TextMeshPro hCostText;
    [SerializeField] private TextMeshPro fCostText;
    [SerializeField] private SpriteRenderer isWalkableSpriteRenderer;

    private PathNode pathNode;

    public override object GridObject {
        get => base.GridObject;
        set {
            base.GridObject = value;
            pathNode = value as PathNode;
            UpdatePathNodeText();
        }
    }

    private void UpdatePathNodeText() {
        if (pathNode == null) return;

        gCostText.text = pathNode.GCost.ToString();
        hCostText.text = pathNode.HCost.ToString();
        fCostText.text = pathNode.FCost.ToString();
        isWalkableSpriteRenderer.color = pathNode.IsWalkable ? Color.green : Color.red;
    }
}