using UnityEngine;
using Unity.Cinemachine;

public class ScreenShake : MonoBehaviour {
    public static ScreenShake Instance { get; private set; }

    [Header("ScreenShake Cinemachine Configuration")]
    [SerializeField] private CinemachineImpulseSource cinemachineImpulseSource;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Shake(float intensity) {
        cinemachineImpulseSource.GenerateImpulse(intensity);
    }
}