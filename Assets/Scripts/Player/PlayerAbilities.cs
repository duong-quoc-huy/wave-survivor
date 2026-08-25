using System;
using System.Collections;
using UnityEngine;

public enum CharacterClass
{
    Assassin,
    Mage,
    Warrior
}

public class PlayerAbilities : MonoBehaviour
{
    [Header("Class Configuration")]
    [SerializeField] private CharacterClass characterClass;

    [Header("Skill Cooldowns (Seconds)")]
    [SerializeField] private float eCooldown = 8f;
    [SerializeField] private float qCooldown = 15f;

    public float ECooldownTotal => eCooldown;
    public float QCooldownTotal => qCooldown;
    public float ECooldownRemaining => Mathf.Max(0f, nextETime - Time.time);
    public float QCooldownRemaining => Mathf.Max(0f, nextQTime - Time.time);

    private float nextETime;
    private float nextQTime;

    // Hotbar Potions
    public int SpeedPotionCount { get; private set; } = 1; // Default 1 for testing
    public int AttackPotionCount { get; private set; } = 1;

    public event Action OnAbilitiesUpdated;

    private PlayerController playerController;
    private PlayerHealth playerHealth;
    private WeaponController weaponController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerHealth = GetComponent<PlayerHealth>();
        weaponController = GetComponent<WeaponController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && Time.time >= nextETime) UseESkill();
        if (Input.GetKeyDown(KeyCode.Q) && Time.time >= nextQTime) UseQSkill();
        if (Input.GetKeyDown(KeyCode.Alpha1)) UseSpeedPotion();
        if (Input.GetKeyDown(KeyCode.Alpha2)) UseAttackPotion();
    }

    public void UseESkill()
    {
        if (Time.time < nextETime) return;
        nextETime = Time.time + eCooldown;

        switch (characterClass)
        {
            case CharacterClass.Assassin:
                StartCoroutine(SpeedBoostRoutine(3f, 4f));
                break;
            case CharacterClass.Mage:
                StunNearbyEnemies(5f);
                break;
            case CharacterClass.Warrior:
                Debug.Log("Warrior Defensive Stance Activated!");
                break;
        }
        OnAbilitiesUpdated?.Invoke();
    }

    public void UseQSkill()
    {
        if (Time.time < nextQTime) return;
        nextQTime = Time.time + qCooldown;

        // Trigger Burst Damage across scene
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (enemy.IsBoss)
            {
                int bossDamage = Mathf.CeilToInt(enemy.GetComponent<EnemyHealth>().MaxHealth * 0.10f);
                enemy.TakeDamage(Mathf.Max(1, bossDamage));
            }
            else
            {
                enemy.TakeDamage(9999); // Screen wipe standard mobs
            }
        }
        OnAbilitiesUpdated?.Invoke();
    }

    public void UseSpeedPotion()
    {
        if (SpeedPotionCount <= 0) return;
        SpeedPotionCount--;
        StartCoroutine(SpeedBoostRoutine(2f, 8f));
        OnAbilitiesUpdated?.Invoke();
    }

    public void UseAttackPotion()
    {
        if (AttackPotionCount <= 0) return;
        AttackPotionCount--;
        if (weaponController != null) weaponController.IncreaseProjectileDamage(5);
        OnAbilitiesUpdated?.Invoke();
    }

    private IEnumerator SpeedBoostRoutine(float amount, float duration)
    {
        if (playerController != null)
        {
            playerController.IncreaseMoveSpeed(amount);
            yield return new WaitForSeconds(duration);
            playerController.IncreaseMoveSpeed(-amount);
        }
    }

    private void StunNearbyEnemies(float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
        }
    }
}