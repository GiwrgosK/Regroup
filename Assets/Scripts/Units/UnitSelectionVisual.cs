using UnityEngine;

public class UnitSelectionVisual : MonoBehaviour {
	[Header("Unit Selection Visual Configuration")]
	[SerializeField] private Unit unit;
	[SerializeField] private MeshRenderer meshRenderer;

	private void Start() {
		UnitHandler.Instance.OnSelectedUnitChanged += UnitHandler_OnSelectedUnitChanged;
		UpdateVisual();
	}

	private void OnDestroy() {
    	UnitHandler.Instance.OnSelectedUnitChanged -= UnitHandler_OnSelectedUnitChanged;
    }

	private void UpdateVisual() {
		if (UnitHandler.Instance.GetSelectedUnit() == unit) {
			meshRenderer.enabled = true;
		} else {
			meshRenderer.enabled = false;
		}	
	}

	private void UnitHandler_OnSelectedUnitChanged() {
		UpdateVisual();
	}
}