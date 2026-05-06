using UnityEngine;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour {
    public static Pathfinding Instance { get; private set; }

    [Header("Grid Debug Object Prefab")]
    [SerializeField] private Transform gridDebugObjectPrefab;

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private GridSystem<PathNode> gridSystem;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Setup(int width, int height, float cellSize, LayerMask voidLayerMask) {
        gridSystem = new GridSystem<PathNode>(width, height, cellSize, (GridSystem<PathNode> g, GridPosition gridPosition) => new PathNode(gridPosition), voidLayerMask);
        //gridSystem.CreateDebugObjects(gridDebugObjectPrefab);

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                PathNode node = GetNode(i, j);
                if (node == null) continue;

                GridPosition gridPosition = new GridPosition(i, j);
                GridObject gridObject = LevelGrid.Instance.GetGridObjectAtGridPosition(gridPosition);

                if (gridObject != null) {
                    node.IsWalkable = gridObject.IsWalkable(); 
                }
            }
        }
    }

    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength) {
        List<PathNode> openList = new List<PathNode>();
        HashSet<PathNode> closedSet = new HashSet<PathNode>();

        PathNode startNode = gridSystem.GetGridObject(startGridPosition);
        PathNode endNode = gridSystem.GetGridObject(endGridPosition);
        openList.Add(startNode);

        for (int i = 0; i < LevelGrid.Instance.Width; i++) {
            for (int j = 0; j < LevelGrid.Instance.Height; j++) {
                GridPosition gridPosition = new GridPosition(i, j);
                PathNode pathNode = gridSystem.GetGridObject(gridPosition);
                if (pathNode == null) continue;

                pathNode.GCost = int.MaxValue;
                pathNode.HCost = 0;
                pathNode.PreviousPathNode = null;
            }
        }

        startNode.GCost = 0;
        startNode.HCost = CalculateDistance(startGridPosition, endGridPosition);

        while (openList.Count > 0) {
            PathNode currentNode = GetMinFCostPathNode(openList);
            if (currentNode == endNode) {
                pathLength = endNode.FCost;
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedSet.Add(currentNode);

            foreach (PathNode neighbourNode in GetValidNeighbourList(currentNode)) {
                if (closedSet.Contains(neighbourNode)) {
                    continue;
                }

                if (!neighbourNode.IsWalkable) {
                    closedSet.Add(neighbourNode);
                    continue;
                }

                int baseCost = CalculateDistance(currentNode.GridPosition, neighbourNode.GridPosition);
                int tempGCost = currentNode.GCost + baseCost;
                if (tempGCost < neighbourNode.GCost) {
                    neighbourNode.PreviousPathNode = currentNode;
                    neighbourNode.GCost = tempGCost;
                    neighbourNode.HCost = CalculateDistance(neighbourNode.GridPosition, endGridPosition);

                    if (!openList.Contains(neighbourNode)) {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }
        pathLength = 0;
        return null;
    }

    public int CalculateDistance(GridPosition gridPositionA, GridPosition gridPositionB) {
        GridPosition gridPositionDistance = gridPositionA - gridPositionB;
        int xDistance = Mathf.Abs(gridPositionDistance.x);
        int zDistance = Mathf.Abs(gridPositionDistance.z);
        int remaining = Mathf.Abs(xDistance - zDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, zDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetMinFCostPathNode(List<PathNode> pathNodeList) {
        PathNode minFCostPathNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++) {
            if (pathNodeList[i].FCost < minFCostPathNode.FCost) {
                minFCostPathNode = pathNodeList[i];
            }
        }
        return minFCostPathNode;
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode) {
        List<PathNode> neighbourList = new List<PathNode>();
        GridPosition gridPosition = currentNode.GridPosition;
        if (gridSystem.GetGridObject(gridPosition) == null) return null;

        if (gridPosition.x - 1 >= 0) {
            neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z)); // Left
            if (gridPosition.z - 1 >= 0) {
                neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z - 1)); // Left Down
            }
            if (gridPosition.z + 1 < gridSystem.Height) {
                neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z + 1)); // Left Up
            }
        }

        if (gridPosition.x + 1 < gridSystem.Width) {
            neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z)); // Right
            if (gridPosition.z - 1 >= 0) {
               neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z - 1)); // Right Down
            }
            if (gridPosition.z + 1 < gridSystem.Height) {
               neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z + 1)); // Right Up
            }
        }
 
        if (gridPosition.z - 1 >= 0) {
            neighbourList.Add(GetNode(gridPosition.x, gridPosition.z - 1)); // Down
        }
        if (gridPosition.z + 1 < gridSystem.Height) {
            neighbourList.Add(GetNode(gridPosition.x, gridPosition.z + 1)); // Up
        }

        return neighbourList;
    }

    private List<PathNode> GetValidNeighbourList(PathNode currentNode) {
        List<PathNode> validNeighbourList = new List<PathNode>();
        GridObject currentGridObject = LevelGrid.Instance.GetGridObjectAtGridPosition(currentNode.GridPosition);

        foreach (PathNode pathNode in GetNeighbourList(currentNode)) {
            if (pathNode == null) continue;
            if (gridSystem.GetGridObject(pathNode.GridPosition) == null) continue;
            if (!pathNode.IsWalkable) continue;

            int dx = pathNode.GridPosition.x - currentNode.GridPosition.x;
            int dz = pathNode.GridPosition.z - currentNode.GridPosition.z;
            GridObject targetGridObject = LevelGrid.Instance.GetGridObjectAtGridPosition(pathNode.GridPosition);
            if (Mathf.Abs(dx) + Mathf.Abs(dz) == 1) {
                Vector2Int direction = new Vector2Int(dx, dz);
                if (currentGridObject.GetCoverInDirection(direction) != CoverObject.CoverType.None) continue;
            }

            if (Mathf.Abs(dx) == 1 && Mathf.Abs(dz) == 1) {
                GridPosition orthogonalX = new GridPosition(currentNode.GridPosition.x + dx, currentNode.GridPosition.z);
                GridPosition orthogonalZ = new GridPosition(currentNode.GridPosition.x, currentNode.GridPosition.z + dz);
                
                if (!IsGridPositionWalkable(orthogonalX) || !IsGridPositionWalkable(orthogonalZ)) continue;

                Vector2Int dirX = new Vector2Int(dx, 0);
                Vector2Int dirZ = new Vector2Int(0, dz);

                if (currentGridObject.GetCoverInDirection(dirX) != CoverObject.CoverType.None || currentGridObject.GetCoverInDirection(dirZ) != CoverObject.CoverType.None) {
                    continue;
                }

                if (targetGridObject.GetCoverInDirection(new Vector2Int(-dx, 0)) != CoverObject.CoverType.None || targetGridObject.GetCoverInDirection(new Vector2Int(0, -dz)) != CoverObject.CoverType.None) {
                    continue;
                }
            }

            validNeighbourList.Add(pathNode);
        }
        
        return validNeighbourList;
    }

    public PathNode GetNode(int x, int z) {
        return gridSystem.GetGridObject(new GridPosition(x, z));
    }

    private List<GridPosition> CalculatePath(PathNode endNode) {
        List<PathNode> pathNodeList = new List<PathNode>();
        pathNodeList.Add(endNode);
        PathNode currentNode = endNode;

        while (currentNode.PreviousPathNode != null) {
            pathNodeList.Add(currentNode.PreviousPathNode);
            currentNode = currentNode.PreviousPathNode;
        }
        pathNodeList.Reverse();
        List<GridPosition> gridPositionList = new List<GridPosition>();
        foreach (PathNode pathNode in pathNodeList) {
            gridPositionList.Add(pathNode.GridPosition);
        }
        return gridPositionList;
    }

    public List<GridPosition> GetReachableGridPositions(GridPosition start, int maxCost) {
        HashSet<GridPosition> reachablePositions = new HashSet<GridPosition>();
        Dictionary<GridPosition, int> costMap = new Dictionary<GridPosition, int>();
        Queue<GridPosition> queue = new Queue<GridPosition>();

        costMap[start] = 0;
        queue.Enqueue(start);

        while (queue.Count > 0) {
            GridPosition current = queue.Dequeue();
            int currentCost = costMap[current];

            foreach (PathNode neighborNode in GetValidNeighbourList(gridSystem.GetGridObject(current))) {
                GridPosition neighborPos = neighborNode.GridPosition;

                if (!neighborNode.IsWalkable) continue;

                int moveCost = CalculateDistance(current, neighborPos);
                int newCost = currentCost + moveCost;

                if (newCost > maxCost) continue;

                if (!costMap.ContainsKey(neighborPos) || newCost < costMap[neighborPos]) {
                    costMap[neighborPos] = newCost;
                    queue.Enqueue(neighborPos);
                    reachablePositions.Add(neighborPos);
                }
            }
        }
        return new List<GridPosition>(reachablePositions);
    }

    public bool IsGridPositionWalkable(GridPosition gridPosition) {
        if (!gridSystem.IsValidGridPosition(gridPosition)) return false;
        PathNode node = gridSystem.GetGridObject(gridPosition);
        return node != null && node.IsWalkable;
    }

    public void SetGridPositionWalkable(GridPosition gridPosition, bool isWalkable) {
        gridSystem.GetGridObject(gridPosition).IsWalkable = isWalkable;
    }

    public bool IsGridPositionReachable(GridPosition startGridPosition, GridPosition endGridPosition) {
        return FindPath(startGridPosition, endGridPosition, out int pathLength) != null;
    }

    public int GetPathLength(GridPosition startGridPosition, GridPosition endGridPosition) {
        if (FindPath(startGridPosition, endGridPosition, out int pathLength) != null) {
            return pathLength;
        }
        return -1;
    }
}