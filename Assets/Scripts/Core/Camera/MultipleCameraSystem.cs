using UnityEngine;

public class MultipleCameraSystem : MonoBehaviour {
    [Header("Multiple Camera System Configuration")]
    [SerializeField] private GameObject actionCamera;

    private void Start() {
        BaseAction.OnAnyActionStart += BaseAction_OnAnyActionStart;
        BaseAction.OnAnyActionEnd += BaseAction_OnAnyActionEnd;
        HideActionCamera();
    }

    private void OnDestroy() {
        BaseAction.OnAnyActionStart -= BaseAction_OnAnyActionStart;
        BaseAction.OnAnyActionEnd -= BaseAction_OnAnyActionEnd;
    }

    private void ShowActionCamera() {
        actionCamera.SetActive(true);
    }

    private void HideActionCamera() {
        actionCamera.SetActive(false);
    }

    private void BaseAction_OnAnyActionStart(BaseAction baseAction) {
        Unit shooter = null;
        Unit target = null;

        if (baseAction is ShootAction shootAction) {
            shooter = shootAction.Unit;
            target = shootAction.TargetUnit;
        } else if (baseAction is SuppressAction suppressAction) {
            shooter = suppressAction.Unit;
            target = suppressAction.TargetUnit;
        }

        if (shooter != null && target != null) {
            float shoulderOffsetAmount = 0.5f;
            Vector3 unitShoulderHeight = Vector3.up * 1.7f;
            Vector3 shootingDirection = (target.GetWorldPosition() - shooter.GetWorldPosition()).normalized;
            Vector3 shoulderOffset = Quaternion.Euler(0, 90, 0) * shootingDirection * shoulderOffsetAmount;
            Vector3 actionCameraPosition = shooter.GetWorldPosition() + unitShoulderHeight + shoulderOffset + (shootingDirection * -1);
            
            actionCamera.transform.position = actionCameraPosition;
            actionCamera.transform.LookAt(target.GetWorldPosition() + unitShoulderHeight);
            ShowActionCamera();
        }
    }

    private void BaseAction_OnAnyActionEnd(BaseAction baseAction) {
        if (baseAction is ShootAction or SuppressAction) {
            HideActionCamera();
        }
    }
}