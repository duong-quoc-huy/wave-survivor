using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Legacy Enemy Prefabs")]
    [SerializeField]
    private EnemyHealth ghostPrefab;

    [SerializeField]
    private EnemyHealth spiderPrefab;

    [SerializeField]
    private Transform enemyParent;

    [Header("Legacy Spider Unlock")]
    [SerializeField, Min(0f)]
    private float spiderUnlockTime = 60f;

    [SerializeField, Range(0f, 1f)]
    private float spiderSpawnChance = 0.35f;

    [Header("Arena")]
    [SerializeField]
    private Vector2 arenaHalfSize =
        new Vector2(14f, 9f);

    [Header("Spawn Timing")]
    [SerializeField, Min(0f)]
    private float startDelay = 1f;

    [SerializeField, Min(0.1f)]
    private float initialSpawnInterval = 2f;

    [SerializeField, Min(0.1f)]
    private float minimumSpawnInterval = 0.75f;

    [SerializeField, Min(0f)]
    private float intervalDecreasePerMinute = 0.25f;

    [Header("Limits")]
    [SerializeField, Min(1)]
    private int maxActiveEnemies = 60;

    private readonly List<EnemyHealth> activeEnemies = new List<EnemyHealth>();

    private StageEnemyEntry[] configuredEnemyRoster;
    private float elapsedTime;

    private bool HasConfiguredRoster => configuredEnemyRoster != null && configuredEnemyRoster.Length > 0;
    private bool bossStage;
    private EnemyHealth bossPrefab;
    private float bossSpawnTime;
    private bool bossHasSpawned;

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (bossStage && !bossHasSpawned && bossPrefab != null && elapsedTime >= bossSpawnTime)
        {
            SpawnBoss();
        }
    }

    private void SpawnBoss()
    {
        bossHasSpawned = true;
        Vector2 spawnPosition = GetRandomArenaEdgePosition();

        EnemyHealth boss = Instantiate(
            bossPrefab,
            spawnPosition,
            Quaternion.identity,
            enemyParent
        );

        activeEnemies.Add(boss);
        Debug.Log("Boss Has Spawned!");
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            RemoveDestroyedEnemies();

            if (activeEnemies.Count < maxActiveEnemies)
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(
                GetCurrentSpawnInterval()
            );
        }
    }

    private void SpawnEnemy()
    {
        EnemyHealth selectedPrefab =
            SelectEnemyPrefab();

        if (selectedPrefab == null)
        {
            string message = HasConfiguredRoster
                ? "The stage enemy roster has no eligible " +
                  "enemy. Ensure at least one entry has an " +
                  "Unlock Time of 0, a Spawn Weight above 0, " +
                  "and an assigned enemy prefab."
                : "EnemySpawner is missing an enemy prefab.";

            Debug.LogError(message, this);
            return;
        }

        Vector2 spawnPosition =
            GetRandomArenaEdgePosition();

        EnemyHealth enemy = Instantiate(
            selectedPrefab,
            spawnPosition,
            Quaternion.identity,
            enemyParent
        );

        activeEnemies.Add(enemy);
    }

    private EnemyHealth SelectEnemyPrefab()
    {
        if (HasConfiguredRoster)
        {
            return SelectEnemyFromRoster();
        }

        return SelectLegacyEnemyPrefab();
    }

    private EnemyHealth SelectLegacyEnemyPrefab()
    {
        bool spiderIsUnlocked =
            elapsedTime >= spiderUnlockTime;

        bool shouldSpawnSpider =
            spiderIsUnlocked &&
            spiderPrefab != null &&
            Random.value < spiderSpawnChance;

        if (shouldSpawnSpider)
        {
            return spiderPrefab;
        }

        return ghostPrefab;
    }

    private EnemyHealth SelectEnemyFromRoster()
    {
        float totalWeight = 0f;

        for (int index = 0;
             index < configuredEnemyRoster.Length;
             index++)
        {
            StageEnemyEntry entry =
                configuredEnemyRoster[index];

            if (!IsEntryEligible(entry))
                continue;

            totalWeight += entry.SpawnWeight;
        }

        if (totalWeight <= 0f)
            return null;

        float randomValue =
            Random.Range(0f, totalWeight);

        EnemyHealth lastEligiblePrefab = null;

        for (int index = 0;
             index < configuredEnemyRoster.Length;
             index++)
        {
            StageEnemyEntry entry =
                configuredEnemyRoster[index];

            if (!IsEntryEligible(entry))
                continue;

            lastEligiblePrefab = entry.EnemyPrefab;
            randomValue -= entry.SpawnWeight;

            if (randomValue <= 0f)
            {
                return entry.EnemyPrefab;
            }
        }

        return lastEligiblePrefab;
    }

    private bool IsEntryEligible(
        StageEnemyEntry entry
    )
    {
        return entry != null &&
               entry.EnemyPrefab != null &&
               entry.SpawnWeight > 0f &&
               elapsedTime >= entry.UnlockTime;
    }

    private Vector2 GetRandomArenaEdgePosition()
    {
        Vector2 center = transform.position;
        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0:
                return center + new Vector2(
                    -arenaHalfSize.x,
                    Random.Range(
                        -arenaHalfSize.y,
                        arenaHalfSize.y
                    )
                );

            case 1:
                return center + new Vector2(
                    arenaHalfSize.x,
                    Random.Range(
                        -arenaHalfSize.y,
                        arenaHalfSize.y
                    )
                );

            case 2:
                return center + new Vector2(
                    Random.Range(
                        -arenaHalfSize.x,
                        arenaHalfSize.x
                    ),
                    -arenaHalfSize.y
                );

            default:
                return center + new Vector2(
                    Random.Range(
                        -arenaHalfSize.x,
                        arenaHalfSize.x
                    ),
                    arenaHalfSize.y
                );
        }
    }

    private float GetCurrentSpawnInterval()
    {
        float elapsedMinutes =
            elapsedTime / 60f;

        float currentInterval =
            initialSpawnInterval -
            intervalDecreasePerMinute *
            elapsedMinutes;

        return Mathf.Max(
            minimumSpawnInterval,
            currentInterval
        );
    }

    private void RemoveDestroyedEnemies()
    {
        activeEnemies.RemoveAll(
            enemy => enemy == null
        );
    }

    public void ConfigureStage(StageConfiguration configuration)
    {
        if (configuration == null)
        {
            Debug.LogError(
                "EnemySpawner received a null " +
                "stage configuration.",
                this
            );

            return;
        }

        arenaHalfSize = configuration.ArenaHalfSize;

        startDelay = configuration.StartDelay;

        initialSpawnInterval = configuration.InitialSpawnInterval;

        minimumSpawnInterval = configuration.MinimumSpawnInterval;

        intervalDecreasePerMinute = configuration.IntervalDecreasePerMinute;

        maxActiveEnemies = configuration.MaxActiveEnemies;

        spiderUnlockTime = configuration.SpiderUnlockTime;

        spiderSpawnChance = configuration.SpiderSpawnChance;

        configuredEnemyRoster = configuration.EnemyRoster;

        bossStage = configuration.BossStage;
        bossPrefab = configuration.BossPrefab; 
        bossSpawnTime = configuration.BossSpawnTime;
        bossHasSpawned = false;

        elapsedTime = 0f;

        string rosterDescription =
            HasConfiguredRoster
                ? $"{configuredEnemyRoster.Length} roster entries"
                : "legacy Ghost/Spider configuration";

        Debug.Log(
            $"EnemySpawner configured for Stage " +
            $"{configuration.StageId}. " +
            $"Using {rosterDescription}. " +
            $"Initial interval: " +
            $"{initialSpawnInterval:F2}."
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(
                arenaHalfSize.x * 2f,
                arenaHalfSize.y * 2f,
                0f
            )
        );
    }
}