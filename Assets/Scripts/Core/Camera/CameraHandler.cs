using UnityEngine;
using Unity.Cinemachine;

public class CameraHandler : MonoBehaviour {
	[Header("Camera Handler Configuration")]
	[SerializeField] private CinemachineCamera cinemachineCamera;

	private const float MIN_ZOOM = 10f;
	private const float MAX_ZOOM = 20f;

	private CinemachineFollow cinemachineFollow;
	private Vector3 targetFollowOffset;
	private Rect[] cameraBoundsRegions;
    private bool boundsInitialized = false;

	private void Start() {
		cinemachineFollow = cinemachineCamera.GetComponent<CinemachineFollow>();
		targetFollowOffset = cinemachineFollow.FollowOffset;
	}

	private void Update() {
		if (!boundsInitialized) return;
		HandleCameraMovement();
		HandleCameraRotation();
		HandleCameraZoom();
	}

	public void SetCameraBounds(Rect[] regions) {
        cameraBoundsRegions = regions;
        boundsInitialized = true;
    }

	private void HandleCameraMovement() {
		Vector2 inputMoveDir = InputManager.Instance.GetCameraMovement();
		float baseMoveSpeed = 10f;
		float speedMultiplier = InputManager.Instance.IsShiftPressed() ? 3f : 1f;
		float moveSpeed = baseMoveSpeed * speedMultiplier;
		
		Vector3 moveVector = transform.forward * inputMoveDir.y + transform.right * inputMoveDir.x;
		Vector3 restrictedPosition = transform.position + moveSpeed * Time.deltaTime * moveVector;

		transform.position = GetClosestValidPosition(restrictedPosition);
	}

	private void HandleCameraRotation() {
		Vector3 rotationVector = new Vector3(0, 0, 0);
		rotationVector.y = InputManager.Instance.GetCameraRotation();
		float rotationSpeed = 100f;
		transform.eulerAngles += rotationSpeed * Time.deltaTime * rotationVector;
	}

	private void HandleCameraZoom() {
		float zoomIncreaseAmount = 1f;
		float zoomSpeed = 5f;
		targetFollowOffset.y += InputManager.Instance.GetCameraZoom() * zoomIncreaseAmount;
		targetFollowOffset.y = Mathf.Clamp(targetFollowOffset.y, MIN_ZOOM, MAX_ZOOM);
		cinemachineFollow.FollowOffset = Vector3.Lerp(cinemachineFollow.FollowOffset, targetFollowOffset, Time.deltaTime * zoomSpeed);
	}

	private Vector3 GetClosestValidPosition(Vector3 restrictedPosition) {
		Vector3 finalCameraPosition = restrictedPosition;
		float shortestDistance = float.MaxValue;

		foreach (Rect rect in cameraBoundsRegions) {
            float clampedX = Mathf.Clamp(restrictedPosition.x, rect.xMin, rect.xMax);
            float clampedZ = Mathf.Clamp(restrictedPosition.z, rect.yMin, rect.yMax);
            
            Vector3 clampedPos = new Vector3(clampedX, restrictedPosition.y, clampedZ);
            
            float distanceSq = (restrictedPosition - clampedPos).sqrMagnitude;
            if (distanceSq < shortestDistance) {
                shortestDistance = distanceSq;
                finalCameraPosition = clampedPos;
            }
        }
		return finalCameraPosition;
	}
}