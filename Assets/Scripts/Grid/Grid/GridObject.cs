using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GridObject {
	public GridPosition GridPosition => gridPosition;

	private readonly GridSystem<GridObject> gridSystem;
    private readonly GridPosition gridPosition;
    private readonly List<Unit> unitList;
	private Dictionary<Vector2Int, CoverObject.CoverType> coverType;

	private bool isWalkable = true;
	
	public GridObject(GridSystem<GridObject> gridSystem, GridPosition gridPosition) {
		this.gridSystem = gridSystem;
		this.gridPosition = gridPosition;
		unitList = new List<Unit>();
		coverType = new Dictionary<Vector2Int, CoverObject.CoverType>();
	}
	
	public void AddUnit(Unit unit) {
		unitList.Add(unit);
	}

	public void RemoveUnit(Unit unit) {
		unitList.Remove(unit);
	}
	
	public List<Unit> GetUnitList() {
		return unitList;
	}

	public bool IsOccupied() {
		return unitList.Count > 0;
	}

	public Unit GetUnit() {
		return unitList.FirstOrDefault();
	}

	public void SetIsWalkable(bool isWalkable) {
        this.isWalkable = isWalkable;
    }

    public bool IsWalkable() {
        return isWalkable;
    }

	public void CalculateCoverAroundTile() {
		Vector3 origin = LevelGrid.Instance.GetWorldPosition(gridPosition) + Vector3.up;

		Vector2Int[] cardinalDirections = {
			new Vector2Int(0, 1),    // North
			new Vector2Int(0, -1),   // South
			new Vector2Int(1, 0),    // East
			new Vector2Int(-1, 0)    // West
		};

		foreach (Vector2Int direction in cardinalDirections) {
			coverType[direction] = CoverObject.CoverType.None;
			Vector3 vector3direction = new Vector3(direction.x, 0, direction.y);
			float rayLength = 1.5f;

			if (Physics.Raycast(origin, vector3direction, out RaycastHit hit, rayLength)) {
				CoverObject cover = hit.collider.GetComponentInParent<CoverObject>();
				if (cover != null) {
                	coverType[direction] = cover.CoverTypeProperty;
            	}
			}
		}
		UpdateTileUI();
	}

	public CoverObject.CoverType GetCoverInDirection(Vector2Int direction) {
        if (coverType.TryGetValue(direction, out var cover)) return cover;
        return CoverObject.CoverType.None;
    }

	private void UpdateTileUI() {
        GridTile tile = LevelGrid.Instance.GetTileVisual(gridPosition);
		if (tile == null) return;

		GridTileUI gridTileUI = tile.GetComponentInChildren<GridTileUI>();
		if (gridTileUI == null) return;

		foreach (var pair in coverType) {
            Vector2Int direction = pair.Key;
			CoverObject.CoverType type = pair.Value;
			CoverObject.CoverDirection coverDirection = CalculateCoverDirection(direction);
			gridTileUI.SetCover(coverDirection, type);
        }
    }

	private CoverObject.CoverDirection CalculateCoverDirection(Vector2Int direction) {
		if (direction == new Vector2Int(0, 1)) return CoverObject.CoverDirection.North;
		if (direction == new Vector2Int(0, -1)) return CoverObject.CoverDirection.South;
		if (direction == new Vector2Int(1, 0)) return CoverObject.CoverDirection.East;
		if (direction == new Vector2Int(-1, 0)) return CoverObject.CoverDirection.West;
		throw new System.Exception("Invalid direction.");
	}

	public void SetCoverManual(Vector2Int direction, CoverObject.CoverType type) {
    	coverType[direction] = type;
    	UpdateTileUI();
	}

	public override string ToString() {
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(gridPosition.ToString());
		foreach (Unit unit in unitList) {
			stringBuilder.AppendLine(unit.ToString());
		}
		return stringBuilder.ToString();
	}
}