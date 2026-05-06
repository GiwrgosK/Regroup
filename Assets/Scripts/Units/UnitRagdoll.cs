using UnityEngine;

public class UnitRagdoll : MonoBehaviour {
    [Header("Unit Ragdoll Configuration")]
    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private GameObject gunPrefab;

    public void Setup(Transform originalRoot, Vector3 sourcePosition, string sourceType) {
        Match(originalRoot, ragdollRoot);
        DropGun();

        float force;
        float range;
        switch (sourceType.ToLower()) {
            case "grenade":
                force = 1000f;
                range = 15f;
                break;
            case "bullet":
                force = 400f;
                range = 5f;
                break;
            case "melee":
                force = 200f;
                range = 5f;
                break;
            default:
                force = 300f;
                range = 10f;
                break;
        }
        ApplyPushOnDeath(ragdollRoot, sourcePosition, force, range);
    }

    private void Match(Transform root, Transform clone) {
        foreach (Transform part in root) {
            Transform clonePart = clone.Find(part.name);
            if (clonePart != null) {
                clonePart.SetPositionAndRotation(part.position, part.rotation);
                Match(part, clonePart);
            }
        }
    }

    private void DropGun() {
        if (gunPrefab != null) {
            gunPrefab.transform.parent = null;
            if (gunPrefab.TryGetComponent(out Rigidbody gunRigidBody)) {
                gunRigidBody.isKinematic = false;
                gunRigidBody.useGravity = true;
            }
        }
    }

    private void ApplyPushOnDeath(Transform root, Vector3 position, float force, float range) {
        foreach (Transform part in root) {
            if (part.TryGetComponent(out Rigidbody partRigidBody)) {
                partRigidBody.AddExplosionForce(force, position, range);
            }
            ApplyPushOnDeath(part, position, force, range);
        }
    }
}