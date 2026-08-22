using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField]
    private EnemyHealth ghostPrefab;

    [SerializeField]
    private EnemyHealth spiderPrefab;

    [SerializeField]
    private Transform enemyParent;

    [Header("Spider Unlock")]
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

    private readonly List<EnemyHealth> activeEnemies =
        new List<EnemyHealth>();

    private float elapsedTime;

    private void Update()
    {
        elapsedTime += Time.deltaTime;
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
            Debug.LogError(
                "EnemySpawner is missing an enemy prefab.",
                this
            );

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
        float elapsedMinutes = elapsedTime / 60f;

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

    public void ConfigureStage(
    StageConfiguration configuration
)
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

        arenaHalfSize =
            configuration.ArenaHalfSize;

        startDelay =
            configuration.StartDelay;

        initialSpawnInterval =
            configuration.InitialSpawnInterval;

        minimumSpawnInterval =
            configuration.MinimumSpawnInterval;

        intervalDecreasePerMinute =
            configuration.IntervalDecreasePerMinute;

        maxActiveEnemies =
            configuration.MaxActiveEnemies;

        spiderUnlockTime =
            configuration.SpiderUnlockTime;

        spiderSpawnChance =
            configuration.SpiderSpawnChance;

        elapsedTime = 0f;

        Debug.Log(
            $"EnemySpawner configured for Stage " +
            $"{configuration.StageId}. " +
            $"Initial interval: " +
            $"{initialSpawnInterval:F2}, " +
            $"spider chance: " +
            $"{spiderSpawnChance:F2}."
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