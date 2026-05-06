using UnityEngine;

public class MouseFollowIndicator : MonoBehaviour {
    private void Update() {
        Vector3 mouseWorldPosition = LevelGrid.Instance.GetWorldPosition(LevelGrid.Instance.GetGridPosition(MouseHandler.GetPosition()));
        transform.position = mouseWorldPosition;
    }
}