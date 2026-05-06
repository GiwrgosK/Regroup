using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class UnitHandler : MonoBehaviour {
	public static UnitHandler Instance { get; private set; }

	public event Action<bool> OnActiveChanged;
	public event Action OnSelectedUnitChanged;
	public event Action OnSelectedActionChanged;
	public event Action OnActionStarted;

	[Header("Unit Handler Configuration")]
	[SerializeField] private LayerMask unitLayerMask;
	
	private BaseAction selectedAction;
	private Unit selectedUnit;
	private bool isActive;
	
	private void Awake() {
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	private void Start() {
		Unit.OnAnyUnitDeath += Unit_OnAnyUnitDeath;
	}

	private void Update() {
		if (isActive || EventSystem.current.IsPointerOverGameObject()) return;
		if (!TurnHandler.Instance.IsPlayersTurn) return;
		if (TryHandleUnitSelection()) return;
		HandleSelectedAction();
	}

	private void OnDestroy() {
        Unit.OnAnyUnitDeath -= Unit_OnAnyUnitDeath;
    }

	private void HandleSelectedAction() {
		if (selectedUnit == null || selectedAction == null) return;

		if (InputManager.Instance.IsLeftMouseButtonPressed()) {
			GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseHandler.GetPosition());
			
			if (selectedAction.IsValidActionGridPosition(mouseGridPosition)) {
				if (selectedUnit.PlayAction(selectedAction)) {
					SetActive();
					selectedAction.TakeAction(mouseGridPosition, ClearActive);
					OnActionStarted?.Invoke();
				}
			}
		}
	}
	
	private bool TryHandleUnitSelection() {
		if (InputManager.Instance.IsLeftMouseButtonPressed()) {
			Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMousePosition());
			if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask)) {
				if (raycastHit.transform.TryGetComponent(out Unit unit)) {
					if (selectedUnit == unit) {
						return false;
					}
					if (unit.IsEnemy) {
						return false;
					}
					SetSelectedUnit(unit);
					return true;
				}
			}
		}
		return false;
	}

	public void SetSelectedUnit(Unit unit) {
		selectedUnit = unit;
		
		if (unit != null) {
            SetSelectedAction(unit.GetAction<MoveAction>());
        } else {
            SetSelectedAction(null);
        }

		OnSelectedUnitChanged?.Invoke();
	}
	
	public void SetSelectedAction(BaseAction baseAction) {
		selectedAction?.OnDeselected();
		selectedAction = baseAction;
		OnSelectedActionChanged?.Invoke();
		selectedAction?.OnSelected();
	}
	
	public Unit GetSelectedUnit() {
		return selectedUnit;
	}
	
	public BaseAction GetSelectedAction() {
		return selectedAction;
	}
	
	private void SetActive() {
		isActive = true;
		OnActiveChanged?.Invoke(isActive);
	}
	
	private void ClearActive() {
		isActive = false;
		OnActiveChanged?.Invoke(isActive);
	}

	private void Unit_OnAnyUnitDeath(Unit deadUnit) {
        if (selectedUnit == deadUnit) {
            SetSelectedUnit(null);
        }
    }
}