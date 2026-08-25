using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField, Min(1)]
    private int maxHealth = 3;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    [SerializeField]
    private XPOrb xpOrbPrefab;

    private int currentHealth;
    private bool isDead;

    [Header("Gold Drops")]
    [SerializeField] private GoldCoin goldCoinPrefab;
    [SerializeField, Range(0f, 1f)] private float goldDropChance = 0.35f;
    [SerializeField, Min(1)] private int baseGoldValue = 1;

    [Header("Enemy Classification")]
    [SerializeField] private bool isBoss;
    public bool IsBoss => isBoss;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
        {
            return;
        }

        currentHealth =
            Mathf.Max(currentHealth - damage, 0);

        if (currentHealth == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        Vector3 spawnCenter = transform.position;
        float offsetDistance = 0.4f; 


        if (xpOrbPrefab != null)
        {
            Vector3 xpPosition = spawnCenter + (Vector3.left * offsetDistance);
            Instantiate(xpOrbPrefab, xpPosition, Quaternion.identity);
        }

        if (goldCoinPrefab != null && Random.value <= goldDropChance)
        {
            Vector3 coinPosition = spawnCenter + (Vector3.right * offsetDistance);
            GoldCoin coin = Instantiate(goldCoinPrefab, coinPosition, Quaternion.identity);

            float goldMultiplier = LocalSaveSystem.GetStageGoldMultiplier(StageRunContext.SelectedStageId);
            coin.Initialize(baseGoldValue, goldMultiplier);
        }

        Destroy(gameObject);
    }
}