using System;
using System.Collections;
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
    public bool IsSkill1Active => isSkill1Active;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    private int currentHealth;
    private float nextDamageAllowedTime;
    private bool isDead;

    private bool isSkill1Active;
    private float currentDamageReductionPercent = 0.5f;
    private Coroutine skill1Routine;

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

    public void Heal(int amount)
    {
        if (isDead || amount <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ActivateDamageReduction(float duration, float reductionPercent = 0.5f)
    {
        if (skill1Routine != null) StopCoroutine(skill1Routine);
        skill1Routine = StartCoroutine(DamageReductionRoutine(duration, reductionPercent));
    }

    private IEnumerator DamageReductionRoutine(float duration, float reductionPercent)
    {
        isSkill1Active = true;
        currentDamageReductionPercent = reductionPercent;
        Debug.Log($"[PlayerHealth] Skill 1 Activated: {reductionPercent * 100}% Damage Reduction for {duration}s", this);

        yield return new WaitForSeconds(duration);

        isSkill1Active = false;
        currentDamageReductionPercent = 0f;
        Debug.Log("[PlayerHealth] Skill 1 Expired: Damage Reduction OFF", this);
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0 || Time.time < nextDamageAllowedTime)
        {
            return;
        }

        nextDamageAllowedTime = Time.time + invincibilityDuration;

        int finalDamage = damage;
        if (isSkill1Active)
        {
            float reduced = damage * (1f - currentDamageReductionPercent);
            finalDamage = Mathf.Max(1, Mathf.FloorToInt(reduced));
        }

        currentHealth = Mathf.Max(currentHealth - finalDamage, 0);

        Debug.Log($"Player HP: {currentHealth}/{maxHealth} (Took {finalDamage} dmg, Raw: {damage})", this);

        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void IncreaseMaxHealth(int amount)
    {
        if (isDead || amount <= 0) return;

        maxHealth += amount;
        currentHealth += amount;

        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        isDead = true;

        if (playerController != null) playerController.enabled = false;
        if (weaponController != null) weaponController.enabled = false;

        body.linearVelocity = Vector2.zero;

        Debug.Log("Player died.", this);
        Died?.Invoke();
    }
}