using UnityEngine;

public class MouseHandler : MonoBehaviour {
	private static MouseHandler Instance;

	[Header("Mouse Handler Configuration")]
	[SerializeField] private LayerMask mousePlaneLayerMask;
	
	private void Awake() {
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}
	
	private void Update() {
		transform.position = GetPosition();
	}
	
	public static Vector3 GetPosition() {
		Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMousePosition());
		Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, Instance.mousePlaneLayerMask);
		return raycastHit.point;
	}
}