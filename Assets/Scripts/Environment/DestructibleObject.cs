using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class DestructibleObject : MonoBehaviour {
    public static event Action<DestructibleObject> OnAnyDestroyed;

    [Header("Destructible Object Configuration")]
    [SerializeField] private Transform destroyedBarrelPrefab;
    [SerializeField] private Transform explosionSpecialEffect;
    [SerializeField] private LayerMask targetLayerMask;
    
    [Header("Destructible Object Visual & Audio Configuration")]
    [SerializeField] private GameObject countdownTimerBar;
    [SerializeField] private Image countdownTimerImage;
    [SerializeField] private AudioClip explosionSoundEffect;

    private GridPosition gridPosition;
    private readonly float explosionRadius = 4f;
    private readonly int explosionDamage = 30;
    private readonly float delay = 2.6f;
    private bool isDestroyed = false;

    private void Start() {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        countdownTimerBar.SetActive(false);
    }

    public GridPosition GetGridPosition() {
        return gridPosition;
    }

    public void Damage() {
        if (isDestroyed) return;
        isDestroyed = true;
        countdownTimerBar.SetActive(true);
        StartCoroutine(ExplosionRoutine(delay));
    }

    private void Explode() {
        Collider[] colliderArray = Physics.OverlapSphere(transform.position, explosionRadius, targetLayerMask);
        foreach (Collider collider in colliderArray) {
            if (collider.TryGetComponent(out Unit unit)) {
                unit.Damage(explosionDamage, unit.GetWorldPosition(), "grenade");
            }

            if (collider.TryGetComponent(out DestructibleObject otherBarrel)) {
                otherBarrel.Damage(); 
            }
        }
    }

    private void ApplyPushOnDestruction(Transform root, Vector3 position, float force, float range) {
        foreach (Transform part in root) {
            if (part.TryGetComponent(out Rigidbody partRigitBody)) {
                partRigitBody.AddExplosionForce(force, position, range);
            }
            ApplyPushOnDestruction(part, position, force, range);
        }
    }

    private IEnumerator ExplosionRoutine(float delay) {
        AudioManager.Instance.PlayClip(explosionSoundEffect);

        float timer = delay;
        countdownTimerImage.fillAmount = 1f;

        while (timer > 0f) {
            timer -= Time.deltaTime;

            if (countdownTimerImage != null) {
                countdownTimerImage.fillAmount = timer / delay;
            }

            yield return null;
        }

        Explode();

        GetComponent<Collider>().enabled = false;
        Transform destroyedCrate = Instantiate(destroyedBarrelPrefab, transform.position, transform.rotation);
        Instantiate(explosionSpecialEffect, transform.position, Quaternion.identity);
        ApplyPushOnDestruction(destroyedCrate, transform.position, 150f, 10f);
        LevelGrid.Instance.RecalculateCoverAround(gridPosition);

        Destroy(gameObject);
        OnAnyDestroyed?.Invoke(this);
    }
}