using UnityEngine;
using System;

public class LevelGrid : MonoBehaviour {
	public static LevelGrid Instance { get; private set; }

	public event EventHandler<OnMoveEventArgs> OnAnyUnitMove;

	public class OnMoveEventArgs : EventArgs {
		public GridPosition gridPosition;
	}

	[Serializable] public struct CameraBoundsRegion {
		[Tooltip("Starting X coordinate on the grid")]
		public int startGridX;
		[Tooltip("Starting Z coordinate on the grid")]
		public int startGridZ;
		[Tooltip("How many cells wide this region is")]
		public int widthInCells;
		[Tooltip("How many cells tall this region is")]
		public int heightInCells;
	}

	[Header("Level Settings")]
	[SerializeField] private LayerMask obstacleLayerMask;
	[SerializeField] private LayerMask voidLayerMask;
	[SerializeField] private Transform gridDebugObjectPrefab;
	[SerializeField] private CameraHandler cameraHandler;

	[Header("Level Grid Size")]
	[SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;

	[Header("Level Camera Limits")]
	[SerializeField] private float maxEdgePadding = 5f; 
    [SerializeField] private float minEdgePadding = 0f;

	[Header("Level Regions")]
    [SerializeField] private CameraBoundsRegion[] cameraRegions;

	private GridSystem<GridObject> gridSystem;
	private GridTile[,] gridTileArray;

	public int Width => gridSystem.Width;
	public int Height => gridSystem.Height;
	public float CellSize => gridSystem.CellSize;

	private void Awake() {
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}		
		Instance = this;
		gridSystem = new GridSystem<GridObject>(width, height, cellSize, (GridSystem<GridObject> g,	GridPosition gridPosition) => new GridObject(g, gridPosition), voidLayerMask);
		gridTileArray = new GridTile[width, height];
		//gridSystem.CreateDebugObjects(gridDebugObjectPrefab);
		ScanGridForWalkability();
	}

	private void Start() {
		Pathfinding.Instance.Setup(width, height, cellSize, voidLayerMask);
		InitializeCover();
		InitializeCameraBounds();
	}

	private void InitializeCover() {
		CoverObject[] coverObjects = FindObjectsByType<CoverObject>(FindObjectsSortMode.None);
    	foreach (CoverObject cover in coverObjects) {
        	cover.SetupCover();
    	}
		for (int x = 0; x < width; x++) {
			for (int z = 0; z < height; z++) {
				GridObject gridObject = gridSystem.GetGridObject(new GridPosition(x, z));
				gridObject?.CalculateCoverAroundTile();
			}
		}
	}

	private void InitializeCameraBounds() {
		if (cameraRegions == null || cameraRegions.Length == 0) {
			float finalMinX = 0f + minEdgePadding;
            float finalMaxX = (Width * CellSize) - maxEdgePadding;
            float finalMinZ = 0f + minEdgePadding;
            float finalMaxZ = (Height * CellSize) - maxEdgePadding;

            Rect defaultMapRect = new Rect(finalMinX, finalMinZ, finalMaxX - finalMinX, finalMaxZ - finalMinZ);
            cameraHandler.SetCameraBounds(new Rect[] { defaultMapRect });
            return;
        }
		
		Rect[] calculatedRects = new Rect[cameraRegions.Length];
        for (int i = 0; i < cameraRegions.Length; i++) {
            CameraBoundsRegion region = cameraRegions[i];

            float minX = region.startGridX + minEdgePadding;
            float minZ = region.startGridZ + minEdgePadding;

            float maxX = region.startGridX + (region.widthInCells * CellSize) - maxEdgePadding;
            float maxZ = region.startGridZ + (region.heightInCells * CellSize) - maxEdgePadding;

            calculatedRects[i] = new Rect(minX, minZ, maxX - minX, maxZ - minZ);
        }

        cameraHandler.SetCameraBounds(calculatedRects);
	}

