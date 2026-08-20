using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField, Min(1)]
    private int maxHealth = 10;

    [SerializeField, Min(0f)]
    private float invincibilityDuration = 0.75f;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    private int currentHealth;
    private float nextDamageAllowedTime;
    private bool isDead;

    private Rigidbody2D body;
    private PlayerController playerController;
    private WeaponController weaponController;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        weaponController = GetComponent<WeaponController>();

        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (
            isDead ||
            damage <= 0 ||
            Time.time < nextDamageAllowedTime
        )
        {
            return;
        }

        nextDamageAllowedTime =
            Time.time + invincibilityDuration;

        currentHealth = Mathf.Max(currentHealth - damage, 0);

        Debug.Log(
            $"Player HP: {currentHealth}/{maxHealth}",
            this
        );

        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void IncreaseMaxHealth(int amount)
    {
        if (isDead || amount <= 0)
        {
            return;
        }

        maxHealth += amount;
        currentHealth += amount;

        HealthChanged?.Invoke(currentHealth, maxHealth);
    }



    private void Die()
    {
        isDead = true;

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (weaponController != null)
        {
            weaponController.enabled = false;
        }

        body.linearVelocity = Vector2.zero;

        Debug.Log("Player died.", this);
        Died?.Invoke();
    }
}