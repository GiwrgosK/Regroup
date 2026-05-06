using UnityEngine;
using System;

public class GridSystem<TGridObject> where TGridObject : class {
	public int Width { get; }
	public int Height { get; }
	public float CellSize { get; }

	private readonly TGridObject[,] gridObjectArray;
	
	public GridSystem(int width, int height, float cellSize, Func<GridSystem<TGridObject>, GridPosition, TGridObject> createGridObject, LayerMask voidLayerMask) {
		Width = width;
		Height = height;
		CellSize = cellSize;
		gridObjectArray = new TGridObject[width, height];

		for (int i = 0; i < Width; i++) {
			for (int j = 0; j < Height; j++) {
				GridPosition gridPosition = new GridPosition(i, j);
				Vector3 worldPosition = GetWorldPosition(gridPosition);
				
				if (Physics.CheckSphere(worldPosition, 0.1f, voidLayerMask)) {
					gridObjectArray[i, j] = null;
                    continue;
				}

				TGridObject gridObject = createGridObject(this, gridPosition);
				gridObjectArray[i, j] = gridObject;
			}
		}
	}

	public Vector3 GetWorldPosition(GridPosition gridPosition) {
		return new Vector3(gridPosition.x, 0, gridPosition.z) * CellSize;
	}

	public GridPosition GetGridPosition(Vector3 worldPosition) {
		return new GridPosition(Mathf.RoundToInt(worldPosition.x / CellSize), Mathf.RoundToInt(worldPosition.z / CellSize));
	}

	public void CreateDebugObjects(Transform debugPrefab) {
		for (int i = 0; i < Width; i++) {
			for (int j = 0; j < Height; j++) {
				GridPosition gridPosition = new GridPosition(i, j);
				TGridObject gridObject = GetGridObject(gridPosition);

				if (gridObject != null) {
					Transform debugTransform = GameObject.Instantiate(debugPrefab, GetWorldPosition(gridPosition), Quaternion.identity);
					GridDebugObject gridDebugObject = debugTransform.GetComponent<GridDebugObject>();
					gridDebugObject.GridObject = GetGridObject(gridPosition);	
				}
			}
		}
	}

	public TGridObject GetGridObject(GridPosition gridPosition) {
		if (gridPosition.x >= 0 && gridPosition.z >= 0 && gridPosition.x < Width && gridPosition.z < Height) {
        	return gridObjectArray[gridPosition.x, gridPosition.z];
    	}

		return null;
	}

	public bool IsValidGridPosition(GridPosition gridPosition) {
		bool isWithinBounds = gridPosition.x >= 0 && gridPosition.z >= 0 && gridPosition.x < Width && gridPosition.z < Height;
        if (!isWithinBounds) return false;
		return gridObjectArray[gridPosition.x, gridPosition.z] != null;
	}
}