	private void ScanGridForWalkability() {
		for (int x = 0; x < width; x++) {
			for (int z = 0; z < height; z++) {
				GridPosition gridPosition = new GridPosition(x, z);
				Vector3 worldPosition = GetWorldPosition(gridPosition);
				float raycastOffset = 5f;
				if (Physics.Raycast(worldPosition + Vector3.down * raycastOffset, Vector3.up, raycastOffset * 2, obstacleLayerMask)) {
					GridObject gridObject = GetGridObjectAtGridPosition(gridPosition);
					gridObject?.SetIsWalkable(false);
				}
			}
		}
	}

	public void RecalculateCoverAround(GridPosition centerPosition) {
		Vector2Int[] directions = {
			new Vector2Int(0, 0),   // Center
			new Vector2Int(0, 1),   // North
			new Vector2Int(1, 0),   // East
			new Vector2Int(0, -1),  // South
			new Vector2Int(-1, 0),  // West
		};

		foreach (Vector2Int dir in directions) {
			GridPosition neighbor = new GridPosition(centerPosition.x + dir.x, centerPosition.z + dir.y);
			if (IsValidGridPosition(neighbor)) {
				GridObject gridObject = gridSystem.GetGridObject(neighbor);
				gridObject?.CalculateCoverAroundTile();
			}
		}
	}

	public void SetTileVisual(GridPosition pos, GridTile tile) {
    	gridTileArray[pos.x, pos.z] = tile;
	}

	public GridTile GetTileVisual(GridPosition pos) {
		return gridTileArray[pos.x, pos.z];
	}
	
	public void AddUnitAtPosition(GridPosition gridPosition, Unit unit) {
		GridObject gridObject = gridSystem.GetGridObject(gridPosition);
		gridObject?.AddUnit(unit);
	}

	public void RemoveUnitAtPosition(GridPosition gridPosition, Unit unit) {
		GridObject gridObject = gridSystem.GetGridObject(gridPosition);
		gridObject?.RemoveUnit(unit);
	}

	public Unit GetUnitAtGridPosition(GridPosition gridPosition) {
		GridObject gridObject = gridSystem.GetGridObject(gridPosition);
		return gridObject.GetUnit();
	}

	public GridObject GetGridObjectAtGridPosition(GridPosition gridPosition) {
		return gridSystem.GetGridObject(gridPosition);
	}

	public void UnitMovedPosition(Unit unit, GridPosition startingPosition, GridPosition destinationPosition) {
		RemoveUnitAtPosition(startingPosition, unit);
		AddUnitAtPosition(destinationPosition, unit);

		/*if (CoverSystem.HasCover(destinationPosition)) {
			Debug.Log($"Unit {unit.name} moved into a cover-protected tile: {destinationPosition}");
		}*/

		OnAnyUnitMove?.Invoke(this, new OnMoveEventArgs {
			gridPosition = destinationPosition
		});
	}
	
	public bool IsGridPositionOccupied(GridPosition gridPosition) {
		GridObject gridObject = gridSystem.GetGridObject(gridPosition);
		return gridObject != null && gridObject.IsOccupied();
	}
	
	public GridPosition GetGridPosition(Vector3 worldPosition) => gridSystem.GetGridPosition(worldPosition);
	public Vector3 GetWorldPosition(GridPosition gridPosition) => gridSystem.GetWorldPosition(gridPosition);
	public bool IsValidGridPosition(GridPosition gridPosition) => gridSystem.IsValidGridPosition(gridPosition);

	/*private void OnDrawGizmos() {
        if (cameraRegions == null || cameraRegions.Length == 0) return;

		foreach (CameraBoundsRegion region in cameraRegions) {
			float minX = region.startGridX + minEdgePadding;
            float minZ = region.startGridZ + minEdgePadding;

            float maxX = region.startGridX + (region.widthInCells * cellSize) - maxEdgePadding;
            float maxZ = region.startGridZ + (region.heightInCells * cellSize) - maxEdgePadding;

            float rectWidth = maxX - minX;
            float rectLength = maxZ - minZ;

            Vector3 center = new Vector3(minX + (rectWidth / 2f), 0f, minZ + (rectLength / 2f));
            Vector3 size = new Vector3(rectWidth, 1f, rectLength);

            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(center, size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(center, size);
        }
    }*/
}