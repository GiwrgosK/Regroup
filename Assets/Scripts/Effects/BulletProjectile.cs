using UnityEngine;

public class BulletProjectile : MonoBehaviour {
    [Header("Bullet Projectile Configuration")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Transform bulletHitParticles;

    private Vector3 targetPosition;

    private void Update() {
        Vector3 moveDirection  = (targetPosition - transform.position).normalized;
        float distanceBeforeMoving = Vector3.Distance(transform.position, targetPosition);

        float moveSpeed = 200f;
        transform.position += moveSpeed * Time.deltaTime * moveDirection;
        float distanceAfterMoving = Vector3.Distance(transform.position, targetPosition);

        if (distanceBeforeMoving < distanceAfterMoving) {
            transform.position = targetPosition;
            trailRenderer.transform.parent = null;
            Destroy(gameObject);
            Instantiate(bulletHitParticles, targetPosition, Quaternion.identity);
        }
    }

    public void Setup(Vector3 targetPosition) {
        this.targetPosition = targetPosition;
    }
}