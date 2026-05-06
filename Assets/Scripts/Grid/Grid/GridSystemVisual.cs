using UnityEngine;
using System;
using System.Collections.Generic;

public class GridSystemVisual : MonoBehaviour {
	//public static GridSystemVisual Instance { get; private set; }

	private enum GridTileVisualColor {
		White,
		Blue,
		Red,
		Yellow,
		LightRed,
		Black
	}

	[Serializable] private struct GridTileVisualStruct {
		public GridTileVisualColor gridTileVisual;
		public Material color;
	}
	
	[Header("Grid System Visual Configuration")]
	[SerializeField] private Transform gridTilePrefab;
	[SerializeField] private List<GridTileVisualStruct> gridTileVisuals;
	
	private GridTile[,] gridTileArray;
	private Dictionary<GridTileVisualColor, Material> colorLookup;

	private int width;
	private int height;
	
	private void Awake() {
		/*if (Instance != null) {
			Destroy(gameObject);
			return;
		}*/
		//Instance = this;

		colorLookup = new Dictionary<GridTileVisualColor, Material>();
		foreach (var item in gridTileVisuals) {
			colorLookup[item.gridTileVisual] = item.color;
		}
	}

	private void Start() {
		width = LevelGrid.Instance.Width;
		height = LevelGrid.Instance.Height;
		gridTileArray = new GridTile[width, height];
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				GridPosition gridPosition = new GridPosition(i, j);
				if (LevelGrid.Instance.GetGridObjectAtGridPosition(gridPosition) != null) {
					Transform gridTiles = Instantiate(gridTilePrefab, LevelGrid.Instance.GetWorldPosition(gridPosition), Quaternion.identity);
					gridTileArray[i, j] = gridTiles.GetComponent<GridTile>();
					LevelGrid.Instance.SetTileVisual(gridPosition, gridTileArray[i, j]);
				}
			}
		}

		UnitHandler.Instance.OnSelectedActionChanged += UnitHandler_OnSelectedActionChanged;
		LevelGrid.Instance.OnAnyUnitMove += LevelGrid_OnAnyUnitMove;
		DestructibleObject.OnAnyDestroyed += DestructibleObject_OnAnyDestroyed;
		if (UnitHandler.Instance.GetSelectedUnit() != null) {
			UpdateGridVisual(UnitHandler.Instance.GetSelectedUnit(), UnitHandler.Instance.GetSelectedAction());
		}
	}

	private void OnDestroy() {
		UnitHandler.Instance.OnSelectedActionChanged -= UnitHandler_OnSelectedActionChanged;
    	LevelGrid.Instance.OnAnyUnitMove -= LevelGrid_OnAnyUnitMove;
    	DestructibleObject.OnAnyDestroyed -= DestructibleObject_OnAnyDestroyed;
	}

	private void UpdateGridVisual(Unit selectedUnit, BaseAction selectedAction) {
		HideAllTiles();
		if (selectedUnit == null || selectedAction == null) return;

		GridTileVisualColor color;
		switch (selectedAction) {
			default:
			case MoveAction:
				color = GridTileVisualColor.White;
				break;
			case ShootAction shootAction:
				color = GridTileVisualColor.Red;
				ShowTileRange(selectedUnit.UnitGridPosition, ((IAction)shootAction).Range, GridTileVisualColor.LightRed);
				break;
			case GrenadeAction:
				color = GridTileVisualColor.Red;
				break;
			case MeleeAction meleeAction:
				color = GridTileVisualColor.Red;
				ShowTileRangeSquare(selectedUnit.UnitGridPosition, ((IAction)meleeAction).Range, GridTileVisualColor.LightRed);
				break;
			case AmbushAction:
				color = GridTileVisualColor.Red;
				break;
			case SuppressAction suppressAction:
				color = GridTileVisualColor.Red;
				ShowTileRange(selectedUnit.UnitGridPosition, ((IAction)suppressAction).Range, GridTileVisualColor.LightRed);
				break;

		}
		ShowAllTiles(selectedAction.GetValidActionGridPositionList(), color);
	}

	private void HideAllTiles() {
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				if (gridTileArray[i, j] != null) {
					gridTileArray[i, j].IsVisible = false;
				}
			}
		}	
	}
	
	private void ShowAllTiles(List<GridPosition> gridPositionList, GridTileVisualColor color) {
		foreach (GridPosition gridPosition in gridPositionList) {
			gridTileArray[gridPosition.x, gridPosition.z].TileMaterial = GetGridTileVisualMaterial(color);
		}
	}

	private void ShowTileRange(GridPosition gridPosition, int range, GridTileVisualColor color) {
		List<GridPosition> gridPositionList = new List<GridPosition>();
		for (int i = -range; i <= range; i++) {
			for (int j = -range; j <= range; j++) {
				GridPosition testGridPosition = gridPosition + new GridPosition(i, j);
				if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) continue;
				int testDistance = Mathf.Abs(i) + Mathf.Abs(j);
				if (testDistance > range) {
					continue;
				}
				gridPositionList.Add(testGridPosition);
			}
		}
		ShowAllTiles(gridPositionList, color);
	}

	private void ShowTileRangeSquare(GridPosition gridPosition, int range, GridTileVisualColor color) {
		List<GridPosition> gridPositionList = new List<GridPosition>();
		for (int i = -range; i <= range; i++) {
			for (int j = -range; j <= range; j++) {
				GridPosition testGridPosition = gridPosition + new GridPosition(i, j);
				if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) {
					continue;
				}
				gridPositionList.Add(testGridPosition);
			}
		}
		ShowAllTiles(gridPositionList, color);
	}

	private Material GetGridTileVisualMaterial(GridTileVisualColor targetColor) {
		if (colorLookup.TryGetValue(targetColor, out Material material)) {
			return material;
		}
		Debug.LogError("Could not find the GridTileVisualColor: " + targetColor);
		return null;
	}

	private void UnitHandler_OnSelectedActionChanged() {
		UpdateGridVisual(UnitHandler.Instance.GetSelectedUnit(), UnitHandler.Instance.GetSelectedAction());
	}

	private void LevelGrid_OnAnyUnitMove(object sender, EventArgs e) {
		UpdateGridVisual(UnitHandler.Instance.GetSelectedUnit(), UnitHandler.Instance.GetSelectedAction());
	}

	private void DestructibleObject_OnAnyDestroyed(DestructibleObject _) {
		UpdateGridVisual(UnitHandler.Instance.GetSelectedUnit(), UnitHandler.Instance.GetSelectedAction());
	}
}