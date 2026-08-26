using System;
using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Experience & Leveling")]
    [SerializeField, Min(1)]
    private int startingExperienceRequirement = 5;

    [SerializeField, Min(1.01f)]
    private float requirementMultiplier = 1.5f;

    public int Level { get; private set; } = 1;
    public int CurrentExperience { get; private set; }
    public int ExperienceToNextLevel { get; private set; }

    public event Action<int, int> ExperienceChanged;
    public event Action<int> LevelChanged;
    public event Action<int> LeveledUp;

    [Header("Base Character Stats")]
    [SerializeField] private float baseHp = 10f;
    [SerializeField] private float baseAtk = 8f;
    [SerializeField] private float baseSpeed = 6f;

    // Buffs & Multipliers
    public int BonusAttack { get; private set; } = 0;
    public float FlatHpBuff { get; private set; } = 0f;
    public float FlatAtkBuff { get; private set; } = 0f;
    public float FlatSpeedBuff { get; private set; } = 0f;

    public float AtkPercentMultiplier { get; private set; } = 0f;
    public float SpeedPercentMultiplier { get; private set; } = 0f;

    // Computed Stat Properties
    public float BaseHp => baseHp;
    public float BaseAtk => baseAtk;
    public float BaseSpeed => baseSpeed;

    public float CurrentMaxHp => baseHp + FlatHpBuff;
    public float CurrentAtk => (baseAtk + FlatAtkBuff + BonusAttack) * (1f + AtkPercentMultiplier);
    public float CurrentSpeed => (baseSpeed + FlatSpeedBuff) * (1f + SpeedPercentMultiplier);

    public event Action OnStatsChanged;
    public event Action<int> BonusAttackChanged;

    private Coroutine attackBoostRoutine;
    private Coroutine atkPotionRoutine;
    private Coroutine speedPotionRoutine;

    private void Awake()
    {
        ExperienceToNextLevel = startingExperienceRequirement;
    }

    // --- Experience & Leveling ---
    public void AddExperience(int amount)
    {
        if (amount <= 0) return;

        CurrentExperience += amount;

        while (CurrentExperience >= ExperienceToNextLevel)
        {
            CurrentExperience -= ExperienceToNextLevel;
            Level++;

            ExperienceToNextLevel = Mathf.CeilToInt(ExperienceToNextLevel * requirementMultiplier);

            Debug.Log($"Player reached level {Level}.", this);

            LevelChanged?.Invoke(Level);
            LeveledUp?.Invoke(Level);
        }

        Debug.Log($"Player XP: {CurrentExperience}/{ExperienceToNextLevel}", this);
        ExperienceChanged?.Invoke(CurrentExperience, ExperienceToNextLevel);
    }

    // --- Temporary Skill Buffs ---
    public void ApplyTemporaryAttackBoost(int amount, float duration)
    {
        if (attackBoostRoutine != null) StopCoroutine(attackBoostRoutine);
        attackBoostRoutine = StartCoroutine(AttackBoostRoutine(amount, duration));
    }

    private IEnumerator AttackBoostRoutine(int amount, float duration)
    {
        BonusAttack += amount;
        BonusAttackChanged?.Invoke(BonusAttack);
        OnStatsChanged?.Invoke();

        yield return new WaitForSeconds(duration);

        BonusAttack = Mathf.Max(0, BonusAttack - amount);
        BonusAttackChanged?.Invoke(BonusAttack);
        OnStatsChanged?.Invoke();
    }

    // --- Potion Multipliers ---
    public void ApplyAttackPotion(float percentBoost = 0.30f, float duration = 150f)
    {
        if (atkPotionRoutine != null) StopCoroutine(atkPotionRoutine);
        atkPotionRoutine = StartCoroutine(AtkPotionRoutine(percentBoost, duration));
    }

    private IEnumerator AtkPotionRoutine(float percentBoost, float duration)
    {
        AtkPercentMultiplier = percentBoost;
        OnStatsChanged?.Invoke();

        yield return new WaitForSeconds(duration);

        AtkPercentMultiplier = 0f;
        OnStatsChanged?.Invoke();
    }

    public void ApplySpeedPotion(float percentBoost = 0.50f, float duration = 150f)
    {
        if (speedPotionRoutine != null) StopCoroutine(speedPotionRoutine);
        speedPotionRoutine = StartCoroutine(SpeedPotionRoutine(percentBoost, duration));
    }

    private IEnumerator SpeedPotionRoutine(float percentBoost, float duration)
    {
        SpeedPercentMultiplier = percentBoost;
        OnStatsChanged?.Invoke();

        yield return new WaitForSeconds(duration);

        SpeedPercentMultiplier = 0f;
        OnStatsChanged?.Invoke();
    }
}