using UnityEngine;

public class UnitRagdollSpawner : MonoBehaviour {
    [Header("Unit Ragdoll Spawner Configuration")]
    [SerializeField] private Transform unitRagdollPrefab;
    [SerializeField] private Transform ragdollOriginalRoot;
    [SerializeField] private HealthHandler healthHandler;

    private void Awake() {
        healthHandler.OnDead += HealthHandler_OnDead;
    }

    private void OnDestroy() {
        healthHandler.OnDead -= HealthHandler_OnDead;
    }

    private void HealthHandler_OnDead(HealthHandler.OnDeadEventArgs onDeadEventArgs) {
        Transform ragdollTransform = Instantiate(unitRagdollPrefab, transform.position, transform.rotation);
        UnitRagdoll unitRagdoll = ragdollTransform.GetComponent<UnitRagdoll>();
        unitRagdoll.Setup(ragdollOriginalRoot, onDeadEventArgs.sourcePosition, onDeadEventArgs.sourceType);
    }
}