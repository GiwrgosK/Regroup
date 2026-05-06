using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class UnitHandlerUI : MonoBehaviour {
	[Header("Unit Handler UI Configuration")]
	[SerializeField] private Transform buttonContainerTransform;
	[SerializeField] private Transform actionButtonPrefab;
	[SerializeField] private Transform infoButtonPrefab;
	[SerializeField] private TextMeshProUGUI actionPointsText;
	
	private List<ButtonUI> buttonList;
	
	private void Awake() {
		buttonList = new List<ButtonUI>();
	}

	private void Start() {
		UnitHandler.Instance.OnSelectedUnitChanged += UnitHandler_OnSelectedUnitChanged;
		UnitHandler.Instance.OnSelectedActionChanged += UnitHandler_OnSelectedActionChanged;
		UnitHandler.Instance.OnActionStarted += UnitHandler_OnActionStarted;
		TurnHandler.Instance.OnTurnChanged += TurnHandler_OnTurnChanged;
		Unit.OnAnyActionPointChange += Unit_OnAnyActionPointChange;
		BaseAction.OnAnyActionEnd += BaseAction_OnAnyActionEnd;

		if (UnitHandler.Instance.GetSelectedUnit() != null) {
			CreateButtons();
			UpdateActionPoints();
			UpdateSelectedButton();
		}
	}

	private void OnDestroy() {
		UnitHandler.Instance.OnSelectedUnitChanged -= UnitHandler_OnSelectedUnitChanged;
		UnitHandler.Instance.OnSelectedActionChanged -= UnitHandler_OnSelectedActionChanged;
		UnitHandler.Instance.OnActionStarted -= UnitHandler_OnActionStarted;
		TurnHandler.Instance.OnTurnChanged -= TurnHandler_OnTurnChanged;
		Unit.OnAnyActionPointChange -= Unit_OnAnyActionPointChange;
		BaseAction.OnAnyActionEnd -= BaseAction_OnAnyActionEnd;
	}

	private void CreateButtons() {
		foreach (Transform buttonTransform in buttonContainerTransform) {
			Destroy(buttonTransform.gameObject);
		}

		buttonList.Clear();

		Unit selectedUnit = UnitHandler.Instance.GetSelectedUnit();
		if (selectedUnit == null) return;

		Transform infoButtonTransform = Instantiate(infoButtonPrefab, buttonContainerTransform);
        InfoButtonUI infoButtonUI = infoButtonTransform.GetComponent<InfoButtonUI>();
        infoButtonUI.SetUnit(selectedUnit);

		foreach (BaseAction baseAction in selectedUnit.BaseActionArray) {
			if (!baseAction.IsActionAvailable()) continue;

			Transform button = Instantiate(actionButtonPrefab, buttonContainerTransform);
			ButtonUI buttonUI = button.GetComponent<ButtonUI>();
			buttonUI.SetBaseAction(baseAction);
			buttonList.Add(buttonUI);
		}
	}

	private void UpdateActionPoints() {
		Unit selectedUnit = UnitHandler.Instance.GetSelectedUnit();
		if (selectedUnit == null) return;

		actionPointsText.text = "Action Points: " + selectedUnit.ActionPoints;
	}

	private void UpdateSelectedButton() {
		foreach (ButtonUI buttonUI in buttonList) {
			buttonUI.UpdateButtonVisual();
		}
	}

	private void UpdateTurnVisuals() {
		bool isPlayerTurn = TurnHandler.Instance.IsPlayersTurn;
		buttonContainerTransform.gameObject.SetActive(isPlayerTurn);
        actionPointsText.gameObject.SetActive(isPlayerTurn);
	}

	private void UnitHandler_OnSelectedUnitChanged() {
		CreateButtons();
		UpdateSelectedButton();
		UpdateActionPoints();
	}
	
	private void UnitHandler_OnSelectedActionChanged() {
		UpdateSelectedButton();
	}
	
	private void UnitHandler_OnActionStarted() {
		UpdateActionPoints();
	}
	
	private void TurnHandler_OnTurnChanged() {
		UpdateActionPoints();
		UpdateTurnVisuals();
	}
	
	private void Unit_OnAnyActionPointChange() {
		UpdateActionPoints();
	}

	private void BaseAction_OnAnyActionEnd(BaseAction _) {
		Unit selectedUnit = UnitHandler.Instance.GetSelectedUnit();
        if (selectedUnit != null) {
            BaseAction selectedAction = UnitHandler.Instance.GetSelectedAction();
            
            if (selectedAction != null && !selectedAction.IsActionAvailable()) {
                MoveAction moveAction = selectedUnit.GetAction<MoveAction>();
                if (moveAction != null) {
                    UnitHandler.Instance.SetSelectedAction(moveAction);
                }
            }
        }

        CreateButtons();
        UpdateSelectedButton();
	}
}