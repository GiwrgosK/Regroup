using UnityEngine;
using System;

public class HealthHandler : MonoBehaviour {
    public event Action<OnDeadEventArgs> OnDead;
    public event Action OnDamage;

    public class OnDeadEventArgs : EventArgs {
       public Vector3 sourcePosition;
       public string sourceType;
    }

    [Header("Health Handler Death Sound Effect")]
    [SerializeField] private AudioClip deathSoundEffect;

    private int maxHealth;
    private int health;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => health;
    public bool IsDead => health == 0;

    public void Initialize(int startingHealth, int maxHealth) {
        this.maxHealth = maxHealth;
        health = Mathf.Clamp(startingHealth, 0, maxHealth);
        OnDamage?.Invoke();
    }

    public void Damage(int amount, Vector3 sourcePosition, string sourceType) {
        health -= amount;
        if (health < 0) {
            health = 0;
        }
        OnDamage?.Invoke();
        if (IsDead) {
            Die(sourcePosition, sourceType);
            AudioManager.Instance.PlayClip(deathSoundEffect);
        }
    }

    private void Die(Vector3 sourcePosition, string sourceType) {
        OnDead?.Invoke(new OnDeadEventArgs{
            sourcePosition = sourcePosition,
            sourceType = sourceType
        });
    }

    public void SetHealth(int amount) {
        health += amount;
        if (health > maxHealth) {
            health = maxHealth;
        }
        OnDamage?.Invoke();
    }

    public float GetHealthNormalized() {
        if (maxHealth == 0) return 0f;
        return (float) health / maxHealth;
    }
}