public class PathNode {
    public PathNode PreviousPathNode { get; set; }
    public GridPosition GridPosition { get; }
    public bool IsWalkable { get; set; } = true;
    public int GCost { get; set; }
    public int HCost { get; set; }
    public int FCost => GCost + HCost;

    public PathNode(GridPosition gridPosition) {
        GridPosition = gridPosition;
    }

    public override string ToString() {
        return GridPosition.ToString();
    }
}