using UnityEngine;
using System;
using System.Collections;

public class GrenadeProjectile : MonoBehaviour {
    public static event Action OnAnyGrenadeThrown;

    [Header("Grenade Projectile Configuration")]
    [SerializeField] private Transform grenadeExplosionPrefab;
    [SerializeField] private AnimationCurve grenadeCurve;
    [SerializeField] private TrailRenderer trailRenderer;

    private TrailRenderer trailRendererInstance;
    private Vector3 landPosition;
    private Vector3 targetPosition;
    private Vector3 positionXZ;
    private Action OnGrenadeActionComplete;
    private float totalDistance;
    private bool isFlying = true;

    private void Update() {
        if (!isFlying) return;

        Vector3 moveDirection = (landPosition - positionXZ).normalized;
        float moveSpeed = 15f;
        positionXZ += moveSpeed * Time.deltaTime * moveDirection;
        float distance = Vector3.Distance(positionXZ, landPosition);
        float normalizedDistance = 1 - distance / totalDistance;

        float maxHeight = totalDistance / 4f;
        float positionY = grenadeCurve.Evaluate(normalizedDistance) * maxHeight;
        if (float.IsNaN(positionY)) positionY = 0f;
        transform.position = new Vector3(positionXZ.x, positionY, positionXZ.z);

        float reachedTargetDistance = 0.2f;
        if (distance < reachedTargetDistance) {
            isFlying = false;
            transform.position = new Vector3(landPosition.x, 0f, landPosition.z);

            if (trailRendererInstance != null) {
                trailRendererInstance.transform.parent = null;
            }
            TipOverGrenade();
            StartCoroutine(RollToFinalTarget());
        }
    }

    public void Setup(GridPosition targetGridPosition, Action OnGrenadeActionComplete) {
        this.OnGrenadeActionComplete = OnGrenadeActionComplete;

        targetPosition = LevelGrid.Instance.GetWorldPosition(targetGridPosition);

        Vector3 flatStart = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 flatTarget = new Vector3(targetPosition.x, 0f, targetPosition.z);
        Vector3 directionToTarget = (flatTarget - flatStart).normalized;

        float tileSize = 2f;
        landPosition = flatTarget - directionToTarget * tileSize;

        positionXZ = flatStart;

        trailRendererInstance = Instantiate(trailRenderer, transform.position, Quaternion.identity, transform);
        totalDistance = Vector3.Distance(positionXZ, landPosition);

        Debug.DrawLine(flatStart, landPosition, Color.green, 5f);
        Debug.DrawLine(landPosition, targetPosition, Color.yellow, 5f);
    }

    private void TipOverGrenade() {
        Vector3 flatDirection = (targetPosition - landPosition).normalized;
        Quaternion tipRotation = Quaternion.LookRotation(flatDirection);
        tipRotation *= Quaternion.Euler(90f, 0f, 0f);
        transform.rotation = tipRotation;
    }

    private IEnumerator RollToFinalTarget() {
        float rollSpeed = 8f;
        float rollDistance = Vector3.Distance(landPosition, targetPosition);
        float traveled = 0f;

        Vector3 direction = (targetPosition - landPosition).normalized;
        Vector3 rightAxis = Vector3.Cross(Vector3.up, direction);

        float grenadeGroundHeight = 0.09f;

        while (traveled < rollDistance) {
            Vector3 move = direction * rollSpeed * Time.deltaTime;
            Vector3 nextPosition = transform.position + move;
            nextPosition.y = grenadeGroundHeight;
            transform.position = nextPosition;
            traveled += move.magnitude;

            float rollAngle = 360f / (2 * Mathf.PI * 0.05f) * move.magnitude;
            transform.Rotate(rightAxis, rollAngle, Space.World);

            yield return null;
        }

        transform.position = new Vector3(targetPosition.x, grenadeGroundHeight, targetPosition.z);

        yield return new WaitForSeconds(1.2f);

        float grenadeRadius = 4f;
        Collider[] colliderArray = Physics.OverlapSphere(targetPosition, grenadeRadius);

        foreach (Collider collider in colliderArray) {
            if (collider.TryGetComponent(out Unit targetUnit)) {
                targetUnit.Damage(50, transform.position, "grenade");
            }
            if (collider.TryGetComponent(out DestructibleObject destructibleObject)) {
                destructibleObject.Damage();
            }
        }

        OnAnyGrenadeThrown?.Invoke();
        Instantiate(grenadeExplosionPrefab, targetPosition, Quaternion.identity);
        Destroy(gameObject);
        OnGrenadeActionComplete();
    }
}