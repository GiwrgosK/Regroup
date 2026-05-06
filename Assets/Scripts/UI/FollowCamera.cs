using UnityEngine;

public class FollowCamera : MonoBehaviour {
    [Header("Follow Camera Invert Boolean")]
    [SerializeField] private bool invert;

    private Transform cameraTransform;
    
    private void Awake() {
        cameraTransform = Camera.main.transform;
    }

    private void LateUpdate() {
        if (invert) {
            Vector3 directionOfCamera = (cameraTransform.position - transform.position).normalized;
            transform.LookAt(transform.position - directionOfCamera);
        } else {
            transform.LookAt(cameraTransform);
        }
    }
}