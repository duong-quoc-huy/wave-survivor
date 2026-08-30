using UnityEngine;

[DefaultExecutionOrder(-10000)]
public sealed class AdminConsole : MonoBehaviour
{
    public static AdminConsole Instance { get; private set; }

    [Header("Master Switch")]
    [Tooltip("Cheat values are ignored until this option is enabled.")]
    public bool cheatsEnabled;

    [Header("Player Stats & God Mode")]
    [Min(1)] public int playerBaseHp = 99999;
    [Min(0f)] public float playerBaseAtk = 99999f;
    [Min(0f)] public float playerBaseSpeed = 10f;
    [Min(0f)] public float playerInvincibilityDuration = 9999f;
    [Range(0f, 1f)] public float playerDamageReductionPercent = 1f;

    [Header("XP & Leveling Curve")]
    [Min(1)] public int startingXpRequirement = 1;
    [Min(1f)] public float xpRequirementMultiplier = 1f;
    [Min(0)] public int enemyXpDropValue = 100;

    [Header("Potion Buffs & Durations")]
    [Min(0f)] public float attackPotionMultiplier = 5f;
    [Min(0f)] public float attackPotionDuration = 99999f;
    [Min(0f)] public float speedPotionMultiplier = 3f;
    [Min(0f)] public float speedPotionDuration = 99999f;

    [Header("Enemy Nerfs & Controls")]
    [Min(1)] public int enemyMaxHp = 1;
    [Min(0f)] public float enemyMoveSpeed = 0.5f;
    [Min(0)] public int enemyContactDamage;

    [Header("Economy & Drop Rates")]
    [Range(0f, 1f)] public float goldDropChance = 1f;
    [Min(1)] public int baseGoldValue = 100;
    [Min(0)] public int startingGold = 99999;

    [Tooltip("When enabled, repeat clears never reduce enemy gold rewards.")]
    public bool disableGoldMultiplierDecay = true;

    [Tooltip("Gold multiplier removed per previous clear when decay is enabled. The normal game value is 0.20.")]
    [Range(0f, 1f)] public float goldMultiplierDecay = 0.2f;

    public static bool CheatsEnabled =>
        Instance != null && Instance.cheatsEnabled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ClampValues();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ClampValues();
    }
#endif

    private void ClampValues()
    {
        playerBaseHp = Mathf.Max(1, playerBaseHp);
        playerBaseAtk = Mathf.Max(0f, playerBaseAtk);
        playerBaseSpeed = Mathf.Max(0f, playerBaseSpeed);
        playerInvincibilityDuration = Mathf.Max(0f, playerInvincibilityDuration);
        playerDamageReductionPercent = Mathf.Clamp01(playerDamageReductionPercent);

        startingXpRequirement = Mathf.Max(1, startingXpRequirement);
        xpRequirementMultiplier = Mathf.Max(1f, xpRequirementMultiplier);
        enemyXpDropValue = Mathf.Max(0, enemyXpDropValue);

        attackPotionMultiplier = Mathf.Max(0f, attackPotionMultiplier);
        attackPotionDuration = Mathf.Max(0f, attackPotionDuration);
        speedPotionMultiplier = Mathf.Max(0f, speedPotionMultiplier);
        speedPotionDuration = Mathf.Max(0f, speedPotionDuration);

        enemyMaxHp = Mathf.Max(1, enemyMaxHp);
        enemyMoveSpeed = Mathf.Max(0f, enemyMoveSpeed);
        enemyContactDamage = Mathf.Max(0, enemyContactDamage);

        goldDropChance = Mathf.Clamp01(goldDropChance);
        baseGoldValue = Mathf.Max(1, baseGoldValue);
        startingGold = Mathf.Max(0, startingGold);
        goldMultiplierDecay = Mathf.Clamp01(goldMultiplierDecay);
    }

    public static int ResolvePlayerBaseHp(int fallback) =>
        CheatsEnabled ? Instance.playerBaseHp : fallback;

    public static float ResolvePlayerBaseAtk(float fallback) =>
        CheatsEnabled ? Instance.playerBaseAtk : fallback;

    public static float ResolvePlayerBaseSpeed(float fallback) =>
        CheatsEnabled ? Instance.playerBaseSpeed : fallback;

    public static float ResolvePlayerInvincibilityDuration(float fallback) =>
        CheatsEnabled ? Instance.playerInvincibilityDuration : fallback;

    public static float ResolvePlayerDamageReduction(float fallback) =>
        CheatsEnabled ? Instance.playerDamageReductionPercent : fallback;

    public static int ResolveStartingXpRequirement(int fallback) =>
        CheatsEnabled ? Instance.startingXpRequirement : fallback;

    public static float ResolveXpRequirementMultiplier(float fallback) =>
        CheatsEnabled ? Instance.xpRequirementMultiplier : fallback;

    public static int ResolveEnemyXpDropValue(int fallback) =>
        CheatsEnabled ? Instance.enemyXpDropValue : fallback;

    public static float ResolveAttackPotionMultiplier(float fallback) =>
        CheatsEnabled ? Instance.attackPotionMultiplier : fallback;

    public static float ResolveAttackPotionDuration(float fallback) =>
        CheatsEnabled ? Instance.attackPotionDuration : fallback;

    public static float ResolveSpeedPotionMultiplier(float fallback) =>
        CheatsEnabled ? Instance.speedPotionMultiplier : fallback;

    public static float ResolveSpeedPotionDuration(float fallback) =>
        CheatsEnabled ? Instance.speedPotionDuration : fallback;

    public static int ResolveEnemyMaxHp(int fallback) =>
        CheatsEnabled ? Instance.enemyMaxHp : fallback;

    public static float ResolveEnemyMoveSpeed(float fallback) =>
        CheatsEnabled ? Instance.enemyMoveSpeed : fallback;

    public static int ResolveEnemyContactDamage(int fallback) =>
        CheatsEnabled ? Instance.enemyContactDamage : fallback;

    public static float ResolveGoldDropChance(float fallback) =>
        CheatsEnabled ? Instance.goldDropChance : fallback;

    public static int ResolveBaseGoldValue(int fallback) =>
        CheatsEnabled ? Instance.baseGoldValue : fallback;

    public static int ResolveStartingGold(int fallback) =>
        CheatsEnabled ? Instance.startingGold : fallback;

    public static float ResolveGoldMultiplierDecay(float fallback)
    {
        if (!CheatsEnabled)
            return fallback;

        return Instance.disableGoldMultiplierDecay
            ? 0f
            : Instance.goldMultiplierDecay;
    }
}
